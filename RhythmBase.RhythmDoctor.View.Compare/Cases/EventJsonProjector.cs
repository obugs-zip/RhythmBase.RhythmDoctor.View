using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using RhythmBase.Global.Events;
using RhythmBase.RhythmDoctor;
using RhythmBase.RhythmDoctor.Events;
using RhythmBase.RhythmDoctor.Extensions;

namespace RhythmBase.RhythmDoctor.View.Compare.Cases;

public static class EventJsonProjector
{
	private static readonly JsonSerializerOptions Options = new()
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	};

	public static string Project(ParityCase @case)
	{
		IBaseEvent evt = @case.Event;

		// ToJsonString() is the source of truth: it already writes type/bar/beat/y and
		// every non-default property. Only patch what the TS renderer needs but the
		// C# serializer intentionally omits (derived preXs, TS tab names, default rooms).
		JsonObject node = JsonNode.Parse(evt.ToJsonString()) as JsonObject
			?? throw new InvalidOperationException($"Could not parse event JSON for {@case.Id}.");

		// The JSON converter uses "SetVFXPreset" while EventType/TS use "SetVfxPreset".
		node["type"] = evt.Type.ToString();
		node["tab"] = MapTab(evt.Tab);

		if (evt is AddClassicBeat classicBeat)
		{
			SetRowXs? preXs = classicBeat.TickTime.IsEmpty
				? null
				: classicBeat.FrontOrDefault<SetRowXs>();
			node["preXs"] = new JsonObject
			{
				["syncoBeat"] = preXs?.SyncoBeat ?? -1,
				["syncoSwing"] = preXs?.SyncoSwing ?? 0f,
			};
		}

		IReadOnlyList<byte>? rooms = RoomsOf(evt);
		if (rooms is not null)
		{
			JsonArray array = [];
			foreach (byte room in rooms)
				array.Add(room);
			node["rooms"] = array;
		}

		return node.ToJsonString(Options);
	}

	public static string MapTab(Tab tab) => tab switch
	{
		Tab.Decorations => "Sprites",
		_ => tab.ToString(),
	};

	public static IReadOnlyList<byte>? RoomsOf(IBaseEvent evt)
	{
		if (evt is IRoomEvent roomEvent)
			return roomEvent.Rooms.Rooms;
		if (evt is ISingleRoomEvent singleRoomEvent)
		{
			List<byte> rooms = [];
			int index = (int)singleRoomEvent.Room.Room;
			if (index is >= 1 and <= 4)
				rooms.Add((byte)(index - 1));
			if (singleRoomEvent.Room.EnableTop)
				rooms.Add(4);
			return rooms;
		}
		return null;
	}
}
