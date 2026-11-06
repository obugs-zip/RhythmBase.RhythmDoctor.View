using RhythmBase.Global.Components;
using RhythmBase.RhythmDoctor;
using RhythmBase.RhythmDoctor.Components;
using RhythmBase.RhythmDoctor.Events;
using RhythmBase.RhythmDoctor.Extensions;
using RhythmBase.RhythmDoctor.View;
using SkiaSharp;

namespace RhythmBase.RhythmDoctor.View.Compare.Testing;

public sealed record TimelineResult(string Path, int EventCount, int Width, int Height);

public static class TimelineTest
{
	private readonly record struct YRange(int Min, int Max);

	private sealed record DrawData
	{
		public int UnitSize { get; init; } = 14;
		public int PixelSize { get; init; } = 2;
		public required IReadOnlyList<Tab> Tabs { get; init; }
		public required TickTime Start { get; init; }
		public required TickTime End { get; init; }
		public required Dictionary<Tab, YRange> TabYRanges { get; init; }
		public required Dictionary<Tab, int> TabYOffsets { get; init; }
		public required int CurrentY { get; init; }
		public required IReadOnlyList<KeyValuePair<TickTime, float>> TempoChanges { get; init; }
		public required float? StartBpm { get; init; }
	}

	public static TimelineResult Render(string file, int pixelSize, string outputDirectory)
	{
		using IDisposable? owner = OpenChart(file, out Chart chart);
		try
		{
			View.Directory = chart.ResolvedDirectory ?? string.Empty;
			View.Palette = chart.ColorPalette;
			View.Conditionals = chart.Conditionals;

			const int unitSize = 14;
			List<Tab> tabs = [Tab.Sounds, Tab.Rows, Tab.Actions, Tab.Decorations, Tab.Rooms, Tab.Windows];

			TickTime start = new(chart.Calculator, 1, 1);
			TickTime end = chart.Duration;

			SKFontManager manager = SKFontManager.Default;
			SKTypeface? typeface = manager.MatchFamily(
				"DinkieBitmap 7px",
				new SKFontStyle(400, 5, SKFontStyleSlant.Upright));

			List<IBaseEvent> events = [];
			float? startBpm = null;
			SortedDictionary<TickTime, float> tempoChanges = [];
			Dictionary<Tab, YRange> tabYRanges = [];
			foreach (IBaseEvent e in chart.InRange(start, end + 1))
			{
				startBpm ??= e.TickTime.Bpm;
				if (e is BaseBeatsPerMinute bpm)
				{
					tempoChanges.Remove(bpm.TickTime);
					tempoChanges.Add(bpm.TickTime, bpm.BeatsPerMinute);
				}
				if (!tabs.Contains(e.Tab))
					continue;

				if (e.Tab is Tab.Rooms)
					tabYRanges[Tab.Rooms] = new YRange(0, 3);
				if (e.Tab is Tab.Windows)
				{
					int maxY = 4;
					if (tabYRanges.TryGetValue(Tab.Windows, out YRange windowRange))
						maxY = int.Max(windowRange.Max, 4);
					tabYRanges[Tab.Windows] = new YRange(0, int.Max(maxY, e.Y));
				}

				if (tabYRanges.TryGetValue(e.Tab, out YRange range))
					tabYRanges[e.Tab] = new YRange(Math.Min(range.Min, e.Y), Math.Max(range.Max, e.Y));
				else
					tabYRanges[e.Tab] = new YRange(e.Y, e.Y);

				events.Add(e);
			}

			if (events.Count == 0)
				throw new InvalidOperationException($"No renderable events were found in '{file}'.");

			Dictionary<Tab, int> tabYOffsets = [];
			int currentY = 0;
			foreach (Tab tab in tabs)
			{
				if (!tabYRanges.TryGetValue(tab, out YRange range))
					continue;
				tabYOffsets[tab] = currentY;
				currentY += range.Max - range.Min + 1;
			}
			currentY -= 1;

			start = events.First().TickTime.QuantizeBeat(1);
			end = events.Last().TickTime.QuantizeBeat(1) + 1;

			DrawData drawData = new()
			{
				Start = start,
				End = end,
				Tabs = tabs,
				TabYOffsets = tabYOffsets,
				TabYRanges = tabYRanges,
				CurrentY = currentY,
				UnitSize = unitSize,
				PixelSize = pixelSize,
				TempoChanges = [.. tempoChanges],
				StartBpm = startBpm,
			};

			SKImageInfo info = new(
				(int)(end.Tick - start.Tick + 2) * pixelSize * unitSize,
				(currentY + 3) * pixelSize * unitSize);
			using SKBitmap surface = new(info);
			using SKCanvas canvas = new(surface);
			DrawBackground(drawData, chart, info, typeface, canvas);

			foreach (IBaseEvent e in events)
			{
				canvas.DrawEventIcon(e, ToLocation(e, start.Tick, tabYRanges[e.Tab].Min - tabYOffsets[e.Tab]), new()
				{
					Scale = drawData.PixelSize,
					ShowDuration = false,
					Active = false,
					Hover = e is AddClassicBeat,
					Enabled = e.Active,
				});
			}

			Directory.CreateDirectory(outputDirectory);
			string name = Path.GetFileNameWithoutExtension(file);
			string path = Path.Combine(outputDirectory, "timeline", $"{name}_timeline.png");
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			using SKImage image = SKImage.FromBitmap(surface);
			using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
			using FileStream stream = File.Create(path);
			data.SaveTo(stream);
			return new TimelineResult(path, events.Count, info.Width, info.Height);
		}
		finally
		{
			if (owner is null)
				chart.Dispose();
		}
	}

