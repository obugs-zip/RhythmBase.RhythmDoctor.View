export const eventSamples: Record<string, Record<string, unknown>> = {
  "AddClassicBeat": {
    "length": 7,
    "preXs": {
      "syncoBeat": 3,
      "syncoSwing": 0.5
    },
    "bar": 1,
    "beat": 7,
    "y": 0,
    "type": "AddClassicBeat",
    "tag": "1",
    "tick": 0.5,
    "hold": 1,
    "swing": 0.5,
    "setXs": "NoChange"
  },
  "AddFreeTimeBeat": {
    "type": "AddFreeTimeBeat",
    "row": 0,
    "pulse": 2,
    "hold": 1
  },
  "AddOneshotBeat": {
    "loops": 1,
    "hold": true,
    "subdivisions": 2,
    "interval": 2,
    "pulseType": "Wave",
    "bar": 1,
    "beat": 7,
    "y": 6,
    "type": "AddOneshotBeat",
    "freezeBurnMode": "Freezeshot",
    "tick": 1,
    "delay": 0.5,
    "skipshot": true
  },
  "AdvanceText": {
    "duration": 2,
    "ease": "Linear",
    "bar": 3,
    "beat": 5,
    "y": -1,
    "type": "AdvanceText",
    "id": 0,
    "text": "text 字段不存在于原事件中，但是 RDView 提供了支持。"
  },
  "AdvanceTextDecoration": {
    "type": "AdvanceTextDecoration",
    "id": 0,
    "rooms": [
      0
    ],
    "text": "Decoration text"
  },
  "BassDrop": {
    "bar": 2,
    "beat": 5,
    "y": 0,
    "type": "BassDrop",
    "if": "1&~2d0",
    "rooms": [
      0
    ],
    "strength": "High"
  },
  "Blend": {
    "type": "Blend",
    "rooms": [
      0
    ],
    "duration": 2,
    "ease": "Linear"
  },
  "CallCustomMethod": {
    "bar": 4,
    "beat": 1,
    "y": 4,
    "type": "CallCustomMethod",
    "methodName": "",
    "executionTime": "OnBar",
    "sortOffset": 0
  },
  "ChangeCharacter": {
    "bar": 4,
    "beat": 3,
    "y": 3,
    "type": "ChangeCharacter",
    "row": 0,
    "character": "Samurai",
    "transition": "Instant"
  },
  "ChangePlayersRows": {
    "bar": 5,
    "beat": 1,
    "y": 4,
    "type": "ChangePlayersRows",
    "playerMode": "OneOrTwoPlayers",
    "players": [
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange",
      "NoChange"
    ],
    "cpuMarkers": [
      "Otto",
      "Otto",
      "Otto",
      "Otto",
      "Otto",
      "Otto",
      "Otto",
      "Otto",
      "Otto",
      "Otto",
      "Otto",
      "Otto",
      "Otto",
      "Otto",
      "Otto",
      "Otto"
    ]
  },
  "Comment": {
    "bar": 3,
    "beat": 8,
    "y": 5,
    "type": "Comment",
    "tab": "Actions",
    "text": "五彩斑斓的黑",
    "color": "8C00FF",
    "show": false
  },
  "CustomFlash": {
    "type": "CustomFlash",
    "rooms": [
      0
    ],
    "duration": 2,
    "ease": "Linear",
    "startColor": "FF0000FF",
    "endColor": "0000FFFF",
    "startOpacity": 1,
    "endOpacity": 1
  },
  "DesktopColor": {
    "bar": 3,
    "beat": 5,
    "y": 3,
    "type": "DesktopColor",
    "endColor": "00ffff",
    "duration": 2,
    "ease": "Linear"
  },
  "FadeRoom": {
    "bar": 4,
    "beat": 2,
    "y": 2,
    "type": "FadeRoom",
    "opacity": 100,
    "duration": 2,
    "ease": "Linear"
  },
  "FinishLevel": {
    "type": "FinishLevel"
  },
  "Flash": {
    "type": "Flash",
    "rooms": [
      0
    ],
    "duration": 1,
    "ease": "Linear",
    "startColor": "FFFFFFFF",
    "startOpacity": 1
  },
  "FlipScreen": {
    "bar": 3,
    "beat": 3,
    "y": 6,
    "type": "FlipScreen",
    "rooms": [
      0
    ],
    "flipX": false,
    "flipY": true
  },
  "FloatingText": {
    "duration": 1,
    "bar": 2,
    "beat": 5,
    "y": 2,
    "type": "FloatingText",
    "rooms": [
      0
    ],
    "id": 0,
    "text": "等/呀/等/得/好/心/慌……",
    "times": "",
    "color": "00F4FFFF",
    "outlineColor": "0011FFFF",
    "textPosition": [
      50,
      50
    ],
    "font": "Default",
    "size": 8,
    "angle": 0,
    "showChildren": true,
    "fadeOutRate": 3,
    "mode": "FadeOut",
    "anchor": "MiddleCenter",
    "narrate": true,
    "narrationCategory": "Subtitles"
  },
  "GoToLevel": {
    "type": "GoToLevel"
  },
  "HideRow": {
    "type": "HideRow",
    "row": 0
  },
  "HideWindow": {
    "bar": 3,
    "beat": 8,
    "y": 3,
    "type": "HideWindow",
    "show": false
  },
  "InvertColors": {
    "bar": 3,
    "beat": 4,
    "y": 6,
    "type": "InvertColors",
    "rooms": [
      0
    ],
    "enable": true
  },
  "LinkRows": {
    "type": "LinkRows",
    "rows": [
      0,
      1
    ]
  },
  "MaskRoom": {
    "type": "MaskRoom",
    "rooms": [
      0
    ],
    "sourceRoom": 0
  },
  "Move": {
    "bar": 2,
    "beat": 3,
    "y": 1,
    "type": "Move",
    "position": [
      50,
      50
    ],
    "duration": 1,
    "ease": "Linear"
  },
  "MoveCamera": {
    "duration": 1,
    "bar": 3,
    "beat": 1,
    "y": 0,
    "type": "MoveCamera",
    "rooms": [
      0
    ],
    "cameraPosition": [
      50,
      50
    ],
    "ease": "Linear"
  },
  "MoveRoom": {
    "bar": 1,
    "beat": 1,
    "y": 0,
    "type": "MoveRoom",
    "if": "1d0",
    "roomPosition": [
      50,
      50
    ],
    "scale": [
      70,
      100
    ],
    "angle": 20,
    "pivot": [
      0,
      0
    ],
    "duration": 1,
    "ease": "Linear"
  },
  "MoveRow": {
    "type": "MoveRow",
    "row": 0,
    "position": [
      50,
      50
    ],
    "duration": 1,
    "ease": "Linear"
  },
  "NarrateRowInfo": {
    "bar": 3,
    "beat": 8,
    "y": 4,
    "type": "NarrateRowInfo",
    "row": 0,
    "infoType": "Connect",
    "soundOnly": false,
    "narrateSkipBeats": "On",
    "skipsUnstable": false,
    "customPlayer": "AutoDetect"
  },
  "NewWindowDance": {
    "bar": 2,
    "beat": 3,
    "y": 2,
    "type": "NewWindowDance",
    "tab": "Windows",
    "preset": "Move",
    "position": [
      50,
      50
    ],
    "reference": "Center",
    "easingDuration": 1,
    "ease": "Linear"
  },
  "PaintHands": {
    "bar": 4,
    "beat": 1,
    "y": 2,
    "type": "PaintHands",
    "rooms": [
      0
    ],
    "hands": "Right",
    "border": "Outline",
    "borderColor": "AFFF00FF",
    "tint": true,
    "tintColor": "4FAF00FF",
    "opacity": 100,
    "duration": 0
  },
  "PlayAnimation": {
    "bar": 2,
    "beat": 3,
    "y": 3,
    "type": "PlayAnimation",
    "expression": "neutral"
  },
  "PlayExpression": {
    "type": "PlayExpression",
    "expression": "neutral"
  },
  "PlaySong": {
    "bar": 4,
    "beat": 2,
    "y": 4,
    "type": "PlaySong",
    "song": {
      "filename": "sndOrientalTechno"
    },
    "bpm": 100,
    "loop": false
  },
  "PlaySound": {
    "bar": 4,
    "beat": 7,
    "y": 3,
    "type": "PlaySound",
    "sound": {
      "filename": "Shaker"
    },
    "customSoundType": "CueSound"
  },
  "PulseCamera": {
    "bar": 3,
    "beat": 1,
    "y": 2,
    "type": "PulseCamera",
    "rooms": [
      0
    ],
    "strength": 1,
    "count": 1,
    "frequency": 1
  },
  "PulseFreeTimeBeat": {
    "bar": 3,
    "beat": 5.5,
    "y": -1,
    "type": "PulseFreeTimeBeat",
    "row": 0,
    "action": "Increment",
    "hold": 0
  },
  "ReadNarration": {
    "bar": 5,
    "beat": 1,
    "y": 5,
    "type": "ReadNarration",
    "text": "一名身穿红色盔甲的武士跟着音乐节拍点头。",
    "category": "Description"
  },
  "RenameWindow": {
    "bar": 4,
    "beat": 6,
    "y": 6,
    "type": "RenameWindow",
    "action": "Set",
    "text": "Minecraft 1.21.1 NeoForge"
  },
  "ReorderDecoration": {
    "type": "ReorderDecoration",
    "order": [
      1,
      3,
      2,
      0
    ]
  },
  "ReorderRooms": {
    "bar": 1,
    "beat": 2,
    "y": 1,
    "type": "ReorderRooms",
    "order": [
      1,
      3,
      2,
      0
    ]
  },
  "ReorderRow": {
    "type": "ReorderRow",
    "order": [
      1,
      3,
      2,
      0
    ]
  },
  "ReorderWindows": {
    "type": "ReorderWindows",
    "order": [
      1,
      3,
      2,
      0
    ]
  },
  "SayReadyGetSetGo": {
    "bar": 2,
    "beat": 2,
    "y": 5,
    "type": "SayReadyGetSetGo",
    "tag": "1",
    "rooms": [
      4
    ],
    "phraseToSay": "JustSayRea",
    "voiceSource": "Nurse",
    "tick": 1,
    "volume": 100
  },
  "SetBackgroundColor": {
    "bar": 1,
    "beat": 5,
    "y": 2,
    "type": "SetBackgroundColor",
    "rooms": [
      0
    ],
    "backgroundType": "Color",
    "color": "FFFFFFFF",
    "filter": "NearestNeighbor"
  },
  "SetBeatSound": {
    "bar": 2,
    "beat": 8,
    "y": 3,
    "type": "SetBeatSound",
    "row": 0,
    "sound": {
      "filename": "Shaker"
    }
  },
  "SetBeatsPerMinute": {
    "bar": 1,
    "beat": 5,
    "y": 1,
    "type": "SetBeatsPerMinute",
    "if": "~3d0",
    "beatsPerMinute": 100
  },
  "SetClapSounds": {
    "bar": 5,
    "beat": 1,
    "y": 3,
    "type": "SetClapSounds",
    "rowType": "Classic",
    "p1Sound": {
      "filename": "ClapHit"
    }
  },
  "SetCountingSound": {
    "bar": 1,
    "beat": 8,
    "y": 4,
    "type": "SetCountingSound",
    "voiceSource": "JyiCount",
    "enabled": true,
    "volume": 100,
    "subdivOffset": 0.5
  },
  "SetCrotchetsPerBar": {
    "bar": 2,
    "beat": 7,
    "y": 5,
    "type": "SetCrotchetsPerBar",
    "crotchetsPerBar": 7,
    "visualBeatMultiplier": 1
  },
  "SetFont": {
    "type": "SetFont",
    "font": "DinkieBitmap"
  },
  "SetForeground": {
    "type": "SetForeground",
    "image": [
      "Assets/assets.png"
    ],
    "color": "FFFFFFFF"
  },
  "SetGameSound": {
    "bar": 1,
    "beat": 5,
    "y": 5,
    "type": "SetGameSound",
    "soundType": "SmallMistake",
    "filename": ""
  },
  "SetHandOwner": {
    "bar": 2,
    "beat": 7,
    "y": 6,
    "type": "SetHandOwner",
    "rooms": [
      0
    ],
    "hand": "Right",
    "character": "Player"
  },
  "SetHeartExplodeInterval": {
    "bar": 1,
    "beat": 5,
    "y": 6,
    "type": "SetHeartExplodeInterval",
    "intervalType": "GatherAndCeil",
    "interval": 1
  },
  "SetHeartExplodeVolume": {
    "type": "SetHeartExplodeVolume",
    "volume": 100
  },
  "SetMainWindow": {
    "type": "SetMainWindow",
    "window": 0
  },
  "SetOneshotWave": {
    "bar": 2,
    "beat": 3,
    "y": 4,
    "type": "SetOneshotWave",
    "waveType": "BoomAndRush",
    "height": 100,
    "width": 100
  },
  "SetPlayStyle": {
    "bar": 2,
    "beat": 6,
    "y": 4,
    "type": "SetPlayStyle",
    "PlayStyle": "Normal",
    "NextBar": 1,
    "Relative": true
  },
  "SetRoomContentMode": {
    "bar": 4,
    "beat": 7,
    "y": 5,
    "type": "SetRoomContentMode",
    "mode": "Center"
  },
  "SetRoomPerspective": {
    "bar": 2,
    "beat": 6,
    "y": 3,
    "type": "SetRoomPerspective",
    "cornerPositions": [
      [
        0,
        0
      ],
      [
        100,
        0
      ],
      [
        0,
        100
      ],
      [
        100,
        100
      ]
    ],
    "duration": 1,
    "ease": "Linear"
  },
  "SetRowXs": {
    "pattern": "-x--x-",
    "syncoBeat": 3,
    "syncoSwing": 0,
    "bar": 4,
    "beat": 2,
    "y": 6,
    "type": "SetRowXs",
    "syncoVolume": 70,
    "syncoPitch": 100,
    "syncoStyle": "Chirp",
    "syncoPlayModifierSound": true,
    "syncoPlayModifierOffSound": true
  },
  "SetSpeed": {
    "duration": 0,
    "ease": "Linear",
    "bar": 1,
    "beat": 2,
    "y": 5,
    "type": "SetSpeed",
    "speed": 1
  },
  "SetText": {
    "type": "SetText",
    "id": 0,
    "rooms": [
      0
    ],
    "text": "Hello",
    "color": "FFFFFFFF",
    "outlineColor": "000000FF",
    "duration": 1
  },
  "SetTheme": {
    "bar": 4,
    "beat": 7,
    "y": 4,
    "type": "SetTheme",
    "rooms": [
      0
    ],
    "preset": "None",
    "skipPaintEffects": false
  },
  "SetVfxPreset": {
    "bar": 4,
    "beat": 7,
    "y": 2,
    "type": "SetVfxPreset",
    "rooms": [
      0
    ],
    "preset": "Vignette",
    "enable": true
  },
  "SetVisible": {
    "bar": 1,
    "beat": 8,
    "y": 1,
    "type": "SetVisible",
    "visible": true
  },
  "SetWindowContent": {
    "bar": 4,
    "beat": 1,
    "y": 6,
    "type": "SetWindowContent",
    "contentMode": "OnTop"
  },
  "ShakeScreen": {
    "type": "ShakeScreen",
    "rooms": [
      0
    ],
    "duration": 1,
    "ease": "Linear",
    "strength": 100
  },
  "ShakeScreenCustom": {
    "type": "ShakeScreenCustom",
    "rooms": [
      0
    ],
    "duration": 1,
    "ease": "Linear",
    "strength": 100,
    "shakeX": 10,
    "shakeY": 10
  },
  "ShowDialogue": {
    "bar": 2,
    "beat": 5,
    "y": 1,
    "type": "ShowDialogue",
    "rooms": [
      4
    ],
    "text": "Samurai_Happy: SAMURAI!",
    "speed": 1,
    "panelSide": "Bottom",
    "portraitSide": "Left",
    "playTextSounds": true
  },
  "ShowHands": {
    "bar": 3,
    "beat": 1,
    "y": 1,
    "type": "ShowHands",
    "rooms": [
      0
    ],
    "hand": "Right",
    "action": "Show",
    "extent": "Full",
    "forceRaise": true,
    "align": false,
    "instant": false
  },
  "ShowRooms": {
    "bar": 4,
    "beat": 5,
    "y": 2,
    "type": "ShowRooms",
    "rooms": [
      1,
      3
    ],
    "heights": [
      0,
      0,
      0,
      0
    ],
    "transitionTime": 1,
    "ease": "InOutSine"
  },
  "ShowStatusSign": {
    "bar": 1,
    "beat": 8,
    "y": 2,
    "type": "ShowStatusSign",
    "if": "1d0",
    "text": "RhythmBase.RhythmDoctor.View!",
    "duration": 5,
    "useBeats": true,
    "narrate": true
  },
  "SpinningRows": {
    "type": "SpinningRows",
    "row": 0,
    "duration": 1
  },
  "Stutter": {
    "type": "Stutter",
    "rooms": [
      0
    ],
    "duration": 1,
    "ease": "Linear"
  },
  "TagAction": {
    "bar": 4,
    "beat": 8,
    "y": 6,
    "type": "TagAction",
    "Action": "Run",
    "Tag": "Nothing"
  },
  "TextExplosion": {
    "bar": 1,
    "beat": 8,
    "y": 5,
    "type": "TextExplosion",
    "rooms": [
      0
    ],
    "text": "Explosion",
    "color": "000000",
    "mode": "OneColor",
    "direction": "Left",
    "speed": 100,
    "ease": "Linear"
  },
  "Tile": {
    "type": "Tile",
    "rooms": [
      0
    ],
    "tile": 0
  },
  "Tint": {
    "bar": 1,
    "beat": 8,
    "y": 3,
    "type": "Tint",
    "border": "Glow",
    "borderColor": "FF0000FF",
    "tint": true,
    "tintColor": "001BFFFF",
    "opacity": 100,
    "duration": 3,
    "ease": "Linear"
  },
  "TintRows": {
    "bar": 2,
    "beat": 8,
    "y": 4,
    "type": "TintRows",
    "if": "1&~2d1",
    "rooms": [
      0
    ],
    "border": "Outline",
    "borderColor": "2BFF00FF",
    "tint": true,
    "tintColor": "007E0EFF",
    "opacity": 100,
    "duration": 0,
    "ease": "Linear",
    "effect": "None"
  },
  "TintText": {
    "type": "TintText",
    "tintColor": "FFFFFFFF",
    "borderColor": "FF0000FF"
  },
  "WindowResize": {
    "type": "WindowResize",
    "rooms": [
      0
    ],
    "duration": 1,
    "ease": "Linear",
    "scale": [
      100,
      100
    ]
  }
};
