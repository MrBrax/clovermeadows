namespace vcrossing.Inventory;

public partial class InventoryItem
{
	
	public ItemData ItemData { get; set; }
	public int Quantity { get; set; }
	
	public InventoryItem()
	{
		
	}
	
	public InventoryItem( ItemData itemData, int quantity )
	{
		ItemData = itemData;
		Quantity = quantity;
	}
	
	/*public InventoryItem( WorldItem worldItem )
	{
		ItemData = worldItem.GetItemData();
		Quantity = worldItem.Quantity;
	}*/
	
}
