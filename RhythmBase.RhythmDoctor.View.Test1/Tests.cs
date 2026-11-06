using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using RhythmBase.Global.Components;
using RhythmBase.Global.Components.Easing;
using RhythmBase.Global.Events;
using RhythmBase.Global.Extensions;
using RhythmBase.RhythmDoctor;
using RhythmBase.RhythmDoctor.Components;
using RhythmBase.RhythmDoctor.Events;
using RhythmBase.RhythmDoctor.Extensions;
using RhythmBase.RhythmDoctor.Serialization;
using RhythmBase.RhythmDoctor.Utils;
using SkiaSharp;
using System;
using System.Collections;
using System.Diagnostics;
using static RhythmBase.RhythmDoctor.View.LevelViewer;

namespace RhythmBase.RhythmDoctor.View.Test1;

internal class Tests
{
	record struct YRange
	{
		public int Min;
		public int Max;
	}
	record struct DrawData()
	{
		public int UnitSize { get; set; } = 14;
		public int PixelSize { get; set; } = 2;
		public List<Tab> Tabs { get; set; } = [];
		public TickTime Start { get; set; }
		public TickTime End { get; set; }
		public Dictionary<Tab, YRange> TabYRanges { get; set; } = [];
		public Dictionary<Tab, int> TabYOffsets { get; set; } = [];
		public int CurrentY { get; set; }
		public List<KeyValuePair<TickTime, float>> TempoChanges { get; set; } = [];
		public float? StartBpm { get; set; }
	}
	const int scale = 2;
	const int size = 14 * scale;
	const int resize = 1;

