<p align="center">
	<a href="https://www.nuget.org/packages/RhythmBase.RhythmDoctor.View/"><img src="https://img.shields.io/nuget/v/RhythmBase.RhythmDoctor.View?logo=nuget" alt="NuGet Version"></a>
	<img src="https://img.shields.io/nuget/dt/RhythmBase.RhythmDoctor.View" alt="NuGet Downloads"/>
</p>

# RhythmBase.RhythmDoctor.View

Draw Rhythm Doctor event icons, event states, rooms, and level timelines with
[SkiaSharp](https://github.com/mono/SkiaSharp).

## Installation

```sh
dotnet add package RhythmBase.RhythmDoctor.View
```

The package embeds the event atlas (`assets` and `assets.png`), so no extra files are
required at runtime.

## Quick Start

```cs
using RhythmBase.Global.Components;
using RhythmBase.Global.Events;
using RhythmBase.RhythmDoctor.Components;
using RhythmBase.RhythmDoctor.View;
using SkiaSharp;
using System.IO;

int height = 28 * 60;
string file = @"your\level.Level";

using Level level = Level.FromFile(file);
int width = (int)(level.Length.BeatOnly * 28);

View.Palette = Chart.Default.ColorPalette;
View.Conditionals = new Chart().Conditionals;
View.Directory = Path.GetDirectoryName(file) ?? string.Empty;

using SKBitmap bitmap = new(width, height);
using SKCanvas canvas = new(bitmap);

foreach (IBaseEvent e in level)
{
	canvas.DrawEventIcon(e, ToLocation(e), new()
	{
		Scale = 2,
		Active = true,
		Enabled = true,
		Hover = false,
	});
}

static SKPointI ToLocation(IBaseEvent e)
	=> new((int)(e.TickTime.BeatOnly * 28), e.Y * 28);

using Stream stream = File.OpenWrite("output.png");
bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
```

## Event Icon Style

`IconStyleConfig` controls each icon:

- `Scale` - scaling factor for the icon size
- `Active` - whether the event is active/selected
- `Enabled` - whether the event is enabled or disabled
- `Hover` - whether the mouse is hovering over the event
- `ShowDuration` - draw a duration bar; for duration events this is overridden by
  `Duration > 1`

Use `MeasureEventIcon` to size a canvas before drawing, and `DrawEventIcons` for
batched rendering.

## Related Projects

- [RhythmBase](https://github.com/RDCN-Community-Developers/RhythmToolkit) - core
  Rhythm Doctor level library

## License

This project is licensed under the GNU Lesser General Public License v3.0.
See [LICENSE.txt](https://github.com/OLDRedstone/RhythmBase.RhythmDoctor.View/blob/main/LICENSE.txt) for the full text.
