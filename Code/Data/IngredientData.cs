using System;

namespace vcrossing.Code.Data;

[GlobalClass]
public sealed partial class IngredientData : ItemData, ICraftableData
{

	[Export] public Godot.Collections.Array<string> Aliases { get; set; }
	[Export] public bool IsCraftingIngredient { get; set; } = true;

}
