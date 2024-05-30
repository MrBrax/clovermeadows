using Godot;

namespace vcrossing2.Code.Items;

public partial class DroppedItem : WorldItem
{
	// [Export] public string ItemDataPath;
	
	// [Export] public override World.ItemPlacementType PlacementType => World.ItemPlacementType.Dropped;

	public ItemData ItemData => Loader.LoadResource<ItemData>( ItemDataPath );
	
	public int Quantity { get; set; } = 1;
	
	public DroppedItem()
	{
		
	}
	
	public DroppedItem( ItemData itemData )
	{
		ItemDataPath = itemData.ResourcePath;
	}
	
}
