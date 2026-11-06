using SkiaSharp;

namespace RhythmBase.RhythmDoctor.View.Compare.Diff;

public sealed record DiffThresholds(
	int ChannelDelta,
	double SignificantPixelRatio,
	double MeanAbsoluteError);

public sealed record DiffResult(
	bool SizeMatches,
	bool BackgroundMatches,
	int BadPixels,
	int TotalPixels,
	double SignificantRatio,
	double MeanAbsoluteError,
	long MaxChannelDelta,
	SKBitmap? DiffBitmap)
{
	public bool Passed(DiffThresholds thresholds)
		=> SizeMatches
			&& BackgroundMatches
			&& SignificantRatio <= thresholds.SignificantPixelRatio
			&& MeanAbsoluteError <= thresholds.MeanAbsoluteError;
}

public static class ImageDiff
{
	public static DiffResult Compare(
		SKBitmap expected,
		SKBitmap actual,
		DiffThresholds thresholds,
		SKColor backgroundColor,
		int borderSamplePixels)
	{
		if (expected.Width != actual.Width || expected.Height != actual.Height)
			return new(false, false, 0, 0, 0, 0, 0, null);

		int width = expected.Width;
		int height = expected.Height;
		int total = width * height;
		bool backgroundMatches = CheckBackground(expected, backgroundColor, borderSamplePixels)
			&& CheckBackground(actual, backgroundColor, borderSamplePixels);

		SKBitmap diff = new(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
		int badPixels = 0;
		long deltaSum = 0;
		long maxDelta = 0;

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				SKColor e = expected.GetPixel(x, y);
				SKColor a = actual.GetPixel(x, y);
				int dr = Math.Abs(e.Red - a.Red);
				int dg = Math.Abs(e.Green - a.Green);
				int db = Math.Abs(e.Blue - a.Blue);
				int da = Math.Abs(e.Alpha - a.Alpha);
				int delta = Math.Max(Math.Max(dr, dg), Math.Max(db, da));
				deltaSum += delta;
				maxDelta = Math.Max(maxDelta, delta);
				if (delta > thresholds.ChannelDelta)
				{
					badPixels++;
					diff.SetPixel(x, y, AmplifiedColor(dr, dg, db, da));
				}
				else
				{
					diff.SetPixel(x, y, SKColors.Black);
				}
			}
		}

		double ratio = total == 0 ? 0 : (double)badPixels / total;
		double mae = total == 0 ? 0 : (double)deltaSum / total;
		return new(true, backgroundMatches, badPixels, total, ratio, mae, maxDelta, diff);
	}

	private static bool CheckBackground(SKBitmap bitmap, SKColor background, int samplePixels)
	{
		int border = Math.Min(samplePixels, Math.Min(bitmap.Width, bitmap.Height));
		if (border <= 0)
			return true;
		for (int y = 0; y < bitmap.Height; y++)
		{
			for (int x = 0; x < bitmap.Width; x++)
			{
				bool sample = x < border || y < border
					|| x >= bitmap.Width - border || y >= bitmap.Height - border;
				if (!sample)
					continue;
				SKColor pixel = bitmap.GetPixel(x, y);
				if (pixel.Red != background.Red
					|| pixel.Green != background.Green
					|| pixel.Blue != background.Blue)
					return false;
			}
		}
		return true;
	}

	private static SKColor AmplifiedColor(int dr, int dg, int db, int da)
	{
		const int factor = 4;
		int r = Math.Min(255, dr * factor);
		int g = Math.Min(255, dg * factor);
		int b = Math.Min(255, db * factor);
		_ = da;
		return new SKColor((byte)r, (byte)g, (byte)b, 255);
	}
}
