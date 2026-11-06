using System.Diagnostics;
using RhythmBase.RhythmDoctor.View.Compare.Cases;
using RhythmBase.RhythmDoctor.View.Compare.Diff;
using RhythmBase.RhythmDoctor.View.Compare.Rendering;
using RhythmBase.RhythmDoctor.View.Compare.Testing;
using SkiaSharp;

namespace RhythmBase.RhythmDoctor.View.Compare;

internal static class Program
{
	private static async Task<int> Main(string[] args)
	{
		try
		{
			return await RunAsync(args);
		}
		catch (Exception exception)
		{
			Console.Error.WriteLine(exception);
			return 2;
		}
	}

	private static async Task<int> RunAsync(string[] args)
	{
		Options options = Options.Parse(args);
		if (options.Help)
		{
			Console.WriteLine(Options.Usage);
			return 0;
		}
		if (options.SelfTest)
			return RunSelfTest();

		string repoRoot = FindRepoRoot();
		string webDist = ResolvePath(options.WebDist ?? Path.Combine(repoRoot, "web", "dist"));
		string outputDirectory = ResolvePath(options.OutDir ?? Path.Combine(repoRoot, "artifacts", "parity"));
		string configPath = ResolvePath(options.ConfigPath ?? Path.Combine(repoRoot, "RhythmBase.RhythmDoctor.View.Compare", "parity.config.json"));

		if (options.Montage || options.Single is not null || options.Timeline is not null)
			return RunTestModes(options, outputDirectory);

		if (!options.NoBuild)
		{
			int buildExitCode = RunNpmBuild(repoRoot);
			if (buildExitCode != 0)
			{
				Console.Error.WriteLine($"npm build failed with exit code {buildExitCode}.");
				return 2;
			}
		}

		if (!File.Exists(Path.Combine(webDist, "harness.html")))
		{
			Console.Error.WriteLine($"harness.html was not found in '{webDist}'. Run 'npm --prefix web run build' or drop --no-build.");
			return 2;
		}

		DiffConfig config = DiffConfig.Load(configPath);
		IReadOnlyList<ParityCase> catalog = CaseCatalog.Create(options.Scale);
		List<ParityCase> selected = catalog
			.Where(item => options.Filter is null
				|| item.Id.Contains(options.Filter, StringComparison.OrdinalIgnoreCase))
			.ToList();

		if (options.List)
		{
			foreach (ParityCase item in selected)
			{
				Console.WriteLine(item.SkipReason is null
					? item.Id
					: $"{item.Id} (skipped: {item.SkipReason})");
			}
			return 0;
		}

		if (selected.Count == 0)
		{
			Console.Error.WriteLine($"No cases matched filter '{options.Filter}'.");
			return 1;
		}

		Directory.CreateDirectory(outputDirectory);
		CSharpRenderer.Initialize();

		Console.WriteLine($"Running {selected.Count} cases at scale {options.Scale}, padding {options.Padding}.");

		Stopwatch totalWatch = Stopwatch.StartNew();
		List<CaseReport> reports = [];
		await using TsHarness harness = await CreateHarnessAsync(webDist, options);

		foreach (ParityCase item in selected)
		{
			if (item.SkipReason is not null)
			{
				reports.Add(new CaseReport
				{
					Id = item.Id,
					Result = "skipped",
					SkipReason = item.SkipReason,
				});
				continue;
			}

			CaseReport report = await RunCaseAsync(item, options, config, outputDirectory, harness);
			reports.Add(report);
			PrintRow(report);
		}

		totalWatch.Stop();
		ReportWriter.Write(outputDirectory, reports, totalWatch.Elapsed);

		int failed = reports.Count(item => item.Result == "failed");
		int skipped = reports.Count(item => item.Result == "skipped");
		int passed = reports.Count(item => item.Result == "passed");
		Console.WriteLine();
		Console.WriteLine($"Passed {passed}, failed {failed}, skipped {skipped}. Report: {Path.Combine(outputDirectory, "report.md")}");
		return failed == 0 ? 0 : 1;
	}

	private static async Task<TsHarness> CreateHarnessAsync(string webDist, Options options)
	{
		if (options.InstallBrowser)
			Microsoft.Playwright.Program.Main(["install", "chromium"]);

		try
		{
			return await TsHarness.CreateAsync(webDist, options.Headful);
		}
		catch (Microsoft.Playwright.PlaywrightException exception)
		{
			string script = Path.Combine(AppContext.BaseDirectory, "playwright.ps1");
			Console.Error.WriteLine(exception.Message);
			Console.Error.WriteLine();
			Console.Error.WriteLine("Chromium may not be installed. Install it with:");
			Console.Error.WriteLine($"  pwsh \"{script}\" install chromium");
			Console.Error.WriteLine("or rerun this tool with --install-browser.");
			throw;
		}
	}

