using RhythmBase.Global.Components;
using RhythmBase.Global.Components.Vector;
using RhythmBase.Global.Events;
using RhythmBase.RhythmDoctor;
using RhythmBase.RhythmDoctor.Components;
using RhythmBase.RhythmDoctor.Events;
using RhythmBase.RhythmDoctor.Serialization;
using RhythmBase.RhythmDoctor.View;

namespace RhythmBase.RhythmDoctor.View.Compare.Cases;

public sealed record ParityCase(
	string Id,
	IBaseEvent Event,
	IconStyleConfig Style,
	Chart? Chart = null,
	string? SkipReason = null);

public static class CaseCatalog
{
	public static IReadOnlyList<ParityCase> Create(int scale)
	{
		List<ParityCase> cases = [];
		IconStyleConfig Style(bool enabled = true, bool active = false, bool hover = false)
			=> new()
			{
				Scale = scale,
				Enabled = enabled,
				Active = active,
				Hover = hover,
			};

		void Add(string id, IBaseEvent e, IconStyleConfig? style = null, Chart? chart = null, string? skip = null)
			=> cases.Add(new($"{id}", e, style ?? Style(), chart, skip));

		foreach (EventType type in Enum.GetValues(typeof(EventType)))
		{
			if (EventTypeRegistry.CustomTypes.Contains(type))
				continue;
			Type eventType;
			try
			{
				eventType = type.ToType();
			}
			catch (Exception)
			{
				continue;
			}
			if (eventType.IsAbstract)
				continue;
			if (Activator.CreateInstance(eventType) is not IBaseEvent e)
				continue;

			PrepareBaseline(e);
			ApplyDefaults(e);

			string id = type.ToString();
			if (type is EventType.SetBackgroundColor or EventType.SetForeground)
			{
				Add(id, e, Style(), skip: "external image events are out of scope in the first version");
				continue;
			}
			Add(id, e);
		}

		AddClassicBeatVariants(Add, Style);
		AddOneshotVariants(Add, Style);
		AddFreeTimeVariants(Add, Style);
		AddSayReadyVariants(Add, Style);
		AddDurationAndStateVariants(Add, Style);
		AddRoomVariants(Add, Style);
		AddTextVariants(Add, Style);
		AddRowAndMiscVariants(Add, Style);

		return cases;
	}

	private static void PrepareBaseline(IBaseEvent e)
	{
		e.Active = true;
		e.Y = 0;
		e.Tag = "";
		if (e is IDurationEvent durationEvent)
			durationEvent.Duration = 1f;
	}

	private static void ApplyDefaults(IBaseEvent e)
	{
		switch (e)
		{
			case CustomFlash customFlash:
				customFlash.StartColor = new PaletteColor(new Color(0xFFFF0000));
				customFlash.EndColor = new PaletteColor(new Color(0xFF0000FF));
				customFlash.StartOpacity = 1;
				customFlash.EndOpacity = 1;
				break;
			case DesktopColor desktopColor:
				desktopColor.EndColor = new PaletteColor(new Color(0xFF800080));
				break;
			case FloatingText floatingText:
				floatingText.Text = "Hello";
				floatingText.Color = new Color(0xFFFFFFFF);
				floatingText.OutlineColor = new Color(0xFF000000);
				break;
			case SetText setText:
				setText.Text = "Hello";
				setText.Color = new Color(0xFFFFFFFF);
				setText.OutlineColor = new Color(0xFF000000);
				break;
			case Tint tint:
				tint.TintColor = new Color(0xFFFFFFFF);
				tint.Border = Border.Glow;
				tint.BorderColor = new Color(0xFFFF0000);
				break;
			case TintRows tintRows:
				tintRows.TintColor = new Color(0xFFFFFFFF);
				tintRows.Border = Border.Glow;
				tintRows.BorderColor = new Color(0xFFFF0000);
				break;
			case TintText tintText:
				tintText.TintColor = new Color(0xFFFFFFFF);
				tintText.BorderColor = new Color(0xFF000000);
				break;
			case PaintHands paintHands:
				paintHands.TintColor = new Color(0xFFFFFFFF);
				paintHands.Border = Border.Glow;
				paintHands.BorderColor = new Color(0xFFFF0000);
				break;
			case Comment comment:
				comment.Text = "comment";
				comment.Color = new PaletteColor(new Color(0xFFFF0000));
				break;
			case ShowRooms showRooms:
				showRooms.Rooms = new Room(new byte[] { 0, 2 });
				break;
			case ReorderRooms reorderRooms:
				reorderRooms.Order = new Order(new[] { 1, 3, 2, 0 });
				break;
			case ReorderWindows reorderWindows:
				reorderWindows.Order = new Order(new[] { 1, 3, 2, 0 });
				break;
		}
	}

