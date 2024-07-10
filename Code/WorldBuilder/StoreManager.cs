using System;
using System.Text.Json;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Npc;
using static vcrossing.Code.Data.ShopInventoryData;

namespace vcrossing.Code.WorldBuilder;

[GlobalClass]
public partial class StoreManager : Node3D
{

	[Export] public string ShopId { get; set; }

	[Export] public Godot.Collections.Array<ShopDisplay> ShopDisplays { get; set; }

	[Export] public Shopkeeper Shopkeeper { get; set; }

	[Export] public Godot.Collections.Array<ShopTallDisplay> TallDisplays { get; set; }

	private ShopInventoryData ShopData = new();

	private bool IsItemBeingDisplayed( ShopItem item )
	{
		return ShopDisplays.Any( x => x.Item == item );
	}

	public override void _Ready()
	{
		GenerateShopData();
	}

	private void GenerateShopData()
	{

		DirAccess.MakeDirAbsolute( "user://shops" );

		foreach ( var display in TallDisplays )
		{
			var category = display.Category;
			var items = new List<ShopItem>();
			foreach ( var item in category.Items )
			{
				items.Add( new ShopItem
				{
					ItemData = item,
					ItemDataId = item.Id,
					ItemDataName = item.Name,
					Price = item.BaseBuyPrice,
					Stock = 999, // TODO: set stock
				} );
			}

			display.Items = items;
		}

		var path = $"user://shops/{ShopId}.json";
		if ( FileAccess.FileExists( path ) )
		{
			var textData = FileAccess.Open( path, FileAccess.ModeFlags.Read ).GetAsText();
			var loadedShopData = JsonSerializer.Deserialize<ShopInventoryData>( textData );

			// if we're still on the same day, continue using saved data
			if ( loadedShopData.IsValid )
			{
				// re-add random itemdata
				foreach ( var dict in loadedShopData.ShopDisplayItems )
				{
					dict.Value.ItemData = ResourceManager.Instance.LoadItemFromId<ItemData>( dict.Value.ItemDataId );

					bool found = false;
					foreach ( var display in ShopDisplays )
					{
						if ( display.Name == dict.Key )
						{
							display.StoreManager = this;
							display.Item = dict.Value;
							display.SpawnModel();
							found = true;
							// break;
						}
					}

					if ( !found )
					{
						Logger.LogError( $"StoreManager", $"Failed to find display {dict.Key}" );
					}
				}

				// re-add static itemdata
				/* foreach ( var item in loadedShopData.StaticItems )
				{
					item.ItemData = Loader.LoadResource<ItemData>( item.ItemDataPath );
				} */

				ShopData = loadedShopData;
				return;
			}

		}

		var inventoryData = new ShopInventoryData( ShopId );
		// shopData.AddItem( Loader.LoadResource<ItemData>( "res://items/furniture/armchair/armchair.tres" ) );
		ShopData = inventoryData;

		var shopData = Loader.LoadResource<ShopData>( $"res://shops/{ShopId}.tres" );
		/* var itemCount = shopData.MaxItems;

		// add random items to the shop
		while ( itemCount > 0 )
		{
			var item = shopData.Categories.PickRandom().Items.PickRandom();
			if ( inventoryData.IsInStock( item ) ) continue;
			inventoryData.AddItem( item );
			itemCount--;
		}

		// add static items to the shop
		foreach ( var item in shopData.StaticItems )
		{
			inventoryData.AddStaticItem( item );
		}
 */
		// TODO: store items per display stand
		foreach ( var shopDisplay in ShopDisplays )
		{
			int tries = 0;
			do
			{
				tries++;
				if ( shopDisplay.StaticItem )
				{
					var item = shopData.StaticItems.PickRandom();
					if ( inventoryData.IsInStock( item ) ) continue;
					if ( !shopDisplay.CanDisplayItem( item ) ) continue;
					shopDisplay.Item = inventoryData.AddItem( shopDisplay, item );
				}
				else
				{
					var item = shopData.Categories.PickRandom().Items.PickRandom();
					if ( inventoryData.IsInStock( item ) ) continue;
					if ( !shopDisplay.CanDisplayItem( item ) ) continue;
					shopDisplay.Item = inventoryData.AddItem( shopDisplay, item );
				}
			} while ( shopDisplay.Item == null && tries < 10 );

			if ( shopDisplay.Item != null )
			{
				shopDisplay.StoreManager = this;
				shopDisplay.SpawnModel();
			}
			else
			{
				Logger.LogError( $"StoreManager", $"Failed to add item to display {shopDisplay.Name}" );
			}
		}

		var data = JsonSerializer.Serialize( ShopData, new JsonSerializerOptions { WriteIndented = true, } );
		using var file = FileAccess.Open( path, FileAccess.ModeFlags.Write );
		file.StoreString( data );

	}

	/* public override void _Ready()
	{
		var main = GetNode<MainGame>( "/root/Main" );
		var shop = main.Shops[ShopId];

		var randomShopDisplays = ShopDisplays.Where( x => x.StaticItem == false );
		var staticShopDisplays = ShopDisplays.Where( x => x.StaticItem == true );

		randomShopDisplays = randomShopDisplays.OrderBy( x => x.TileSize );
		staticShopDisplays = staticShopDisplays.OrderBy( x => x.TileSize );


		var randomInventoryItems = shop.Items.OrderBy( x => Math.Max( x.ItemData.Width, x.ItemData.Height ) );
		var staticInventoryItems = shop.StaticItems.OrderBy( x => Math.Max( x.ItemData.Width, x.ItemData.Height ) );

		foreach ( var item in randomInventoryItems )
		{
			if ( IsItemBeingDisplayed( item ) ) continue;

			var display = randomShopDisplays.FirstOrDefault( x => x.CanDisplayItem( item.ItemData ) && !x.HasItem );
			if ( display == null ) continue;

			display.StoreManager = this;
			display.Item = item;
			display.SpawnModel();
		}

		foreach ( var item in staticInventoryItems )
		{
			if ( IsItemBeingDisplayed( item ) ) continue;

			var display = staticShopDisplays.FirstOrDefault( x => x.CanDisplayItem( item.ItemData ) && !x.HasItem );
			if ( display == null ) continue;

			display.StoreManager = this;
			display.Item = item;
			display.SpawnModel();
		}
	} */


}
