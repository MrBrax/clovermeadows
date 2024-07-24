using vcrossing.Code.Items;

namespace vcrossing.Code.Data;

public interface ICraftableData
{

	public Godot.Collections.Array<string> Aliases { get; set; }

	public bool IsCraftingIngredient { get; set; }

}
