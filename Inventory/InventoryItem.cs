using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code;
using vcrossing2.Code.DTO;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;

namespace vcrossing2.Inventory;

public partial class InventoryItem<T> where T : BaseDTO
{
	
	// public ItemData ItemData { get; set; }
	public string ItemDataPath { get; set; }
	public T DTO { get; set; }
	public int Quantity { get; set; }
	
	[JsonIgnore] public Code.Player.Inventory Inventory { get; set; }
	[JsonIgnore] public InventorySlot Slot { get; set; }
	
	public InventoryItem()
	{
		
	}
	
	public InventoryItem( Code.Player.Inventory inventory )
	{
		Inventory = inventory;
	}
	
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
