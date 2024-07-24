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


}
