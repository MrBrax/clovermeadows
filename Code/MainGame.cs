using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using vcrossing.Code.Data;
using YarnSpinnerGodot;

namespace vcrossing.Code;

public partial class MainGame : Node3D
{

	public Dictionary<string, ShopInventoryData> Shops = new();

	public override void _Ready()
	{
		base._Ready();

		GenerateAllShopData();

		SetupDialogue();
	}

	private string _dialogueText;

	private void SetupDialogue()
	{
		var runner = GetNode<DialogueRunner>( "/root/Main/UserInterface/YarnSpinnerCanvasLayer/DialogueRunner" );
		var lineView = GetNode<LineView>( "/root/Main/UserInterface/YarnSpinnerCanvasLayer/LineView" );
		var speechPlayer = GetNode<AudioStreamPlayer>( "/root/Main/UserInterface/YarnSpinnerCanvasLayer/Speech" );

		lineView.onCharacterTyped += () =>
		{
			var currentLetter = lineView.lineText.Text[lineView.lineText.VisibleCharacters - 1].ToString().Trim();
			Logger.Info( $"YarnSpinner typed: {currentLetter}" );
			if ( currentLetter == " " ) return;
			// TODO: play sound for each letter typed

			// only match letters
			if ( !Regex.IsMatch( currentLetter, @"[a-zA-Z]" ) )
			{
				Logger.Info( $"YarnSpinner typed: {currentLetter} is not a letter" );
				return;
			}

			Logger.Info( $"YarnSpinner say letter: {currentLetter}" );
			speechPlayer.Stream = Loader.LoadResource<AudioStream>( $"res://sound/speech/alphabet/{currentLetter.ToUpper()}.wav" );
			speechPlayer.Play();

		};

		runner.onCommand += ( command ) =>
		{
			Logger.Info( $"YarnSpinner command: {command}" );
		};

		runner.onDialogueComplete += () =>
		{
			Logger.Info( "YarnSpinner dialogue complete" );
		};
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
				// re-add random itemdata
				foreach ( var item in loadedShopData.Items )
				{
					item.ItemData = Loader.LoadResource<ItemData>( item.ItemDataPath );
				}

				// re-add static itemdata
				foreach ( var item in loadedShopData.StaticItems )
				{
					item.ItemData = Loader.LoadResource<ItemData>( item.ItemDataPath );
				}

				Shops.Add( id, loadedShopData );
				return;
			}

		}

		var inventoryData = new ShopInventoryData( id );
		// shopData.AddItem( Loader.LoadResource<ItemData>( "res://items/furniture/armchair/armchair.tres" ) );
		Shops.Add( id, inventoryData );

		var shopData = Loader.LoadResource<ShopData>( $"res://shops/{id}.tres" );
		var itemCount = shopData.MaxItems;

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
