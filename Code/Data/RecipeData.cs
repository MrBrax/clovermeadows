using System;
using vcrossing.Code.Inventory;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Data;

[GlobalClass]
public sealed partial class RecipeData : Resource
{

	[Export] public string Name { get; set; }
	[Export] public string Description { get; set; }

	[Export] public Godot.Collections.Array<RecipeEntryData> Ingredients { get; set; } = [];

	[Export] public Godot.Collections.Array<RecipeEntryData> Results { get; set; } = [];

	[Export] public CraftingStation.CraftingStationType CraftingStation { get; internal set; }

	public bool IsValid => Ingredients.Count > 0 && Results.Count > 0;

	public List<PersistentItem> GetResults()
	{
		List<PersistentItem> results = [];

		foreach ( RecipeEntryData entry in Results )
		{
			var itemData = entry.GetItem();
			if ( itemData == null ) throw new Exception( $"Item not found: {entry.ItemId} ({entry.Item}) in recipe {Name}. Please check the recipe data." );
			for ( int i = 0; i < entry.Quantity; i++ )
			{
				results.Add( PersistentItem.Create( itemData ) );
			}
		}

		return results;
	}

	public bool HasIngredients( InventoryContainer container )
	{

		if ( Ingredients.Count == 0 )
		{
			throw new Exception( $"Recipe {Name} has no ingredients." );
			return true;
		}

		foreach ( RecipeEntryData entry in Ingredients )
		{
			/* if ( !container.HasItem( entry.Item, entry.Quantity ) )
			{
				return false;
			} */
			var slot = container.GetSlotWithItem( entry.GetItem(), entry.Quantity );
			if ( slot == null )
			{
				Logger.Verbose( "RecipeData", $"Missing ingredient: {entry.GetItem()} x{entry.Quantity}" );
				return false;
			}
			Logger.Verbose( "RecipeData", $"Has ingredient: {entry.GetItem()} x{entry.Quantity}" );
		}

		Logger.Verbose( "RecipeData", $"Has ingredients for {Name}" );

		return true;
	}

	public void TakeIngredients( InventoryContainer container )
	{
		foreach ( RecipeEntryData entry in Ingredients )
		{
			container.RemoveItem( entry.GetItem(), entry.Quantity );
		}
	}

	internal string GetDescription()
	{
		if ( !string.IsNullOrWhiteSpace( Description ) )
		{
			return Description;
		}

		var results = GetResults();

		return string.Join( ", ", results.Select( r => r.ItemData.Name ) );
	}


	internal string GetDisplayName()
	{
		if ( !string.IsNullOrWhiteSpace( Name ) )
		{
			return Name;
		}

		var results = GetResults();

		return string.Join( ", ", results.Select( r => r.GetName() ) );

	}

	internal bool HasIngredient( RecipeEntryData ingredient, InventoryContainer container )
	{
		var slot = container.GetSlotWithItem( ingredient.GetItem(), ingredient.Quantity );
		return slot != null;
	}
}
