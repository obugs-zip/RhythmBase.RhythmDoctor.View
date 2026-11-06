using RhythmBase.RhythmDoctor.Components;
using RhythmBase.RhythmDoctor.View;
using RhythmBase.RhythmDoctor.View.Compare.Cases;
using SkiaSharp;

namespace RhythmBase.RhythmDoctor.View.Compare.Rendering;

public sealed record CSharpRenderResult(
	SKBitmap Bitmap,
	SKRect IconRect,
	int Width,
	int Height,
	bool Overflow);

public static class CSharpRenderer
{
	public static void Initialize()
	{
		View.Palette = Chart.Default.ColorPalette;
		View.Conditionals = new Chart().Conditionals;
		View.Directory = Path.Combine(Path.GetTempPath(), "rbview-parity-assets");
		System.IO.Directory.CreateDirectory(View.Directory);
	}

	public static SKSize Measure(ParityCase @case, int scale)
		=> @case.Event.MeasureEventIcon(new IconStyle { Scale = scale });

	public static CSharpRenderResult Render(
		ParityCase @case,
		int scale,
		int width,
		int height,
		int originX,
		int originY,
		SKColor backgroundColor)
	{
		IconStyleConfig style = @case.Style;
		style.Scale = scale;

		SKBitmap bitmap = new(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
		using SKCanvas canvas = new(bitmap);
		canvas.Clear(backgroundColor);

		SKRect iconRect = canvas.DrawEventIcon(@case.Event, new SKPoint(originX, originY), style);
		bool overflow = iconRect.Left < 0 || iconRect.Top < 0
			|| iconRect.Right > width || iconRect.Bottom > height;

		return new(bitmap, iconRect, width, height, overflow);
	}

	public static SKRectI? ContentBounds(SKBitmap bitmap, SKColor background)
	{
		int left = bitmap.Width;
		int top = bitmap.Height;
		int right = -1;
		int bottom = -1;
		for (int y = 0; y < bitmap.Height; y++)
		{
			for (int x = 0; x < bitmap.Width; x++)
			{
				SKColor pixel = bitmap.GetPixel(x, y);
				if (pixel.Red == background.Red
					&& pixel.Green == background.Green
					&& pixel.Blue == background.Blue)
					continue;
				left = Math.Min(left, x);
				top = Math.Min(top, y);
				right = Math.Max(right, x);
				bottom = Math.Max(bottom, y);
			}
		}
		if (right < 0)
			return null;
		return new SKRectI(left, top, right + 1, bottom + 1);
	}
}
