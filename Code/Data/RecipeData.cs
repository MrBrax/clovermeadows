using vcrossing.Code.Inventory;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Data;

[GlobalClass]
public sealed partial class RecipeData : Resource
{

	[Export] public Godot.Collections.Array<RecipeEntryData> Ingredients { get; set; } = [];

	[Export] public Godot.Collections.Array<RecipeEntryData> Results { get; set; } = [];


	public List<PersistentItem> GetResults()
	{
		List<PersistentItem> results = new List<PersistentItem>();

		foreach ( RecipeEntryData entry in Results )
		{
			var itemData = entry.Item ?? ItemData.GetById( entry.ItemId );
			for ( int i = 0; i < entry.Quantity; i++ )
			{
				results.Add( PersistentItem.Create( itemData ) );
			}
		}

		return results;
	}

	public bool HasIngredients( InventoryContainer container )
	{
		foreach ( RecipeEntryData entry in Ingredients )
		{
			/* if ( !container.HasItem( entry.Item, entry.Quantity ) )
			{
				return false;
			} */
			var slot = container.GetSlotWithItem( entry.Item, entry.Quantity );
			if ( slot == null )
			{
				return false;
			}
		}

		return true;
	}

	public void TakeIngredients( InventoryContainer container )
	{
		foreach ( RecipeEntryData entry in Ingredients )
		{
			container.RemoveItem( entry.Item, entry.Quantity );
		}
	}


}
