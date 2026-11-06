using System.Text.Json;
using SkiaSharp;

namespace RhythmBase.RhythmDoctor.View.Compare.Diff;

public sealed class DiffConfigOverride
{
	public int? ChannelDelta { get; set; }
	public double? SignificantPixelRatio { get; set; }
	public double? MeanAbsoluteError { get; set; }
}

public sealed class DiffConfig
{
	public int ChannelDelta { get; set; } = 24;
	public double SignificantPixelRatio { get; set; } = 0.005;
	public double MeanAbsoluteError { get; set; } = 2.0;
	public string BackgroundColor { get; set; } = "#2C2C2C";
	public int BorderSamplePixels { get; set; } = 2;
	public Dictionary<string, DiffConfigOverride> Overrides { get; set; } = [];

	public SKColor Background => SKColor.Parse(BackgroundColor);

	public static DiffConfig Load(string path)
	{
		if (!File.Exists(path))
			return new DiffConfig();
		JsonSerializerOptions options = new()
		{
			PropertyNameCaseInsensitive = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true,
		};
		return JsonSerializer.Deserialize<DiffConfig>(File.ReadAllText(path), options)
			?? new DiffConfig();
	}

	public DiffThresholds ThresholdsFor(string caseId)
	{
		int channelDelta = ChannelDelta;
		double ratio = SignificantPixelRatio;
		double mae = MeanAbsoluteError;

		string? match = Overrides.Keys
			.Where(key => caseId.StartsWith(key, StringComparison.Ordinal))
			.OrderByDescending(key => key.Length)
			.FirstOrDefault();
		if (match is not null)
		{
			DiffConfigOverride item = Overrides[match];
			channelDelta = item.ChannelDelta ?? channelDelta;
			ratio = item.SignificantPixelRatio ?? ratio;
			mae = item.MeanAbsoluteError ?? mae;
		}

		return new DiffThresholds(channelDelta, ratio, mae);
	}
}
