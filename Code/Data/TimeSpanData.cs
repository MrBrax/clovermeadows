using System;
using Godot.Collections;
using vcrossing.Code.Components;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class TimeSpanData : Resource
{

	[Export( PropertyHint.Range, "1,12" )] public int MonthStart = 1;

	[Export( PropertyHint.Range, "1,31" )] public int DayStart = 1;

	[Export( PropertyHint.Range, "1,12" )] public int MonthEnd = 12;
	[Export( PropertyHint.Range, "1,31" )] public int DayEnd = 31;

}
