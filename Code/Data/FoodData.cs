using System;

namespace vcrossing.Code.Data;

[GlobalClass]
public sealed partial class FoodData : ItemData, ICraftableData
{
	[Export] public bool IsCraftingIngredient { get; set; } = false;
	[Export] public Godot.Collections.Array<string> Aliases { get; set; }
}
