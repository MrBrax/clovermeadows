using System;
using System.Text.Json;
using vcrossing.Code.Data;

namespace vcrossing.Code;

public partial class MainGame : Node3D
{

	public Dictionary<string, ShopInventoryData> Shops = new();

	public override void _Ready()
	{
		base._Ready();

		GenerateAllShopData();
	}

	private void GenerateAllShopData()
	{

		DirAccess.MakeDirAbsolute( "user://shops" );

		GenerateShopData( "shop" );

		/* DirAccess.MakeDirAbsolute( "user://shops" );

		var path = "user://shops/shop.json";
		if ( FileAccess.FileExists( "user://shops/shop.json" ) )
		{
			var textData = FileAccess.Open( path, FileAccess.ModeFlags.Read ).GetAsText();
			var loadedShopData = JsonSerializer.Deserialize<ShopInventoryData>( textData );

			if ( loadedShopData.IsValid )
			{
				Shops.Add( "shop", loadedShopData );
				return;
			}

		}

		var shopData = new ShopInventoryData( "Shop" );
		shopData.AddItem( Loader.LoadResource<ItemData>( "res://items/furniture/armchair/armchair.tres" ) );
		Shops.Add( "shop", shopData );

		var data = JsonSerializer.Serialize( Shops["shop"], new JsonSerializerOptions { WriteIndented = true, } );
		using var file = FileAccess.Open( path, FileAccess.ModeFlags.Write );
		file.StoreString( data ); */
	}

	private void GenerateShopData( string id )
	{
		var path = $"user://shops/{id}.json";
		if ( FileAccess.FileExists( path ) )
		{
			var textData = FileAccess.Open( path, FileAccess.ModeFlags.Read ).GetAsText();
			var loadedShopData = JsonSerializer.Deserialize<ShopInventoryData>( textData );

			// if we're still on the same day, continue using saved data
			if ( loadedShopData.IsValid )
			{
				Shops.Add( id, loadedShopData );
				return;
			}

		}

		var inventoryData = new ShopInventoryData( id );
		// shopData.AddItem( Loader.LoadResource<ItemData>( "res://items/furniture/armchair/armchair.tres" ) );
		Shops.Add( id, inventoryData );

		var shopData = Loader.LoadResource<ShopData>( $"res://shops/{id}.tres" );
		var itemCount = shopData.MaxItems;

		while ( itemCount > 0 )
		{
			var item = shopData.Categories.PickRandom().Items.PickRandom();
			if ( inventoryData.IsInStock( item ) ) continue;
			inventoryData.AddItem( item );
			itemCount--;
		}

		var data = JsonSerializer.Serialize( Shops[id], new JsonSerializerOptions { WriteIndented = true, } );
		using var file = FileAccess.Open( path, FileAccess.ModeFlags.Write );
		file.StoreString( data );

	}

	public void SaveShops()
	{
		foreach ( var shop in Shops )
		{
			var path = $"user://shops/{shop.Key}.json";
			var data = JsonSerializer.Serialize( shop.Value, new JsonSerializerOptions { WriteIndented = true, } );
			using var file = FileAccess.Open( path, FileAccess.ModeFlags.Write );
			file.StoreString( data );
		}
	}
}
