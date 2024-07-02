using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Components;
using vcrossing.Code.Items;

namespace vcrossing.Code.Data;


public partial class ShopInventoryData
{

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
		[JsonInclude, Obsolete] public string ItemDataPath;
		[JsonInclude] public string ItemDataId;
		[JsonInclude] public string ItemDataName;
		[JsonInclude] public int Price;
		[JsonInclude] public int Stock;
		[JsonInclude] public int CustomCategory;

		[JsonIgnore] public ItemData ItemData;
	}

	[JsonInclude] public Dictionary<string, ShopItem> ShopDisplayItems = new();

	// [JsonInclude] public List<ShopItem> Items = new();

	// [JsonInclude] public List<ShopItem> StaticItems = new();

	public bool IsInStock( string item )
	{
		// return Items.FirstOrDefault( i => i.ItemDataPath == item )?.Stock > 0 || StaticItems.FirstOrDefault( i => i.ItemDataPath == item )?.Stock > 0;
		return ShopDisplayItems.FirstOrDefault( i => i.Value?.ItemDataId == item || i.Value?.ItemDataName == item ).Value?.Stock > 0;
	}

	public bool IsInStock( ItemData item )
	{
		return IsInStock( item.ResourcePath );
	}

	public ShopItem AddItem( ShopDisplay display, ItemData itemData )
	{
		// TODO: proper buy price
		var item = new ShopItem
		{
			// ItemDataPath = itemData.ResourcePath,
			ItemDataId = itemData.Id,
			ItemDataName = itemData.ResourcePath.GetFile().GetBaseName(),
			Price = itemData.BaseBuyPrice,
			Stock = 1,
			ItemData = itemData
		};
		ShopDisplayItems.Add( display.Name, item );
		return item;
		// Logger.Info( "ShopData", $"Added item {itemData.ResourcePath} to shop {this.Name}" );
	}

	/* public ShopItem AddItem( ItemData itemData )
	{
		// TODO: proper buy price
		var item = new ShopItem
		{
			ItemDataPath = itemData.ResourcePath,
			Price = itemData.BaseBuyPrice,
			Stock = 1,
			ItemData = itemData
		};
		Items.Add( item );
		return item;
		// Logger.Info( "ShopData", $"Added item {itemData.ResourcePath} to shop {this.Name}" );
	}

	public ShopItem AddItem( ItemData itemData, int price )
	{
		// TODO: proper buy price
		var item = new ShopItem
		{
			ItemDataPath = itemData.ResourcePath,
			Price = price,
			Stock = 1,
			ItemData = itemData
		};
		Items.Add( item );
		return item;
		// Logger.Info( "ShopData", $"Added item {itemData.ResourcePath} to shop {this.Name}" );
	}

	public ShopItem AddItem( ItemData itemData, int price, int stock )
	{
		var item = new ShopItem
		{
			ItemDataPath = itemData.ResourcePath,
			Price = price,
			Stock = stock,
			ItemData = itemData
		};
		Items.Add( item );
		return item;
		// Logger.Info( "ShopData", $"Added item {itemData.ResourcePath} to shop {this.Name}" );
	}

	public ShopItem AddStaticItem( ItemData itemData )
	{
		var item = new ShopItem
		{
			ItemDataPath = itemData.ResourcePath,
			Price = itemData.BaseBuyPrice,
			Stock = 1,
			ItemData = itemData
		};
		StaticItems.Add( item );
		return item;
		// Logger.Info( "ShopData", $"Added static item {itemData.ResourcePath} to shop {this.Name}" );
	}

	public ShopItem AddStaticItem( ItemData itemData, int price )
	{
		var item = new ShopItem
		{
			ItemDataPath = itemData.ResourcePath,
			Price = price,
			Stock = 1,
			ItemData = itemData
		};
		StaticItems.Add( item );
		return item;
		// Logger.Info( "ShopData", $"Added static item {itemData.ResourcePath} to shop {this.Name}" );
	}

	public ShopItem AddStaticItem( ItemData itemData, int price, int stock )
	{
		var item = new ShopItem
		{
			ItemDataPath = itemData.ResourcePath,
			Price = price,
			Stock = stock,
			ItemData = itemData
		};
		StaticItems.Add( item );
		return item;
		// Logger.Info( "ShopData", $"Added static item {itemData.ResourcePath} to shop {this.Name}" );
	} */

}