	private static int RunTestModes(Options options, string outputDirectory)
	{
		Directory.CreateDirectory(outputDirectory);
		IReadOnlyList<ParityCase> cases = CaseCatalog.Create(options.Scale);

		if (options.Montage)
		{
			string path = MontageTest.Render(cases, options.Scale, outputDirectory);
			Console.WriteLine($"Montage written to {path}");
		}

		if (options.Single is { } single)
		{
			ParityCase? item = cases.FirstOrDefault(c =>
					string.Equals(c.Id, single, StringComparison.OrdinalIgnoreCase))
				?? cases.FirstOrDefault(c =>
					string.Equals(c.Event.Type.ToString(), single, StringComparison.OrdinalIgnoreCase));
			if (item is null)
			{
				Console.Error.WriteLine($"Unknown event type '{single}'. Use --list to see case ids.");
				return 1;
			}
			string path = SingleEventTest.Render(
				item,
				options.Scale,
				options.Padding,
				options.Active,
				options.Hover,
				!options.Disabled,
				outputDirectory);
			Console.WriteLine($"Single event written to {path}");
		}

		if (options.Timeline is { } timeline)
		{
			string path = ResolvePath(timeline);
			TimelineResult result = TimelineTest.Render(path, options.Scale, outputDirectory);
			Console.WriteLine(
				$"Timeline written to {result.Path} ({result.EventCount} events, {result.Width}x{result.Height})");
		}

		return 0;
	}

