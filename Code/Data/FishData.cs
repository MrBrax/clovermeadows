using System;

namespace vcrossing2.Code.Items;

[GlobalClass]
public partial class FishData : ItemData
{

	/* public enum FishRarity
	{
		Common,
		Uncommon,
		Rare,
		Legendary
	} */

	[Flags]
	public enum FishLocation
	{
		Sea,
		Pond,
		River,
	}

	public enum FishSize
	{
		Tiny,
		Small,
		Medium,
		Large,
	}

	[Export] public int MonthStart { get; set; } = 1;
	[Export] public int MonthEnd { get; set; } = 12;

	[Export] public int TimeStart { get; set; } = 0;
	[Export] public int TimeEnd { get; set; } = 24;

	[Export] public int WeightMin { get; set; } = 1;
	[Export] public int WeightMax { get; set; } = 1;

	[Export] public int BaseSellPrice { get; set; } = 0;

	[Export] public FishLocation Location { get; set; } = FishLocation.River;

	[Export] public FishSize Size { get; set; } = FishSize.Small;



}