	private static void AddClassicBeatVariants(
		Action<string, IBaseEvent, IconStyleConfig?, Chart?, string?> add,
		Func<bool, bool, bool, IconStyleConfig> style)
	{
		add("AddClassicBeat/tick-0.5", new AddClassicBeat { Tick = 0.5f }, null, null, null);
		add("AddClassicBeat/tick-0.5-swing-0.5", new AddClassicBeat { Tick = 0.5f, Swing = 0.5f }, null, null, null);
		add("AddClassicBeat/hold-2", new AddClassicBeat { Tick = 0.5f, Hold = 2f }, null, null, null);
		add("AddClassicBeat/length-5", new AddClassicBeat { Tick = 0.5f, Length = 5 }, null, null, null);
		add("AddClassicBeat/hover", new AddClassicBeat { Tick = 0.5f, Length = 5 }, style(true, false, true), null, null);

		Chart chart = [];
		SetRowXs setRowXs = new()
		{
			SyncoBeat = 3,
			SyncoSwing = 0.5f,
			TickTime = new TickTime(chart.Calculator, 1, 1)
		};
		chart.Add(setRowXs);
		AddClassicBeat beat = new()
		{
			Tick = 0.5f,
			Length = 5,
			TickTime = new TickTime(chart.Calculator, 1, 2)
		};
		chart.Add(beat);
		add("AddClassicBeat/preXs-synco-3-swing-0.5", beat, null, chart, null);
	}

	private static void AddOneshotVariants(
		Action<string, IBaseEvent, IconStyleConfig?, Chart?, string?> add,
		Func<bool, bool, bool, IconStyleConfig> style)
	{
		add("AddOneshotBeat/hover", new AddOneshotBeat { Tick = 1f, Loop = 1 }, style(true, true, true), null, null);
		add("AddOneshotBeat/loops-subdiv", new AddOneshotBeat
		{
			Tick = 1f,
			Interval = 2f,
			Loop = 2,
			Subdivisions = 3,
			PulseType = OneshotPulseShapeType.Square,
		}, null, null, null);
		add("AddOneshotBeat/hold", new AddOneshotBeat { Tick = 1f, Interval = 2f, Hold = true }, null, null, null);
		add("AddOneshotBeat/skipshot", new AddOneshotBeat { Tick = 1f, Interval = 2f, Skipshot = true }, null, null, null);
		add("AddOneshotBeat/freezeshot-delay", new AddOneshotBeat
		{
			Tick = 1f,
			Interval = 2f,
			FreezeBurnMode = OneshotType.Freezeshot,
			Delay = 1f,
		}, null, null, null);
		add("AddOneshotBeat/burnshot", new AddOneshotBeat
		{
			Tick = 1f,
			Interval = 2f,
			FreezeBurnMode = OneshotType.Burnshot,
		}, null, null, null);
	}

	private static void AddFreeTimeVariants(
		Action<string, IBaseEvent, IconStyleConfig?, Chart?, string?> add,
		Func<bool, bool, bool, IconStyleConfig> style)
	{
		add("AddFreeTimeBeat/pulse-6", new AddFreeTimeBeat { Pulse = 6 }, null, null, null);
		add("AddFreeTimeBeat/pulse-minus-1", new AddFreeTimeBeat { Pulse = -1 }, null, null, null);
		add("AddFreeTimeBeat/hold-2", new AddFreeTimeBeat { Pulse = 2, Hold = 2f }, null, null, null);
		add("PulseFreeTimeBeat/custom-6", new PulseFreeTimeBeat
		{
			Action = PulseAction.Custom,
			CustomPulse = 6,
		}, null, null, null);
		add("PulseFreeTimeBeat/hold-2", new PulseFreeTimeBeat { Hold = 2f }, null, null, null);
	}