	private static async Task<CaseReport> RunCaseAsync(
		ParityCase item,
		Options options,
		DiffConfig config,
		string outputDirectory,
		TsHarness harness)
	{
		Stopwatch watch = Stopwatch.StartNew();
		string caseDirectory = Path.Combine(outputDirectory, SafeName(item.Id));
		Directory.CreateDirectory(caseDirectory);
		string expectedPath = Path.Combine(caseDirectory, "expected.png");
		string actualPath = Path.Combine(caseDirectory, "actual.png");
		string diffPath = Path.Combine(caseDirectory, "diff.png");

		CSharpRenderResult? csharp = null;
		try
		{
			string json = EventJsonProjector.Project(item);
			SKSize measure = CSharpRenderer.Measure(item, options.Scale);
			int measureWidth = Math.Max(1, (int)Math.Ceiling(measure.Width * options.Scale));
			int measureHeight = Math.Max(1, (int)Math.Ceiling(measure.Height * options.Scale));

			(HarnessRenderResult provisional, _) = await harness.RenderAsync(
				json,
				item.Style,
				measureWidth + 2 * options.Padding + 256,
				measureHeight + 2 * options.Padding + 256,
				options.Padding,
				options.Padding,
				null);
			if (!provisional.Ok)
			{
				return new CaseReport
				{
					Id = item.Id,
					Result = "failed",
					Error = provisional.Error ?? "TS harness returned ok=false.",
					ElapsedMilliseconds = watch.ElapsedMilliseconds,
				};
			}

			int localLeft = 0;
			int localTop = 0;
			int localRight = measureWidth;
			int localBottom = measureHeight;
			if (provisional.Bounds is not null)
			{
				localLeft = (int)Math.Floor(provisional.Bounds.Left - options.Padding);
				localTop = (int)Math.Floor(provisional.Bounds.Top - options.Padding);
				localRight = (int)Math.Ceiling(provisional.Bounds.Right - options.Padding);
				localBottom = (int)Math.Ceiling(provisional.Bounds.Bottom - options.Padding);
			}
			int minX = Math.Min(0, localLeft);
			int minY = Math.Min(0, localTop);
			int maxX = Math.Max(measureWidth, localRight);
			int maxY = Math.Max(measureHeight, localBottom);
			int width = Math.Max(1, maxX - minX + 2 * options.Padding);
			int height = Math.Max(1, maxY - minY + 2 * options.Padding);
			int originX = options.Padding - minX;
			int originY = options.Padding - minY;

			csharp = CSharpRenderer.Render(item, options.Scale, width, height, originX, originY, config.Background);
			SavePng(csharp.Bitmap, expectedPath);

			if (csharp.Overflow)
			{
				return new CaseReport
				{
					Id = item.Id,
					Result = "failed",
					Error = "C# icon rectangle overflowed the canvas; increase --padding.",
					Width = csharp.Width,
					Height = csharp.Height,
					Overflow = true,
					ExpectedPath = expectedPath,
					ElapsedMilliseconds = watch.ElapsedMilliseconds,
				};
			}

			SKRectI? content = CSharpRenderer.ContentBounds(csharp.Bitmap, config.Background);
			if (content is not null
				&& (content.Value.Left <= 0
					|| content.Value.Top <= 0
					|| content.Value.Right >= csharp.Width
					|| content.Value.Bottom >= csharp.Height))
			{
				return new CaseReport
				{
					Id = item.Id,
					Result = "failed",
					Error = $"C# content bounds {content} touched the canvas edge {csharp.Width}x{csharp.Height}; increase --padding.",
					Width = csharp.Width,
					Height = csharp.Height,
					Overflow = true,
					ExpectedPath = expectedPath,
					ElapsedMilliseconds = watch.ElapsedMilliseconds,
				};
			}

			(HarnessRenderResult result, IReadOnlyList<string> errors) = await harness.RenderAsync(
				json,
				item.Style,
				csharp.Width,
				csharp.Height,
				originX,
				originY,
				actualPath);

			if (!result.Ok)
			{
				return new CaseReport
				{
					Id = item.Id,
					Result = "failed",
					Error = result.Error ?? "TS harness returned ok=false.",
					Width = csharp.Width,
					Height = csharp.Height,
					ExpectedPath = expectedPath,
					HarnessErrors = errors,
					ElapsedMilliseconds = watch.ElapsedMilliseconds,
				};
			}

			bool boundsOverflow = result.Bounds is not null
				&& (result.Bounds.Left < 0
					|| result.Bounds.Top < 0
					|| result.Bounds.Right > csharp.Width
					|| result.Bounds.Bottom > csharp.Height);
			if (boundsOverflow)
			{
				return new CaseReport
				{
					Id = item.Id,
					Result = "failed",
					Error = $"TS bounds {result.Bounds} overflowed {csharp.Width}x{csharp.Height}.",
					Width = csharp.Width,
					Height = csharp.Height,
					Overflow = true,
					ExpectedPath = expectedPath,
					ActualPath = actualPath,
					HarnessErrors = errors,
					ElapsedMilliseconds = watch.ElapsedMilliseconds,
				};
			}

			using SKBitmap actual = SKBitmap.Decode(actualPath)
				?? throw new InvalidOperationException($"Could not decode '{actualPath}'.");
			DiffThresholds thresholds = config.ThresholdsFor(item.Id);
			DiffResult diff = ImageDiff.Compare(
				csharp.Bitmap,
				actual,
				thresholds,
				config.Background,
				config.BorderSamplePixels);
			if (diff.DiffBitmap is not null)
			{
				SavePng(diff.DiffBitmap, diffPath);
				diff.DiffBitmap.Dispose();
			}

			bool passed = diff.Passed(thresholds);
			watch.Stop();
			return new CaseReport
			{
				Id = item.Id,
				Result = passed ? "passed" : "failed",
				Error = passed ? null : DescribeDiff(diff, thresholds),
				Width = csharp.Width,
				Height = csharp.Height,
				BadPixels = diff.BadPixels,
				SignificantPixelRatio = diff.SignificantRatio,
				MeanAbsoluteError = diff.MeanAbsoluteError,
				MaxChannelDelta = diff.MaxChannelDelta,
				ExpectedPath = expectedPath,
				ActualPath = actualPath,
				DiffPath = diff.SizeMatches ? diffPath : null,
				HarnessErrors = errors,
				ElapsedMilliseconds = watch.ElapsedMilliseconds,
			};
		}
		catch (Exception exception)
		{
			watch.Stop();
			return new CaseReport
			{
				Id = item.Id,
				Result = "failed",
				Error = exception.ToString(),
				ExpectedPath = File.Exists(expectedPath) ? expectedPath : null,
				ActualPath = File.Exists(actualPath) ? actualPath : null,
				ElapsedMilliseconds = watch.ElapsedMilliseconds,
			};
		}
		finally
		{
			csharp?.Bitmap.Dispose();
		}
	}

	private static string DescribeDiff(DiffResult diff, DiffThresholds thresholds)
	{
		if (!diff.SizeMatches)
			return "Image sizes differ.";
		if (!diff.BackgroundMatches)
			return "Edge pixels are not the expected background color.";
		return $"bad ratio {diff.SignificantRatio:P3} (limit {thresholds.SignificantPixelRatio:P3}), "
			+ $"mae {diff.MeanAbsoluteError:F3} (limit {thresholds.MeanAbsoluteError:F3}).";
	}

