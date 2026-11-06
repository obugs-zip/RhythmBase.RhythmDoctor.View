using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhythmBase.Global.Components;
using RhythmBase.RhythmDoctor.Events;
using RhythmBase.RhythmDoctor.View;
using RhythmBase.RhythmDoctor.View.Test1;

//Tests.Run2(new DesktopColor()
//{
//	StartColor = Color.Blue,
//	Active = true,
//	Duration = 4,
//},
//new()
//{
//	Scale = 2,
//	Active = true,
//	Enabled = true,
//	Hover = false,
//	ShowDuration = true,
//});

var a =Stopwatch.StartNew();

//Tests.Run4_2(@"E:\Download\Whispers of the Water (Demo Ver.) - Hexatunes, Monopoupa by Caprisk.rdzip");
Tests.Run4(@"E:\Download\P - Mind Brand.rdzip");
//Tests.Run3();//Tests.Run4_2(@"E:\Download\Time Traveler - KNOWER by 3T, Diagamma.rdzip");

Console.WriteLine($"{a.ElapsedMilliseconds}ms");