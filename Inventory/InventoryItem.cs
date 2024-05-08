using Godot;
using vcrossing.DTO;

namespace vcrossing.Inventory;

public partial class InventoryItem
{
	
	// public ItemData ItemData { get; set; }
	public string ItemDataPath { get; set; }
	public BaseDTO DTO { get; set; }
	public int Quantity { get; set; }
	
	public ItemData GetItemData()
	{
		return GD.Load<ItemData>( ItemDataPath );
	}
	
	/*public InventoryItem()
	{
		
	}*/
	
	/*public InventoryItem( ItemData itemData, int quantity )
	{
		ItemData = itemData;
		Quantity = quantity;
	}*/
	
	/*public InventoryItem( WorldItem worldItem )
	{
		ItemData = worldItem.GetItemData();
		Quantity = worldItem.Quantity;
	}*/
	
}
