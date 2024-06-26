using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Components;

namespace vcrossing.Code.Data;


public partial class ShopInventoryData
{

	/* [Export] public Godot.Collections.Array<ItemData> Items = new();

	[Export] public Godot.Collections.Array<string> BoughtItems = new();

	public bool HasBoughtItem( string item )
	{
		return BoughtItems.Contains( item );
	}

	public bool HasBoughtItem( ItemData item )
	{
		return HasBoughtItem( item.ResourcePath );
	} */

	public ShopInventoryData()
	{
		Name = "Shop";
	}

	public ShopInventoryData( string name )
	{
		Name = name;
	}

	[JsonInclude] public string Name { get; set; }

	[JsonInclude] public DateTime Created { get; set; } = DateTime.Now;

	// shop data is valid only the same day it was created
	[JsonIgnore] public bool IsValid => Created.Date == DateTime.Now.Date;

	public partial class ShopItem
	{
		[JsonInclude] public string ItemDataPath;
		[JsonInclude] public int Price;
		[JsonInclude] public int Stock;
	}

	[JsonInclude] public List<ShopItem> Items = new();

	public bool IsInStock( string item )
	{
		return Items.FirstOrDefault( i => i.ItemDataPath == item )?.Stock > 0;
	}

	public bool IsInStock( ItemData item )
	{
		return IsInStock( item.ResourcePath );
	}

	public void AddItem( ItemData itemData )
	{
		// TODO: proper buy price
		Items.Add( new ShopItem { ItemDataPath = itemData.ResourcePath, Price = itemData.BaseSellPrice, Stock = 1 } );
		Logger.Info( "ShopData", $"Added item {itemData.ResourcePath} to shop {this.Name}" );
	}

	public void AddItem( ItemData itemData, int price )
	{
		// TODO: proper buy price
		Items.Add( new ShopItem { ItemDataPath = itemData.ResourcePath, Price = price, Stock = 1 } );
		Logger.Info( "ShopData", $"Added item {itemData.ResourcePath} to shop {this.Name}" );
	}

	public void AddItem( ItemData itemData, int price, int stock )
	{
		Items.Add( new ShopItem { ItemDataPath = itemData.ResourcePath, Price = price, Stock = stock } );
		Logger.Info( "ShopData", $"Added item {itemData.ResourcePath} to shop {this.Name}" );
	}
}
