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

		GenerateShopData();
	}

	private void GenerateShopData()
	{

		DirAccess.MakeDirAbsolute( "user://shops" );

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
		file.StoreString( data );
	}
}
