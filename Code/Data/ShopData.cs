using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Components;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class ShopData : Resource
{

	[Export] public string Name { get; set; }

	[Export] public Godot.Collections.Array<ItemCategoryData> Categories = new();

	[Export] public int MaxItems { get; set; } = 1;

}
