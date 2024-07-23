using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Components;
using vcrossing.Code.Items;

namespace vcrossing.Code.Data;

public sealed partial class ShopInventoryData
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

	[JsonInclude] public DateTime Created { get; set; } = NodeManager.TimeManager.Time;

	// shop data is valid only the same day it was created
	[JsonIgnore] public bool IsValid => Created.Date == NodeManager.TimeManager.Time.Date;

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

	// [JsonInclude] public List<ShopItem> ShopTallDisplayItems = new();

	// [JsonInclude] public List<ShopItem> Items = new();

	// [JsonInclude] public List<ShopItem> StaticItems = new();

	public bool IsInStock( string itemId )
	{
		// return Items.FirstOrDefault( i => i.ItemDataPath == item )?.Stock > 0 || StaticItems.FirstOrDefault( i => i.ItemDataPath == item )?.Stock > 0;
		return ShopDisplayItems.FirstOrDefault( i => i.Value?.ItemDataId == itemId || i.Value?.ItemDataName == itemId ).Value?.Stock > 0;
	}

	public bool IsInStock( ItemData item )
	{
		return IsInStock( item.Id );
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

}
