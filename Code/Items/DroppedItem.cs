using Godot;

namespace vcrossing;

public partial class DroppedItem : WorldItem
{
	// [Export] public string ItemDataPath;

	public ItemData ItemData => ResourceLoader.Load<ItemData>( ItemDataPath );
	
	public int Quantity { get; set; } = 1;
	
	public DroppedItem()
	{
		
	}
	
	public DroppedItem( ItemData itemData )
	{
		ItemDataPath = itemData.ResourcePath;
	}
	
}
