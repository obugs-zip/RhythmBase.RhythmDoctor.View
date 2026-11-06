using RhythmBase.RhythmDoctor.View.Compare.Cases;
using RhythmBase.RhythmDoctor.View.Compare.Rendering;
using SkiaSharp;

namespace RhythmBase.RhythmDoctor.View.Compare.Testing;

public static class SingleEventTest
{
	public static string Render(
		ParityCase item,
		int scale,
		int padding,
		bool active,
		bool hover,
		bool enabled,
		string outputDirectory)
	{
		CSharpRenderer.Initialize();
		SKSize measure = CSharpRenderer.Measure(item, scale);
		int width = Math.Max(1, (int)Math.Ceiling(measure.Width * scale) + 2 * padding);
		int height = Math.Max(1, (int)Math.Ceiling(measure.Height * scale) + 2 * padding);

		IconStyleConfig style = item.Style;
		style.Scale = scale;
		style.Active = active;
		style.Hover = hover;
		style.Enabled = enabled;

		CSharpRenderResult result = CSharpRenderer.Render(
			item,
			scale,
			width,
			height,
			padding,
			padding,
			new SKColor(0xFF2C2C2C));
		try
		{
			using SKCanvas canvas = new(result.Bitmap);
			canvas.DrawRect(result.IconRect, new SKPaint
			{
				Color = SKColors.Red,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 1,
			});

			Directory.CreateDirectory(outputDirectory);
			string path = Path.Combine(outputDirectory, $"single-{SafeName(item.Id)}.png");
			SavePng(result.Bitmap, path);
			return path;
		}
		finally
		{
			result.Bitmap.Dispose();
		}
	}

	private static string SafeName(string id)
	{
		char[] invalid = Path.GetInvalidFileNameChars();
		return string.Concat(id.Select(c => invalid.Contains(c) || c == '/' ? '_' : c));
	}

	private static void SavePng(SKBitmap bitmap, string path)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
		using FileStream stream = File.Create(path);
		data.SaveTo(stream);
	}
}
