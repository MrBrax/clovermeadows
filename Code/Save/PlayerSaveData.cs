using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code.Player;
using vcrossing2.Inventory;

namespace vcrossing2.Code.Save;

public class PlayerSaveData : BaseSaveData
{
	
	[JsonInclude] public string PlayerName { get; set; }
	[JsonInclude] public List<InventoryItem> Items = new();

	public void AddPlayer( PlayerController playerNode )
	{
		var inventory = playerNode.GetNode<Player.Inventory>( "PlayerInventory" );
		foreach ( var item in inventory.GetItems() )
		{
			Items.Add( item );
		}
		PlayerName = playerNode.Name;
		GD.Print( "Added player to save data" );
	}
	
	public bool LoadFile( string filePath )
	{
		if ( !FileAccess.FileExists( filePath ) )
		{
			// throw new System.Exception( $"File {filePath} does not exist" );
			GD.PushWarning( $"File {filePath} does not exist" );
			return false;
		}

		using var file = FileAccess.Open( filePath, FileAccess.ModeFlags.Read );
		var json = file.GetAsText();
		var saveData =
			JsonSerializer.Deserialize<PlayerSaveData>( json, new JsonSerializerOptions { IncludeFields = true, } );

		PlayerName = saveData.PlayerName;
		Items = saveData.Items;
		
		GD.Print( "Loaded save data from file" );
		
		return true;
		
	}

	public void LoadPlayer( PlayerController playerController )
	{
		var inventory = playerController.GetNode<Player.Inventory>( "PlayerInventory" );
		foreach ( var item in Items )
		{
			// inventory.Items.Add( item );
			inventory.AddItem( item );
		}
		GD.Print( "Loaded player from save data" );
	}
}
