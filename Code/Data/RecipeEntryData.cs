namespace vcrossing.Code.Data;

[GlobalClass]
public sealed partial class RecipeEntryData : Resource
{

	[Export] public string ItemId { get; set; }
	[Export] public ItemData Item { get; set; }
	[Export] public int Quantity { get; set; } = 1;


}