	private static void SavePng(SKBitmap bitmap, string path)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
		using FileStream stream = File.Create(path);
		data.SaveTo(stream);
	}

	private static void PrintRow(CaseReport report)
	{
		if (report.Result == "skipped")
		{
			Console.WriteLine($"{report.Id,-52} skipped");
			return;
		}
		string size = report.Width > 0 ? $"{report.Width}x{report.Height}" : "-";
		Console.WriteLine($"{report.Id,-52} {size,9} {report.SignificantPixelRatio,8:P3} {report.MeanAbsoluteError,8:F3} {report.Result}");
	}

	private static int RunNpmBuild(string repoRoot)
	{
		ProcessStartInfo startInfo = new()
		{
			WorkingDirectory = repoRoot,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
		};
		if (OperatingSystem.IsWindows())
		{
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = "/c npm --prefix web run build";
		}
		else
		{
			startInfo.FileName = "npm";
			startInfo.Arguments = "--prefix web run build";
		}

		using Process process = Process.Start(startInfo)
			?? throw new InvalidOperationException("Could not start npm.");
		string stdout = process.StandardOutput.ReadToEnd();
		string stderr = process.StandardError.ReadToEnd();
		process.WaitForExit();
		if (!string.IsNullOrWhiteSpace(stdout))
			Console.WriteLine(stdout.TrimEnd());
		if (!string.IsNullOrWhiteSpace(stderr))
			Console.Error.WriteLine(stderr.TrimEnd());
		return process.ExitCode;
	}

	private static int RunSelfTest()
	{
		DiffThresholds thresholds = new(24, 0.005, 2.0);
		SKBitmap identicalA = Solid(10, 10, SKColors.Red);
		SKBitmap identicalB = Solid(10, 10, SKColors.Red);
		SKBitmap minorA = Solid(10, 10, SKColors.Red);
		SKBitmap minorB = Solid(10, 10, SKColors.Red);
		minorB.SetPixel(5, 5, new SKColor(255, 5, 0));
		SKBitmap largeA = Solid(10, 10, SKColors.Red);
		SKBitmap largeB = Solid(10, 10, SKColors.Blue);
		SKBitmap sizeA = Solid(10, 10, SKColors.Red);
		SKBitmap sizeB = Solid(11, 10, SKColors.Red);

		bool identicalPass = ImageDiff.Compare(identicalA, identicalB, thresholds, SKColors.Red, 2).Passed(thresholds);
		bool minorPass = ImageDiff.Compare(minorA, minorB, thresholds, SKColors.Red, 2).Passed(thresholds);
		bool largePass = ImageDiff.Compare(largeA, largeB, thresholds, SKColors.Red, 2).Passed(thresholds);
		DiffResult sizeDiff = ImageDiff.Compare(sizeA, sizeB, thresholds, SKColors.Red, 2);

		foreach (SKBitmap bitmap in new[] { identicalA, identicalB, minorA, minorB, largeA, largeB, sizeA, sizeB })
			bitmap.Dispose();

		bool ok = identicalPass && minorPass && !largePass && !sizeDiff.SizeMatches;
		Console.WriteLine($"identical: {(identicalPass ? "PASS" : "FAIL")} (expected PASS)");
		Console.WriteLine($"minor-delta: {(minorPass ? "PASS" : "FAIL")} (expected PASS)");
		Console.WriteLine($"large-delta: {(largePass ? "PASS" : "FAIL")} (expected FAIL)");
		Console.WriteLine($"size-mismatch: {(sizeDiff.SizeMatches ? "PASS" : "FAIL")} (expected FAIL)");
		Console.WriteLine(ok ? "self-test passed." : "self-test failed.");
		return ok ? 0 : 2;
	}

	private static SKBitmap Solid(int width, int height, SKColor color)
	{
		SKBitmap bitmap = new(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
				bitmap.SetPixel(x, y, color);
		}
		return bitmap;
	}

	private static string SafeName(string id)
	{
		char[] invalid = Path.GetInvalidFileNameChars();
		return string.Concat(id.Select(c => invalid.Contains(c) || c == '/' ? '_' : c));
	}

	private static string ResolvePath(string path)
		=> Path.IsPathRooted(path) ? path : Path.GetFullPath(path);

	private static string FindRepoRoot()
	{
		DirectoryInfo? directory = new(AppContext.BaseDirectory);
		while (directory is not null)
		{
			if (File.Exists(Path.Combine(directory.FullName, "RhythmBase.RhythmDoctor.View.slnx")))
				return directory.FullName;
			directory = directory.Parent;
		}
		return Directory.GetCurrentDirectory();
	}

	private sealed class Options
	{
		public string? WebDist { get; init; }
		public string? OutDir { get; init; }
		public int Scale { get; init; } = 2;
		public int Padding { get; init; } = 32;
		public string? Filter { get; init; }
		public bool List { get; init; }
		public bool Headful { get; init; }
		public bool NoBuild { get; init; }
		public bool InstallBrowser { get; init; }
		public bool SelfTest { get; init; }
		public bool Montage { get; init; }
		public string? Single { get; init; }
		public string? Timeline { get; init; }
		public bool Active { get; init; }
		public bool Hover { get; init; }
		public bool Disabled { get; init; }
		public string? ConfigPath { get; init; }
		public bool Help { get; init; }

		public static string Usage => """
			Usage: RhythmBase.RhythmDoctor.View.Compare [options]

			  --web-dist <dir>     web dist directory (default: web/dist)
			  --out <dir>          artifact directory (default: artifacts/parity)
			  --scale <n>          icon scale (default: 2)
			  --padding <n>        canvas padding (default: 32)
			  --filter <text>      only run parity cases whose id contains text
			  --list               list cases without rendering
			  --headful            show the browser window
			  --no-build           skip npm build
			  --install-browser    install Playwright Chromium before running
			  --self-test          validate the image diff algorithm
			  --config <path>      parity config file
			  --montage            render every baseline event type into montage.png
			  --single <type>      render one event type into single-<type>.png
			  --timeline <file>    render a .Level/.rdlevel/.rdzip chart timeline
			  --active             draw --single with Active=true
			  --hover              draw --single with Hover=true
			  --disabled           draw --single with Enabled=false
			  --help               show this help
			""";

		public static Options Parse(string[] args)
		{
			Options defaults = new();
			string? webDist = defaults.WebDist;
			string? outDir = defaults.OutDir;
			int scale = defaults.Scale;
			int padding = defaults.Padding;
			string? filter = null;
			bool list = false;
			bool headful = false;
			bool noBuild = false;
			bool installBrowser = false;
			bool selfTest = false;
			bool montage = false;
			string? single = null;
			string? timeline = null;
			bool active = false;
			bool hover = false;
			bool disabled = false;
			string? configPath = null;
			bool help = false;

			for (int i = 0; i < args.Length; i++)
			{
				string argument = args[i];
				switch (argument)
				{
					case "--web-dist":
						webDist = Next(args, ref i, argument);
						break;
					case "--out":
						outDir = Next(args, ref i, argument);
						break;
					case "--scale":
						scale = int.Parse(Next(args, ref i, argument));
						break;
					case "--padding":
						padding = int.Parse(Next(args, ref i, argument));
						break;
					case "--filter":
						filter = Next(args, ref i, argument);
						break;
					case "--list":
						list = true;
						break;
					case "--headful":
						headful = true;
						break;
					case "--no-build":
						noBuild = true;
						break;
					case "--install-browser":
						installBrowser = true;
						break;
					case "--self-test":
						selfTest = true;
						break;
					case "--montage":
						montage = true;
						break;
					case "--single":
						single = Next(args, ref i, argument);
						break;
					case "--timeline":
						timeline = Next(args, ref i, argument);
						break;
					case "--active":
						active = true;
						break;
					case "--hover":
						hover = true;
						break;
					case "--disabled":
						disabled = true;
						break;
					case "--config":
						configPath = Next(args, ref i, argument);
						break;
					case "--help":
					case "-h":
						help = true;
						break;
					default:
						throw new ArgumentException($"Unknown argument '{argument}'. Use --help for usage.");
				}
			}

			return new Options
			{
				WebDist = webDist,
				OutDir = outDir,
				Scale = scale,
				Padding = padding,
				Filter = filter,
				List = list,
				Headful = headful,
				NoBuild = noBuild,
				InstallBrowser = installBrowser,
				SelfTest = selfTest,
				Montage = montage,
				Single = single,
				Timeline = timeline,
				Active = active,
				Hover = hover,
				Disabled = disabled,
				ConfigPath = configPath,
				Help = help,
			};
		}

		private static string Next(string[] args, ref int index, string name)
		{
			if (index + 1 >= args.Length)
				throw new ArgumentException($"Missing value for {name}.");
			return args[++index];
		}
	}
}