	public static void Run1(string file)
	{

		const int scale = 2;
		const int size = 14 * scale;
		const int resize = 1;

		RhythmBase.Global.Config.CachePath = @"\Temp";

		Chart chart = Chart.FromFile(file, new() { ZipProcessingMode = Global.Serialization.ZipProcessingMode.RootEntriesOnly });
		int width = (int)(chart.Duration.Tick * size);
		Console.WriteLine(chart.Duration.TimeSpan);

		int height = size * (chart.Rows.Count + 6);

		using SKBitmap bitmap = new(width * resize, height * resize);
		using SKCanvas canvas = new(bitmap);
		canvas.Clear(0xff2C2C2C);
		canvas.Scale(resize);

		// 网格
		for (TickTime i = chart.DefaultTick; i < chart.Duration; i += 1)
		{
			(int bar, float beat) = i;
			if (beat == 1)
			{
				canvas.DrawRect(
						i.Tick * size, 0,
						scale, height,
						new SKPaint
						{
							Color = 0xff7A7A7A,
							Style = SKPaintStyle.Fill,
						});
				canvas.DrawText(
						$"{bar}",
						new SKPoint(i.Tick * size + 2, 8 * scale), SKTextAlign.Left,
						new SKFont(SKTypeface.FromFamilyName("丁卯点阵�?7px"), 10 * scale),
						new SKPaint
						{
							Color = 0xffFFFFFF,
							IsAntialias = true,
						});
			}
			else
				canvas.DrawRect(
						i.Tick * size, 0,
						scale, height,
						new SKPaint
						{
							Color = 0xff3B3B3B,
							Style = SKPaintStyle.Fill,
						});
		}

		IconStyleConfig style = new() { Scale = scale };

		// 关卡事件
		foreach (var e in chart)
		{
			if (e is PlaySound)
				canvas.DrawEventIcon(e, new SKPointI(
				(int)(e.TickTime.Tick * size),
				(int)((1.5f) * size)
				), style);
			else if (e is SayReadyGetSetGo)
				canvas.DrawEventIcon(e, new SKPointI(
				(int)(e.TickTime.Tick * size),
				(int)((2.5f) * size)
				), style);
			else if (e is BaseBeat bb)
				canvas.DrawEventIcon(e, new SKPointI(
				(int)(e.TickTime.Tick * size),
				(int)(((bb.Parent?.Index ?? 0) + 4.5f) * size)
				), style with { Hover = e is AddClassicBeat, });
			else if (e.Tab is Tab.Sounds)
				canvas.DrawEventIcon(e, new SKPointI(
				(int)(e.TickTime.Tick * size),
				(int)((.5f) * size)
				), style);
			else if (e is ChangePlayersRows)
				canvas.DrawEventIcon(e, new SKPointI(
				(int)(e.TickTime.Tick * size),
				(int)((3.5f) * size)
				), style);
		}

		using Stream stream = File.OpenWrite($"{Path.GetDirectoryName(file)}/{Path.GetFileNameWithoutExtension(file)}.png");
		bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
		Console.WriteLine($"{chart.Settings.Song}");
	}
	public static void Run2(IBaseEvent e, IconStyleConfig? style)
	{
		using SKBitmap bitmap = new(6 * size * resize, 6 * size * resize);
		using SKCanvas canvas = new(bitmap);
		//canvas.Clear(0xff2C2C2C);
		canvas.Clear(0xffff0000);
		canvas.Scale(resize);
		var area = canvas.DrawEventIcon(e, new(28, 0), style ?? new()
		{
			Scale = 2,
			Active = true,
			Enabled = true,
			Hover = false,
			ShowDuration = true,
		});
		canvas.DrawRect(area, new SKPaint
		{
			Color = 0xffff0000,
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 1,
		});
		using Stream stream = File.OpenWrite($"test_icon.png");
		bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
	}
	public static void Run3()
	{
		using SKBitmap bitmap = new(7 * size * resize, 150 * size * resize);
		using SKCanvas canvas = new(bitmap);
		canvas.Clear(0xff2c2c2c);
		SKPoint p = default;
		foreach (EventType type in Enum.GetValues(typeof(EventType)))
		{
			//index++;
			//if (index != max)
			//	continue;
			if (EventTypeRegistry.CustomTypes.Contains(type))
				continue;
			Type type1 = type.ToType();
			if (type1.IsAbstract)
				continue;
			IBaseEvent? e = (IBaseEvent?)Activator.CreateInstance(type1);
			if (e is null) continue;
			if (e is IDurationEvent durationEvent)
				durationEvent.Duration = 2;
			if (e is CustomFlash customFlash)
			{
				customFlash.StartColor = Color.Red;
				customFlash.EndColor = Color.Blue;
			}
			if (e is FlipScreen flipScreen)
				flipScreen.FlipX = true;
			if (e is MoveRoom moveRoom)
				moveRoom.Scale = new(1, 1);
			if (e is DesktopColor dc)
				dc.EndColor = Color.Purple;
			if (e is Tint tint)
			{
				tint.Border = Border.Glow;
				tint.BorderColor = Color.Red;
			}
			var rect = canvas.DrawEventIcon(e, p, new() { Scale = 2, Hover = false, Active = false });
			canvas.DrawRect(rect, new SKPaint
			{
				Color = 0xffff0000,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 1,
			});
			Console.WriteLine(type);
			p.Y += rect.Height;
		}
		using Stream stream = File.OpenWrite($"test_icon.png");
		bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
	}
	public static void Run4(string file)
	{
		Chart chart;
		string extension = Path.GetExtension(file);
		if (extension is ".rdzip" or ".zip")
		{
			using Level level = Level.FromZip(file);
			chart = level.MainChart;
		}
		else
			chart = Chart.FromFile(file);

		Console.WriteLine(chart.Count);


		View.Directory = chart.ResolvedDirectory ?? string.Empty;
		View.Palette = chart.ColorPalette;
		View.Conditionals = chart.Conditionals;

		const int unitSize = 14;
		const int pixelSize = 2;

		int start_bar = 1;
		int start_beat = 1;
		int? end_bar = null;
		int? end_beat = null;
		List<Tab> tabs = [Tab.Sounds, Tab.Rows, Tab.Actions, Tab.Decorations, Tab.Rooms, Tab.Windows];

		TickTime start = new(chart.Calculator, start_bar, start_beat);
		TickTime end = end_bar is int end_bar_notnull ? new(chart.Calculator, end_bar_notnull, end_beat ?? 1) : chart.Duration;

		SKFontManager manager = SKFontManager.Default;
		var tf =
#if arm64
			manager.MatchFamily("DinkieBitmap", new SKFontStyle(400, 5, SKFontStyleSlant.Upright));
#else
			manager.MatchFamily("DinkieBitmap 7px", new SKFontStyle(400, 5, SKFontStyleSlant.Upright));
#endif

		List<IBaseEvent> events = [];
		float? startBpm = null;
		SortedDictionary<TickTime, float> tempoChanges = [];
		Dictionary<Tab, YRange> tabYRanges = [];
		foreach (var e in chart.InRange(start, end + 1))
		{
			(int bar, _) = e.TickTime;
			startBpm ??= e.TickTime.Bpm;
			if (e is BaseBeatsPerMinute bbpm)
			{
				if (tempoChanges.ContainsKey(bbpm.TickTime))
					tempoChanges.Remove(bbpm.TickTime);
				tempoChanges.Add(bbpm.TickTime, bbpm.BeatsPerMinute);
			}
			if (tabs.Contains(e.Tab))
			{
				if (e.Tab is Tab.Rooms)
				{
					tabYRanges[Tab.Rooms] = new YRange { Min = 0, Max = 3 };
				}
				if (e.Tab is Tab.Windows)
				{
					int maxY = 4;
					if (tabYRanges.TryGetValue(Tab.Windows, out var rw))
						maxY = int.Max(rw.Max, 4);
					tabYRanges[Tab.Windows] = new YRange { Min = 0, Max = int.Max(maxY, e.Y) };
				}
				if (tabYRanges.TryGetValue(e.Tab, out YRange range))
				{
					int y = e.Y;
					range.Min = Math.Min(range.Min, y);
					range.Max = Math.Max(range.Max, y);
					tabYRanges[e.Tab] = range;
				}
				else
				{
					tabYRanges[e.Tab] = new YRange { Min = e.Y, Max = e.Y };
				}
				if (e.Y < 0)
				{
					Console.WriteLine(e);
				}
				events.Add(e);
			}
		}
		Dictionary<Tab, int> tabYOffsets = [];
		int currentY = 0;
		foreach (var tab in tabs)
		{
			if (!tabYRanges.ContainsKey(tab))
				continue;
			var range = tabYRanges[tab];
			tabYOffsets[tab] = currentY;
			currentY += range.Max - range.Min + 1;
		}
		currentY -= 1;
		start = events.First().TickTime.QuantizeBeat(1);
		end = events.Last().TickTime.QuantizeBeat(1) + 1;

		DrawData drawData = new DrawData()
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

		SKImageInfo info = new SKImageInfo(
				(int)(end.Tick - start.Tick + 2) * pixelSize * unitSize,
				(currentY + 3) * pixelSize * unitSize);
		using SKBitmap surface = new(info);
		using SKCanvas canvas = new(surface);
		DrawBackground(drawData, chart, info, tf, canvas);
		int index = 0;
		foreach (var e in events)
		{
			canvas.DrawEventIcon(e, ToLocation(e, start.Tick, tabYRanges[e.Tab].Min - tabYOffsets[e.Tab]), new()
			{
				Scale = drawData.PixelSize,
				ShowDuration = false,
				Active = false,
				Hover = e is AddClassicBeat,
				Enabled = e.Active,
			});
			index++;
		}
		chart.Dispose();
		SKBitmap image = surface;
		using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
		using Stream stream = File.OpenWrite($"{Path.GetDirectoryName(file)}/{Path.GetFileNameWithoutExtension(file)}_timeline.png");
		data.SaveTo(stream);

		static SKPointI ToLocation(IBaseEvent e, float left, int top)
		{
			return new SKPointI(
					(int)((e.TickTime.Tick - left) * 28),
					((e is ReorderRooms or ShowRooms ? 0 : e.Y) - top) * 28
			);
		}
	}
	public static void Run4_2(string file)
	{
		Chart chart;
		string extension = Path.GetExtension(file);
		if (extension is ".rdzip" or ".zip")
		{
			using Level level = Level.FromZip(file);
			chart = level.MainChart;
		}
		else
			chart = Chart.FromFile(file);
		Console.WriteLine(chart.Count);


		View.Directory = chart.ResolvedDirectory ?? string.Empty;
		View.Palette = chart.ColorPalette;
		View.Conditionals = chart.Conditionals;

		const int unitSize = 14;
		const int pixelSize = 2;

		int start_bar = 1;
		int start_beat = 1;
		int? end_bar = null;
		int? end_beat = null;
		List<Tab> tabs = [Tab.Sounds, Tab.Rows, Tab.Actions, Tab.Decorations, Tab.Rooms, Tab.Windows];

		TickTime start = new(chart.Calculator, start_bar, start_beat);
		TickTime end = end_bar is int end_bar_notnull ? new(chart.Calculator, end_bar_notnull, end_beat ?? 1) : chart.Duration;

		SKFontManager manager = SKFontManager.Default;
		var tf =
#if arm64
			manager.MatchFamily("DinkieBitmap", new SKFontStyle(400, 5, SKFontStyleSlant.Upright));
#else
			manager.MatchFamily("DinkieBitmap 7px", new SKFontStyle(400, 5, SKFontStyleSlant.Upright));
#endif

		List<IBaseEvent> events = [];
		float? startBpm = null;
		SortedDictionary<TickTime, float> tempoChanges = [];
		Dictionary<Tab, YRange> tabYRanges = [];
		foreach (var e in chart.InRange(start, end + 1))
		{
			(int bar, _) = e.TickTime;
			startBpm ??= e.TickTime.Bpm;
			if (e is BaseBeatsPerMinute bbpm)
			{
				if (tempoChanges.ContainsKey(bbpm.TickTime))
					tempoChanges.Remove(bbpm.TickTime);
				tempoChanges.Add(bbpm.TickTime, bbpm.BeatsPerMinute);
			}
			if (tabs.Contains(e.Tab))
			{
				if (e.Tab is Tab.Rooms)
				{
					tabYRanges[Tab.Rooms] = new YRange { Min = 0, Max = 3 };
				}
				if (e.Tab is Tab.Windows)
				{
					int maxY = 4;
					if (tabYRanges.TryGetValue(Tab.Windows, out var rw))
						maxY = int.Max(rw.Max, 4);
					tabYRanges[Tab.Windows] = new YRange { Min = 0, Max = int.Max(maxY, e.Y) };
				}
				if (tabYRanges.TryGetValue(e.Tab, out YRange range))
				{
					int y = e.Y;
					range.Min = Math.Min(range.Min, y);
					range.Max = Math.Max(range.Max, y);
					tabYRanges[e.Tab] = range;
				}
				else
				{
					tabYRanges[e.Tab] = new YRange { Min = e.Y, Max = e.Y };
				}
				if (e.Y < 0)
				{
					Console.WriteLine(e);
				}
				events.Add(e);
			}
		}
		Dictionary<Tab, int> tabYOffsets = [];
		int currentY = 0;
		foreach (var tab in tabs)
		{
			if (!tabYRanges.ContainsKey(tab))
				continue;
			var range = tabYRanges[tab];
			tabYOffsets[tab] = currentY;
			currentY += range.Max - range.Min + 1;
		}
		currentY -= 1;
		start = events.First().TickTime.QuantizeBeat(1);
		end = events.Last().TickTime.QuantizeBeat(1) + 1;

		DrawData drawData = new DrawData()
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

		using var window = new GameWindow(
			GameWindowSettings.Default,
		new NativeWindowSettings()
		{
			ClientSize = new Vector2i(1, 1),
			Title = "Hidden Window",
			APIVersion = new Version(3, 3),
			Profile = ContextProfile.Core,
			Flags = ContextFlags.ForwardCompatible
		}); // 最小窗口，只用于获取上下文
		window.MakeCurrent();

		// 2. 验证 OpenGL 版本�?FBO 支持
		string version = GL.GetString(StringName.Version);
		Console.WriteLine($"OpenGL Version: {version}");

		string extensions = GL.GetString(StringName.Extensions);
		Console.WriteLine(extensions);
		if (!extensions.Contains("GL_ARB_framebuffer_object") && !extensions.Contains("GL_EXT_framebuffer_object"))
			Console.WriteLine("Warning: FBO extensions not found; may cause issues.");

		// 3. 创建 GRGlInterface �?GRContext
		using var glInterface = GRGlInterface.CreateAngle();
		if (!glInterface.Validate())
			throw new Exception("GRGlInterface is invalid!");

		using var context = GRContext.CreateGl(glInterface);

		// 4. 指定离屏渲染尺寸
		int width = 1920;
		int height = 1080;


		int[] fbo = new int[1];
		int[] colorRb = new int[1];
		GL.GenFramebuffers(1, fbo);
		GL.GenRenderbuffers(1, colorRb);

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo[0]);

		GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, colorRb[0]);
		GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height);
		GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, colorRb[0]);

		// 检�?FBO 状�?
		if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			throw new Exception("FBO not complete!");

		// 2. 创建 GRBackendRenderTarget 包装这个 FBO
		uint glFormat = SKColorType.Rgba8888.ToGlSizedFormat();
		var glFbInfo = new GRGlFramebufferInfo((uint)fbo[0], glFormat);
		using var renderTarget = new GRBackendRenderTarget(width, height, 0, 8, glFbInfo);


		SKImageInfo info = new SKImageInfo(
				(int)(end.Tick - start.Tick + 2) * pixelSize * unitSize,
				(currentY + 3) * pixelSize * unitSize);
		using var surface = SKSurface.Create(context, false, info);
		using SKCanvas canvas = surface.Canvas;
		DrawBackground(drawData, chart, info, tf, canvas);
		canvas.DrawEventIcons(events.Select(e => (
			e,
			(SKPoint)ToLocation(e, start.Tick, tabYRanges[e.Tab].Min - tabYOffsets[e.Tab]),
			new IconStyleConfig()
			{
				Scale = drawData.PixelSize,
				ShowDuration = false,
				Active = false,
				Hover = e is AddClassicBeat,
				Enabled = e.Active,
			}
				)));
		//foreach (var e in events)
		//{
		//	canvas.DrawEventIcon(e, ToLocation(e, start.Tick, tabYRanges[e.Tab].Min - tabYOffsets[e.Tab]), new()
		//	{
		//		Scale = drawData.PixelSize,
		//		ShowDuration = false,
		//		Active = false,
		//		Hover = e is AddClassicBeat,
		//		Enabled = e.Active,
		//	});
		//	index++;
		//}
		using SKImage image = surface.Snapshot();
		using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
		using Stream stream = File.OpenWrite($"{Path.GetDirectoryName(file)}/{Path.GetFileNameWithoutExtension(file)}_timeline.png");
		data.SaveTo(stream);

		static SKPointI ToLocation(IBaseEvent e, float left, int top)
		{
			return new SKPointI(
					(int)((e.TickTime.Tick - left) * 28),
					((e is ReorderRooms or ShowRooms ? 0 : e.Y) - top) * 28
			);
		}
	}
	public class GpuSurface : IDisposable
	{
		private readonly GameWindow _window;
		private readonly GRContext _grContext;
		private readonly int _fbo;
		private readonly int _renderbuffer;
		public SKSurface Surface { get; }

		public GpuSurface(SKImageInfo info)
		{
			// 1. 创建隐藏窗口
			_window = new GameWindow(
					GameWindowSettings.Default,
					new NativeWindowSettings
					{
						ClientSize = new Vector2i(1, 1),
						Title = "Hidden",
						APIVersion = new Version(3, 3),
						Profile = ContextProfile.Core,
						Flags = ContextFlags.ForwardCompatible
					}
			);
			_window.Context.MakeCurrent();

			// 2. GL 接口�?GRContext
			using var glInterface = GRGlInterface.Create();
			if (!glInterface.Validate())
				throw new Exception("GL interface invalid.");
			_grContext = GRContext.CreateGl(glInterface);

			// 3. 创建 FBO
			int[] fbo = new int[1], rb = new int[1];
			GL.GenFramebuffers(1, fbo);
			GL.GenRenderbuffers(1, rb);
			_fbo = fbo[0];
			_renderbuffer = rb[0];

			GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _renderbuffer);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, info.Width, info.Height);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
																 RenderbufferTarget.Renderbuffer, _renderbuffer);

			if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
				throw new Exception("FBO incomplete.");

			// 4. 包装�?GRBackendRenderTarget
			uint glFormat = SKColorType.Rgba8888.ToGlSizedFormat();
			var glFbInfo = new GRGlFramebufferInfo((uint)_fbo, glFormat);
			using var renderTarget = new GRBackendRenderTarget(info.Width, info.Height, 0, 8, glFbInfo);

			// 5. 创建 SKSurface
			Surface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
			if (Surface == null)
				throw new Exception("SKSurface creation failed.");
		}

		public void Dispose()
		{
			Surface?.Dispose();
			_grContext?.Dispose();
			if (_fbo != 0)
				GL.DeleteFramebuffers(1, new int[] { _fbo });
			if (_renderbuffer != 0)
				GL.DeleteRenderbuffers(1, new int[] { _renderbuffer });
			_window?.Dispose();
		}
	}

	private static void DrawBackground(DrawData drawData, Chart chart, SKImageInfo info, SKTypeface typeface, SKCanvas canvas)
	{
		int unitSize = drawData.UnitSize; // Replace with actual unitSize if needed
		int pixelSize = drawData.PixelSize; // Replace with actual pixelSize if needed
		canvas.Translate(pixelSize * unitSize, pixelSize * unitSize);
		canvas.Clear(0xff2C2C2C);
		canvas.Save();
		canvas.RotateDegrees(-90);
		canvas.DrawText($"{chart.Settings.Artist} - {chart.Settings.Song} by {chart.Settings.Author}",
				pixelSize, -pixelSize * 2, SKTextAlign.Right, new SKFont(typeface, 24), new SKPaint
				{
					Color = 0xffffffff,
					Style = SKPaintStyle.Fill,
				});
		canvas.Restore();

		int index = 0;
		List<KeyValuePair<TickTime, float>> tempoList = [.. drawData.TempoChanges];
		if (tempoList.Count == 0 || tempoList[0].Key != drawData.Start)
			tempoList.Insert(0, new KeyValuePair<TickTime, float>(drawData.Start, drawData.StartBpm ?? 0));
		foreach (var tempo in tempoList)
		{
			int x = (int)((tempo.Key.Tick - drawData.Start.Tick) * pixelSize * unitSize);
			using var paint = new SKPaint
			{
				Color = 0xff7a7a7a,
				Style = SKPaintStyle.Fill,
			};
			canvas.DrawText($"{tempo.Value} BPM", x + pixelSize, -pixelSize - 12, SKTextAlign.Left, new SKFont(typeface), paint);
			if (index > 0 && index % 2 == 0)
			{
				var prevTempo = tempoList[index - 1];
				int prevX = (int)((prevTempo.Key.Tick - drawData.Start.Tick) * pixelSize * unitSize);
				canvas.DrawRect(prevX, 0, x - prevX, (drawData.CurrentY + 1) * pixelSize * unitSize, new SKPaint
				{
					Color = 0x227a7a7a,
					Style = SKPaintStyle.Fill,
				});
			}

			index++;
		}
		if (drawData.TempoChanges.Count > 0)
		{
			var lastTempo = tempoList.Last();
			int lastX = (int)((lastTempo.Key.Tick - drawData.Start.Tick) * pixelSize * unitSize);
			canvas.DrawRect(lastX, 0, info.Width - lastX - 2 * pixelSize * unitSize, (drawData.CurrentY + 1) * pixelSize * unitSize, new SKPaint
			{
				Color = 0x227a7a7a,
				Style = SKPaintStyle.Fill,
			});
		}

		for (TickTime b = drawData.Start; b < drawData.End; b += 1)
		{
			int x = (int)((b.Tick - drawData.Start.Tick) * pixelSize * unitSize);
			(int bar, float beat) = b;
			uint color = beat == 1 ? 0xff7a7a7a : 0xff3C3C3C;
			using var paint = new SKPaint
			{
				Color = color,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = pixelSize,
			};
			if (beat == 1)
			{
				canvas.DrawLine(x, 0, x, (drawData.CurrentY + 1) * pixelSize * unitSize, paint);
				canvas.DrawText(bar.ToString(), x + pixelSize, -pixelSize, SKTextAlign.Left, new SKFont(typeface, 24), paint);
			}
			canvas.DrawLine(x, 0, x, (drawData.CurrentY + 1) * pixelSize * unitSize, paint);
		}

		foreach (var bookmark in chart.Bookmarks)
		{
			TickTime b = bookmark.Tick.WithLink(chart);
			int x = (int)((b.Tick - drawData.Start.Tick) * pixelSize * unitSize);
			(int bar, float beat) = bookmark.Tick;
			uint color = bookmark.Color.ToColor();
			using var paint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Shader = SKShader.CreateLinearGradient(
							new SKPoint(x, 0),
							new SKPoint(x + pixelSize * unitSize, 0),
							[new SKColor(color).WithAlpha(192), new SKColor(color).WithAlpha(0)],
							null,
							SKShaderTileMode.Clamp)
			};
			canvas.DrawRect(x, 0, pixelSize * unitSize, (drawData.CurrentY + 1) * pixelSize * unitSize, paint);
		}

		foreach (var tab in drawData.Tabs)
		{
			if (!drawData.TabYRanges.ContainsKey(tab))
				continue;
			var range = drawData.TabYRanges[tab];
			using var paint = new SKPaint
			{
				Color = View.ColorOf(tab).WithAlpha(32),
				Style = SKPaintStyle.Fill,
			};
			int y = (drawData.TabYOffsets[tab] + range.Min) * pixelSize * unitSize;
			int height = (range.Max - range.Min + 1) * pixelSize * unitSize;
			canvas.DrawRect(0, y, info.Width, height, paint);
		}
	}

	private record struct AnimationData
	{
		public IBaseEvent Event { get; init; }
		public float RandomY { get; init; }
	}
	public static void Run5()
	{
		string levelFile = @"E:\Download\H.S. - PAX_13 et al. rK4vR35B.rdzip";


		int unitSize = Config.UnitSize;
		int pixelSize = Config.PixelSize;

		Chart chart = Chart.FromFile(levelFile);

		LevelViewer.Filter filter = LevelViewer.Filter.All;
		LevelViewer.RenderData data = LevelViewer.GetRenderData(chart, filter);

		View.Directory = chart.ResolvedDirectory;
		View.Palette = chart.ColorPalette;

		SKFontManager manager = SKFontManager.Default;
		var tf = manager.MatchFamily("DinkieBitmap", new SKFontStyle(400, 5, SKFontStyleSlant.Upright));

		using SKSurface instantImageSurface = SKSurface.Create(data.Info);
		if (instantImageSurface is null)
		{
			chart.Dispose();
			return;
		}
		using SKCanvas instantImageCanvas = instantImageSurface.Canvas;

		LevelViewer.DrawBackground(data, instantImageCanvas, tf);

		const int fps = 30;
		const int width = 1920;
		const int height = 1080;

		using SKSurface screenSurface = SKSurface.Create(new SKImageInfo(width, height));
		using SKCanvas screenCanvas = screenSurface.Canvas;
		using var encoder = new H264Encoder($"output.mp4", width, height, fps);

		const float animationDuration = .5f; // seconds

		TimeSpan duration = chart.Duration.TimeSpan;
		int totalFrames = (int)(duration.TotalSeconds * fps);
		int extraAnimationFrames = (int)(animationDuration * fps);


		var p = Console.GetCursorPosition();

		Stopwatch stopwatch = Stopwatch.StartNew();
		long tick = stopwatch.ElapsedMilliseconds;

		TickTime start = data.StartBeat;
		Dictionary<Tab, int> tabTop = [];
		int top = 0;
		foreach (var tab in filter.Tabs)
		{
			if (!data.TabDataCol.ContainsKey(tab))
				continue;
			var range = data.TabDataCol[tab];
			tabTop[tab] = top;
			top += range.MaxY - range.MinY + 1;
		}

		List<IBaseEvent> events = [.. data.Events];
		Queue<IBaseEvent> eventsQueue = new(events);
		Queue<AnimationData> activeEvents = [];
		using SKSurface screenTempSurface = SKSurface.Create(data.Info);
		using SKCanvas screenTempCanvas = screenTempSurface.Canvas;

		for (int i = -extraAnimationFrames; i < totalFrames + extraAnimationFrames; i++)
		{
			float progress = (float)i / totalFrames;

			float currentBeat;
			TimeSpan currentTime = TimeSpan.FromSeconds(i * duration.TotalSeconds / totalFrames);

			if (i >= 0)
			{
				currentBeat = chart.Calculator.TimeSpanToTick(currentTime);
			}
			else
			{
				currentBeat = (float)(chart.TickOf(1).Bpm * currentTime.TotalMinutes) + 1;
			}
			while (activeEvents.Count > 0 && activeEvents.Peek().Event.TickTime.TimeSpan.TotalSeconds <= currentTime.TotalSeconds)
			{
				IBaseEvent e = activeEvents.Dequeue().Event;
				SKPointI dest = ToLocation(e, start.Tick, data.TabDataCol[e.Tab].MinY - tabTop[e.Tab]);
				instantImageCanvas.DrawEventIcon(e, dest, new()
				{
					Scale = pixelSize,
					ShowDuration = true,
					Active = false,
					Hover = e is AddClassicBeat,
					Enabled = e.Active,
				});
			}
			screenTempCanvas.Clear(0xff2C2C2C);
			screenTempCanvas.DrawSurface(instantImageSurface, 0, 0);
			screenTempCanvas.Save();
			screenTempCanvas.Translate(pixelSize * unitSize, pixelSize * unitSize);

			foreach (var anim in activeEvents)
			{
				float animProgress = (float)(currentTime.TotalSeconds + animationDuration - anim.Event.TickTime.TimeSpan.TotalSeconds) / animationDuration;
				float easedProgress = (float)EaseType.OutExpo.Calculate(animProgress);
				SKPointI dest = ToLocation(anim.Event, start.Tick, data.TabDataCol[anim.Event.Tab].MinY - tabTop[anim.Event.Tab]);
				dest.Y += (int)((anim.RandomY * 2 - 1) * pixelSize * unitSize * 7 * (1 - easedProgress));
				SKPaint paint = new()
				{
					Color = new SKColor(255, 255, 255, (byte)(255 * animProgress)),
				};
				screenTempCanvas.SaveLayer(paint);
				screenTempCanvas.DrawEventIcon(anim.Event, dest, new()
				{
					Scale = pixelSize,
					ShowDuration = true,
					Active = false,
					Hover = anim.Event is AddClassicBeat,
					Enabled = anim.Event.Active,
				});
				screenTempCanvas.Restore();
			}
			screenTempCanvas.Restore();
			while (animationDuration > 0 && eventsQueue.TryPeek(out var nextEvent) && nextEvent.TickTime.TimeSpan.TotalSeconds <= currentTime.TotalSeconds + animationDuration)
			{
				activeEvents.Enqueue(new() { Event = eventsQueue.Dequeue(), RandomY = Random.Shared.NextSingle() });
			}

			int offset = (int)(-currentBeat * unitSize * pixelSize + width / 2);
			screenCanvas.Clear(0xff2C2C2C);
			screenCanvas.Save();
			screenCanvas.DrawSurface(screenTempSurface, new SKPoint(offset, 0));
			screenCanvas.DrawLine(width / 2, 0, width / 2, height, new SKPaint
			{
				Color = 0xffff0000,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 2,
			});
			screenCanvas.Restore();
			screenCanvas.Flush();


			//using SKImage frameImage = screenSurface.Snapshot();
			//using SKBitmap bitmap = SKBitmap.FromImage(frameImage);
			//byte[] bytes = bitmap.Bytes;

			using var pixmap = screenSurface.PeekPixels();
			if (pixmap is null || pixmap.GetPixels() == nint.Zero)
				continue;

			//{
			//    using FileStream fs = new($"frames/frame_{i:D4}.png", FileMode.Create, FileAccess.Write);
			//    frameImage.Encode(SKEncodedImageFormat.Png, 100).SaveTo(fs);
			//}

			encoder.WriteFrame(pixmap.GetPixels(), pixmap.RowBytes);
			if (i % 10 == 0)
			{
				Console.SetCursorPosition(p.Left, p.Top);
				Console.WriteLine($"Rendering... {i}/{totalFrames} {progress:P2}");
				long currentTick = stopwatch.ElapsedMilliseconds;
				long elapsed = currentTick - tick;
				long remaining = (long)((totalFrames - i) * (elapsed / (i + 1)));
				Console.WriteLine($"Remaining: {TimeSpan.FromMilliseconds(remaining):hh\\:mm\\:ss}");
			}
		}
	}
	private static SKPointI ToLocation(IBaseEvent e, float left, int top)
	{
		return new SKPointI(
				(int)((e.TickTime.Tick - left) * 28),
				((e is ReorderRooms or ShowRooms ? 0 : e.Y) - top) * 28
		);
	}
}
