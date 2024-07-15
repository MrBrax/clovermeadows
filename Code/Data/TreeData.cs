using System;

namespace vcrossing.Code.Data;

[GlobalClass]
public sealed partial class TreeData : ItemData
{

	[Export] public ItemData Fruit { get; set; }


}
