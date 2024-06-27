using System;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using static vcrossing.Code.Data.ShopInventoryData;

namespace vcrossing.Code.WorldBuilder;

[GlobalClass]
public partial class StoreManager : Node3D
{

	[Export] public string ShopId { get; set; }

	[Export] public Godot.Collections.Array<ShopDisplay> ShopDisplays { get; set; }

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

		// TODO: Implement this

		/* int i = 0;
		foreach ( var shopDisplay in randomShopDisplays )
		{
			if ( i >= shop.Items.Count )
			{
				break;
			}

			var item = shop.Items[i];
			if ( IsItemBeingDisplayed( item ) )
			{
				continue;
			}

			var itemData = ResourceLoader.Load<ItemData>( item.ItemDataPath );
			if ( itemData == null )
			{
				Logger.LogError( $"Item data not found for {item.ItemDataPath}" );
				continue;
			}

			if ( itemData.Width > shopDisplay.TileSize || itemData.Height > shopDisplay.TileSize )
			{
				Logger.LogError( $"Item {itemData.Name} is too big for shop display {shopDisplay.Name}" );
				continue;
			}

			shopDisplay.Item = item;
			shopDisplay.SpawnModel();
			i++;

		} */
	}


}
