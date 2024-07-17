using System;

namespace vcrossing.Code.Data;

[GlobalClass]
public sealed partial class PlantData : ItemData
{

	[Export] public PackedScene PlantedScene { get; set; }

}
