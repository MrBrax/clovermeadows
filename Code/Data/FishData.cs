using System;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class FishData : AnimalData
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
		Sea = 1,
		Pond = 1 << 1,
		River = 1 << 2,
	}

	public enum FishSize
	{
		Tiny,
		Small,
		Medium,
		Large,
	}

	[Export] public float WeightMin { get; set; } = 1;
	[Export] public float WeightMax { get; set; } = 1;

	// [Export] public int BaseSellPrice { get; set; } = 0;

	[Export] public FishLocation Location { get; set; } = FishLocation.River;

	[Export] public FishSize Size { get; set; } = FishSize.Small;

	public float GetRandomWeight()
	{
		return WeightMin + (GD.Randf() * (WeightMax - WeightMin));
	}

}