	private static IDisposable? OpenChart(string file, out Chart chart)
	{
		string extension = Path.GetExtension(file);
		if (extension is ".rdzip" or ".zip")
		{
			Level level = Level.FromZip(file);
			chart = level.MainChart;
			return level;
		}
		chart = Chart.FromFile(file);
		return null;
	}

	private static void DrawBackground(
		DrawData drawData,
		Chart chart,
		SKImageInfo info,
		SKTypeface? typeface,
		SKCanvas canvas)
	{
		int unitSize = drawData.UnitSize;
		int pixelSize = drawData.PixelSize;
		canvas.Translate(pixelSize * unitSize, pixelSize * unitSize);
		canvas.Clear(0xff2C2C2C);
		canvas.Save();
		canvas.RotateDegrees(-90);
		canvas.DrawText(
			$"{chart.Settings.Artist} - {chart.Settings.Song} by {chart.Settings.Author}",
			pixelSize,
			-pixelSize * 2,
			SKTextAlign.Right,
			new SKFont(typeface, 24),
			new SKPaint { Color = 0xffffffff, Style = SKPaintStyle.Fill });
		canvas.Restore();

		int index = 0;
		List<KeyValuePair<TickTime, float>> tempoList = [.. drawData.TempoChanges];
		if (tempoList.Count == 0 || tempoList[0].Key != drawData.Start)
			tempoList.Insert(0, new KeyValuePair<TickTime, float>(drawData.Start, drawData.StartBpm ?? 0));
		foreach (KeyValuePair<TickTime, float> tempo in tempoList)
		{
			int x = (int)((tempo.Key.Tick - drawData.Start.Tick) * pixelSize * unitSize);
			using SKPaint paint = new() { Color = 0xff7a7a7a, Style = SKPaintStyle.Fill };
			canvas.DrawText($"{tempo.Value} BPM", x + pixelSize, -pixelSize - 12, SKTextAlign.Left, new SKFont(typeface), paint);
			if (index > 0 && index % 2 == 0)
			{
				KeyValuePair<TickTime, float> previous = tempoList[index - 1];
				int previousX = (int)((previous.Key.Tick - drawData.Start.Tick) * pixelSize * unitSize);
				canvas.DrawRect(previousX, 0, x - previousX, (drawData.CurrentY + 1) * pixelSize * unitSize, new SKPaint
				{
					Color = 0x227a7a7a,
					Style = SKPaintStyle.Fill,
				});
			}
			index++;
		}

		for (TickTime beat = drawData.Start; beat < drawData.End; beat += 1)
		{
			int x = (int)((beat.Tick - drawData.Start.Tick) * pixelSize * unitSize);
			(int bar, float beatIndex) = beat;
			uint color = beatIndex == 1 ? 0xff7a7a7a : 0xff3C3C3C;
			using SKPaint paint = new()
			{
				Color = color,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = pixelSize,
			};
			canvas.DrawLine(x, 0, x, (drawData.CurrentY + 1) * pixelSize * unitSize, paint);
			if (beatIndex == 1)
				canvas.DrawText(bar.ToString(), x + pixelSize, -pixelSize, SKTextAlign.Left, new SKFont(typeface, 24), paint);
		}

		foreach (RhythmBase.RhythmDoctor.Components.Bookmark bookmark in chart.Bookmarks)
		{
			TickTime b = bookmark.Tick.WithLink(chart);
			int x = (int)((b.Tick - drawData.Start.Tick) * pixelSize * unitSize);
			uint color = bookmark.Color.ToColor();
			using SKPaint paint = new()
			{
				Style = SKPaintStyle.Fill,
				Shader = SKShader.CreateLinearGradient(
					new SKPoint(x, 0),
					new SKPoint(x + pixelSize * unitSize, 0),
					[new SKColor(color).WithAlpha(192), new SKColor(color).WithAlpha(0)],
					null,
					SKShaderTileMode.Clamp),
			};
			canvas.DrawRect(x, 0, pixelSize * unitSize, (drawData.CurrentY + 1) * pixelSize * unitSize, paint);
		}

		foreach (Tab tab in drawData.Tabs)
		{
			if (!drawData.TabYRanges.TryGetValue(tab, out YRange range))
				continue;
			using SKPaint paint = new()
			{
				Color = View.ColorOf(tab).WithAlpha(32),
				Style = SKPaintStyle.Fill,
			};
			int y = (drawData.TabYOffsets[tab] + range.Min) * pixelSize * unitSize;
			int height = (range.Max - range.Min + 1) * pixelSize * unitSize;
			canvas.DrawRect(0, y, info.Width, height, paint);
		}
	}

	private static SKPointI ToLocation(IBaseEvent e, float left, int top)
	{
		return new SKPointI(
			(int)((e.TickTime.Tick - left) * 28),
			((e is ReorderRooms or ShowRooms ? 0 : e.Y) - top) * 28);
	}
}
