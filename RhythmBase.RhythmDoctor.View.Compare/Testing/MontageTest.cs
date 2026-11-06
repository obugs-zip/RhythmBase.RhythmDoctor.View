using RhythmBase.RhythmDoctor.View.Compare.Cases;
using RhythmBase.RhythmDoctor.View.Compare.Rendering;
using SkiaSharp;

namespace RhythmBase.RhythmDoctor.View.Compare.Testing;

public static class MontageTest
{
	public static string Render(
		IReadOnlyList<ParityCase> cases,
		int scale,
		string outputDirectory)
	{
		CSharpRenderer.Initialize();
		List<(ParityCase Case, SKSize Size)> items = cases
			.Where(item => !item.Id.Contains('/'))
			.Select(item => (item, CSharpRenderer.Measure(item, scale)))
			.ToList();
		if (items.Count == 0)
			throw new InvalidOperationException("No baseline cases were generated.");

		const int margin = 16;
		const int gap = 8;
		const int labelWidth = 240;
		int iconWidth = items.Max(item => (int)Math.Ceiling(item.Size.Width * scale));
		int width = margin * 2 + labelWidth + iconWidth + 256;
		int height = margin * 2 + items.Sum(item =>
			Math.Max((int)Math.Ceiling(item.Size.Height * scale), 14 * scale) + gap);

		using SKBitmap bitmap = new(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
		using SKCanvas canvas = new(bitmap);
		canvas.Clear(new SKColor(0xFF2C2C2C));

		using SKFont labelFont = new(SKTypeface.Default, 13);
		using SKPaint labelPaint = new() { Color = SKColors.White, IsAntialias = true };
		using SKPaint borderPaint = new()
		{
			Color = SKColors.Red,
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 1,
		};

		int y = margin;
		foreach ((ParityCase item, SKSize size) in items)
		{
			canvas.DrawText(item.Id, new SKPoint(margin, y + 13), SKTextAlign.Left, labelFont, labelPaint);

			IconStyleConfig style = item.Style;
			style.Scale = scale;
			SKRect rect = canvas.DrawEventIcon(item.Event, new SKPoint(margin + labelWidth, y), style);
			canvas.DrawRect(rect, borderPaint);

			int rowHeight = Math.Max((int)Math.Ceiling(size.Height * scale), (int)Math.Ceiling(rect.Height));
			y += rowHeight + gap;
		}

		Directory.CreateDirectory(outputDirectory);
		string path = Path.Combine(outputDirectory, "montage.png");
		SavePng(bitmap, path);
		return path;
	}

	private static void SavePng(SKBitmap bitmap, string path)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
		using FileStream stream = File.Create(path);
		data.SaveTo(stream);
	}
}
