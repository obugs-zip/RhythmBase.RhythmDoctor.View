using System.Text.Json;
using System.Text.Json.Serialization;

namespace RhythmBase.RhythmDoctor.View.Compare.Diff;

public sealed record CaseReport
{
	public required string Id { get; init; }
	public required string Result { get; init; }
	public string? SkipReason { get; init; }
	public string? Error { get; init; }
	public int Width { get; init; }
	public int Height { get; init; }
	public int BadPixels { get; init; }
	public double SignificantPixelRatio { get; init; }
	public double MeanAbsoluteError { get; init; }
	public long MaxChannelDelta { get; init; }
	public bool Overflow { get; init; }
	public long ElapsedMilliseconds { get; init; }
	public string? ExpectedPath { get; init; }
	public string? ActualPath { get; init; }
	public string? DiffPath { get; init; }
	public IReadOnlyList<string> HarnessErrors { get; init; } = [];
}

public static class ReportWriter
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	};

	public static void Write(
		string outputDirectory,
		IReadOnlyList<CaseReport> cases,
		TimeSpan elapsed)
	{
		int passed = cases.Count(c => c.Result == "passed");
		int failed = cases.Count(c => c.Result == "failed");
		int skipped = cases.Count(c => c.Result == "skipped");

		var document = new
		{
			generatedAt = DateTimeOffset.UtcNow,
			elapsedSeconds = Math.Round(elapsed.TotalSeconds, 3),
			total = cases.Count,
			passed,
			failed,
			skipped,
			cases,
		};
		File.WriteAllText(
			Path.Combine(outputDirectory, "report.json"),
			JsonSerializer.Serialize(document, JsonOptions));

		List<string> lines =
		[
			"# RhythmBase.RhythmDoctor.View parity report",
			"",
			$"- Generated: {DateTimeOffset.UtcNow:u}",
			$"- Elapsed: {elapsed.TotalSeconds:F2}s",
			$"- Total: {cases.Count}",
			$"- Passed: {passed}",
			$"- Failed: {failed}",
			$"- Skipped: {skipped}",
			"",
			"- `bad%`: pixels whose maximum per-channel delta exceeds `channelDelta`.",
			"- `mae`: mean of the maximum per-pixel channel delta, in 0-255 units.",
			"- `diff.png` marks only pixels above `channelDelta`; sub-threshold differences still count in `mae` and remain black.",
			"",
		];

		if (failed > 0)
		{
			lines.Add("## Failures");
			lines.Add("");
			lines.Add("| case | bad% | mae | error |");
			lines.Add("|---|---:|---:|---|");
			foreach (CaseReport item in cases.Where(c => c.Result == "failed"))
			{
				lines.Add($"| `{Escape(item.Id)}` | {item.SignificantPixelRatio:P3} | {item.MeanAbsoluteError:F3} | {Escape(item.Error ?? string.Empty)} |");
			}
			lines.Add("");
		}

		if (skipped > 0)
		{
			lines.Add("## Skipped");
			lines.Add("");
			lines.Add("| case | reason |");
			lines.Add("|---|---|");
			foreach (CaseReport item in cases.Where(c => c.Result == "skipped"))
				lines.Add($"| `{Escape(item.Id)}` | {Escape(item.SkipReason ?? string.Empty)} |");
			lines.Add("");
		}

		lines.Add("## All cases");
		lines.Add("");
		lines.Add("| case | result | size | bad% | mae |");
		lines.Add("|---|---|---|---:|---:|");
		foreach (CaseReport item in cases)
		{
			string size = item.Width > 0 ? $"{item.Width}x{item.Height}" : "-";
			lines.Add($"| `{Escape(item.Id)}` | {item.Result} | {size} | {item.SignificantPixelRatio:P3} | {item.MeanAbsoluteError:F3} |");
		}
		lines.Add("");

		File.WriteAllLines(Path.Combine(outputDirectory, "report.md"), lines);
	}

	private static string Escape(string value) => value.Replace("|", "\\|");
}
