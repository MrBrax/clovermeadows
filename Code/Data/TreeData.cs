using System;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class TreeData : ItemData
{

	[Export] public ItemData Fruit { get; set; }


}