	private static void AddSayReadyVariants(
		Action<string, IBaseEvent, IconStyleConfig?, Chart?, string?> add,
		Func<bool, bool, bool, IconStyleConfig> style)
	{
		add("SayReadyGetSetGo/just-go", new SayReadyGetSetGo { PhraseToSay = SayReadyGetSetGoWord.JustSayGo }, null, null, null);
		add("SayReadyGetSetGo/get-set-go", new SayReadyGetSetGo { PhraseToSay = SayReadyGetSetGoWord.SayGetSetGo }, null, null, null);
		add("SayReadyGetSetGo/count-3", new SayReadyGetSetGo { PhraseToSay = SayReadyGetSetGoWord.Count3 }, null, null, null);
		add("SayReadyGetSetGo/tick-2", new SayReadyGetSetGo
		{
			PhraseToSay = SayReadyGetSetGoWord.SayReadyGetSetGo,
			Tick = 2f,
		}, null, null, null);
		add("SayReadyGetSetGo/tick-1", new SayReadyGetSetGo
		{
			PhraseToSay = SayReadyGetSetGoWord.SayReadyGetSetGo,
			Tick = 1f,
		}, null, null, null);
		add("SayReadyGetSetGo/room-hover", new SayReadyGetSetGo
		{
			PhraseToSay = SayReadyGetSetGoWord.SayGetSetGo,
			Tick = 1f,
		}, style(true, true, true), null, null);
	}

	private static void AddDurationAndStateVariants(
		Action<string, IBaseEvent, IconStyleConfig?, Chart?, string?> add,
		Func<bool, bool, bool, IconStyleConfig> style)
	{
		add("SetSpeed/duration-4", new SetSpeed { Duration = 4f }, style(true, true, true), null, null);
		add("MoveCamera/duration-4", new MoveCamera { Duration = 4f }, style(true, true, true), null, null);
		add("PaintHands/duration-4", new PaintHands { Duration = 4f }, style(true, true, true), null, null);
		add("SetBeatsPerMinute/tag", new SetBeatsPerMinute { Tag = "tag1" }, null, null, null);
		add("Comment/disabled", new Comment { Text = "comment" }, style(false, false, false), null, null);
		add("SetSpeed/active", new SetSpeed(), style(true, true, false), null, null);
	}

	private static void AddRoomVariants(
		Action<string, IBaseEvent, IconStyleConfig?, Chart?, string?> add,
		Func<bool, bool, bool, IconStyleConfig> style)
	{
		add("ShowRooms/rooms-1-3", new ShowRooms
		{
			Rooms = new Room(new byte[] { 0, 2 }),
		}, style(true, true, true), null, null);
		add("ReorderRooms/order", new ReorderRooms
		{
			Order = new Order(new[] { 2, 0, 3, 1 }),
		}, null, null, null);
		add("ReorderWindows/order", new ReorderWindows
		{
			Order = new Order(new[] { 2, 0, 3, 1 }),
		}, null, null, null);
		add("MoveRoom/scaled", new MoveRoom
		{
			Scale = new Size(70f, 100f),
			Angle = 20f,
		}, null, null, null);
		add("FadeRoom/room-active", new FadeRoom(), style(true, true, true), null, null);
		add("MaskRoom/room-active", new MaskRoom(), style(true, true, true), null, null);
		add("SetRoomContentMode/room-active", new SetRoomContentMode(), style(true, true, true), null, null);
		add("SetRoomPerspective/room-active", new SetRoomPerspective(), style(true, true, true), null, null);
	}

	private static void AddTextVariants(
		Action<string, IBaseEvent, IconStyleConfig?, Chart?, string?> add,
		Func<bool, bool, bool, IconStyleConfig> style)
	{
		add("FloatingText/text", new FloatingText { Text = "Hello" }, null, null, null);
		add("SetText/text", new SetText { Text = "Hello" }, null, null, null);
		add("TintText/colors", new TintText
		{
			TintColor = new Color(0xFFFFFFFF),
			BorderColor = new Color(0xFF000000),
		}, null, null, null);
	}

	private static void AddRowAndMiscVariants(
		Action<string, IBaseEvent, IconStyleConfig?, Chart?, string?> add,
		Func<bool, bool, bool, IconStyleConfig> style)
	{
		add("SetRowXs/pattern", new SetRowXs
		{
			Pattern = "x-x-x-",
			SyncoBeat = 2,
			SyncoSwing = 0.5f,
		}, null, null, null);
		add("SetCrotchetsPerBar/12", new SetCrotchetsPerBar { CrotchetsPerBar = 12 }, null, null, null);
		add("FlipScreen/flip-x", new FlipScreen { FlipX = true }, null, null, null);
		add("FlipScreen/flip-x-y", new FlipScreen { FlipX = true, FlipY = true }, null, null, null);
	}
}
