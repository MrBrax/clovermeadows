using System;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class AnimalData : ItemData
{

	[Export] public PackedScene LiveScene { get; set; }

	[Export] public int MonthStart { get; set; } = 1;
	[Export] public int MonthEnd { get; set; } = 12;

	[Export] public int TimeStart { get; set; } = 0;
	[Export] public int TimeEnd { get; set; } = 24;


}
