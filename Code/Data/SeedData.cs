using System;

namespace vcrossing.Code.Data;

[GlobalClass]
public sealed partial class SeedData : ItemData
{

	[Export] public ItemData SpawnedItemData { get; set; }

}
