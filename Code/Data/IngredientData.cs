using System;

namespace vcrossing.Code.Data;

[GlobalClass]
public sealed partial class IngredientData : ItemData
{

	[Export] public Godot.Collections.Array<string> Aliases { get; set; }

}
