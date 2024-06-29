using System;
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

	private bool IsItemBeingDisplayed( ShopItem item )
	{
		return ShopDisplays.Any( x => x.Item == item );
	}

	public override void _Ready()
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
	}


}
