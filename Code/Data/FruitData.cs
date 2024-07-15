using System;

namespace vcrossing.Code.Data;

[GlobalClass]
public sealed partial class FruitData : ItemData
{

	[Export] public PackedScene InTreeScene { get; set; }


}
