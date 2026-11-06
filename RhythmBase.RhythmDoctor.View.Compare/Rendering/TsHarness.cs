using System.Text.Json;
using Microsoft.Playwright;
using RhythmBase.RhythmDoctor.View;

namespace RhythmBase.RhythmDoctor.View.Compare.Rendering;

public sealed record HarnessBounds(
	double Left,
	double Top,
	double Right,
	double Bottom,
	double Width,
	double Height);

public sealed record HarnessRenderResult(
	bool Ok,
	string? Error,
	HarnessBounds? Bounds,
	int? ChildCount);

public sealed class TsHarness : IAsyncDisposable
{
	public const string HarnessUrl = "http://rbview.parity/harness.html";

	private readonly string _webDist;
	private readonly IPlaywright _playwright;
	private readonly IBrowser _browser;
	private readonly IBrowserContext _context;
	private readonly IPage _page;

	private TsHarness(
		string webDist,
		IPlaywright playwright,
		IBrowser browser,
		IBrowserContext context,
		IPage page)
	{
		_webDist = webDist;
		_playwright = playwright;
		_browser = browser;
		_context = context;
		_page = page;
	}

	public static async Task<TsHarness> CreateAsync(string webDist, bool headful)
	{
		IPlaywright playwright = await Playwright.CreateAsync();
		IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
		{
			Headless = !headful,
			Args =
			[
				"--disable-gpu",
				"--force-color-profile=srgb",
				"--hide-scrollbars",
				"--mute-audio",
				"--font-render-hinting=none",
			],
		});
		IBrowserContext context = await browser.NewContextAsync(new BrowserNewContextOptions
		{
			DeviceScaleFactor = 1,
			ColorScheme = ColorScheme.Dark,
			ReducedMotion = ReducedMotion.Reduce,
			Locale = "en-US",
			TimezoneId = "UTC",
			ViewportSize = new ViewportSize { Width = 800, Height = 600 },
		});
		string root = Path.GetFullPath(webDist);
		await context.RouteAsync("**/*", async route =>
		{
			Uri uri = new(route.Request.Url);
			if (!string.Equals(uri.Host, "rbview.parity", StringComparison.OrdinalIgnoreCase))
			{
				await route.AbortAsync();
				return;
			}

			string relative = Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/');
			if (relative.Length == 0)
				relative = "harness.html";

			string fullPath = Path.GetFullPath(Path.Combine(
				root,
				relative.Replace('/', Path.DirectorySeparatorChar)));
			if (!fullPath.Equals(root, StringComparison.OrdinalIgnoreCase)
				&& !fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
			{
				await FulfillNotFound(route);
				return;
			}
			if (!File.Exists(fullPath))
			{
				await FulfillNotFound(route);
				return;
			}

			byte[] bytes = await File.ReadAllBytesAsync(fullPath);
			await route.FulfillAsync(new RouteFulfillOptions
			{
				Status = 200,
				BodyBytes = bytes,
				ContentType = ContentTypeOf(fullPath),
			});
		});

		IPage page = await context.NewPageAsync();
		await page.GotoAsync(HarnessUrl);
		await page.WaitForFunctionAsync(
			"window.__harnessReady === true",
			null,
			new PageWaitForFunctionOptions { Timeout = 30_000 });

		return new TsHarness(webDist, playwright, browser, context, page);
	}

	private static async Task FulfillNotFound(IRoute route)
		=> await route.FulfillAsync(new RouteFulfillOptions
		{
			Status = 404,
			ContentType = "text/plain",
			Body = "not found",
		});

	private static string ContentTypeOf(string path) => Path.GetExtension(path).ToLowerInvariant() switch
	{
		".html" => "text/html",
		".css" => "text/css",
		".js" => "text/javascript",
		".mjs" => "text/javascript",
		".json" => "application/json",
		".png" => "image/png",
		".woff2" => "font/woff2",
		".map" => "application/json",
		_ => "application/octet-stream",
	};

	public async Task<(HarnessRenderResult Result, IReadOnlyList<string> ConsoleErrors)> RenderAsync(
		string eventJson,
		IconStyleConfig style,
		int width,
		int height,
		int originX,
		int originY,
		string? screenshotPath)
	{
		await _page.SetViewportSizeAsync(width, height);
		JsonElement raw = await _page.EvaluateAsync<JsonElement>(
			@"request => window.renderCase({
				event: JSON.parse(request.eventJson),
				style: request.style,
				originX: request.originX,
				originY: request.originY,
			})",
			new
			{
				eventJson,
				style = new
				{
					scale = style.Scale,
					active = style.Active,
					hover = style.Hover,
					enabled = style.Enabled ?? false,
					showDuration = style.ShowDuration,
				},
				originX,
				originY,
			});
		HarnessRenderResult result = ParseResult(raw);

		if (result.Ok && screenshotPath is not null)
		{
			await _page.ScreenshotAsync(new PageScreenshotOptions
			{
				Path = screenshotPath,
				Type = ScreenshotType.Png,
				Clip = new Clip
				{
					X = 0,
					Y = 0,
					Width = width,
					Height = height,
				},
			});
		}

		string[] errors = await _page.EvaluateAsync<string[]>("() => window.__harnessErrors ?? []");
		return (result, errors);
	}

	private static HarnessRenderResult ParseResult(JsonElement raw)
	{
		bool ok = raw.TryGetProperty("ok", out JsonElement okElement)
			&& okElement.ValueKind == JsonValueKind.True;
		string? error = raw.TryGetProperty("error", out JsonElement errorElement)
			&& errorElement.ValueKind == JsonValueKind.String
			? errorElement.GetString()
			: null;
		HarnessBounds? bounds = null;
		if (raw.TryGetProperty("bounds", out JsonElement boundsElement)
			&& boundsElement.ValueKind == JsonValueKind.Object)
		{
			bounds = new HarnessBounds(
				boundsElement.GetProperty("left").GetDouble(),
				boundsElement.GetProperty("top").GetDouble(),
				boundsElement.GetProperty("right").GetDouble(),
				boundsElement.GetProperty("bottom").GetDouble(),
				boundsElement.GetProperty("width").GetDouble(),
				boundsElement.GetProperty("height").GetDouble());
		}
		int? childCount = raw.TryGetProperty("childCount", out JsonElement childElement)
			&& childElement.ValueKind == JsonValueKind.Number
			? childElement.GetInt32()
			: null;
		return new HarnessRenderResult(ok, error, bounds, childCount);
	}

	public async ValueTask DisposeAsync()
	{
		await _context.CloseAsync();
		await _browser.CloseAsync();
		_playwright.Dispose();
	}
}
