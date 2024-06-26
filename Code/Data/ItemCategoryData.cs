using System;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class ItemCategoryData : Resource
{

	[Export] public string Name;

	[Export] public Godot.Collections.Array<ItemData> Items = new();


}
