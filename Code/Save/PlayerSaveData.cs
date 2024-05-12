using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code.Carriable;
using vcrossing2.Code.DTO;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;
using vcrossing2.Inventory;

namespace vcrossing2.Code.Save;

public class PlayerSaveData : BaseSaveData
{
	[JsonInclude] public string PlayerName { get; set; }
	[JsonInclude] public List<InventorySlot> InventorySlots = new();
	[JsonInclude] public BaseCarriableDTO Carriable { get; set; }

	public void AddPlayer( PlayerController playerNode )
	{
		var inventory = playerNode.GetNode<Player.Inventory>( "PlayerInventory" );
		foreach ( var item in inventory.GetSlots() )
		{
			InventorySlots.Add( item );
		}

		if ( playerNode.CurrentCarriable != null )
		{
			GD.Print( "Player current carriable is not null" );
			Carriable = playerNode.CurrentCarriable.DTO;
		}
		else
		{
			GD.PushWarning( "Player current carriable is null" );
			Carriable = null;
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
		InventorySlots = saveData.InventorySlots;
		Carriable = saveData.Carriable;

		GD.Print( "Loaded save data from file" );

		return true;
	}

	public void LoadPlayer( PlayerController playerController )
	{
		var inventory = playerController.GetNode<Player.Inventory>( "PlayerInventory" );
		inventory.RemoveSlots();
		foreach ( var slot in InventorySlots )
		{
			// inventory.Items.Add( item );
			inventory.ImportSlot( slot );
		}

		if ( Carriable != null )
		{
			if ( !string.IsNullOrEmpty( Carriable.ItemDataPath ) )
			{
				var carriableItemData = GD.Load<ItemData>( Carriable.ItemDataPath );
				var carriable = carriableItemData.CarryScene.Instantiate<BaseCarriable>();
				carriable.DTO = Carriable;
				carriable.Inventory = inventory;
				playerController.Equip.AddChild( carriable );
				playerController.CurrentCarriable = carriable;
			}
			else
			{
				GD.PushWarning( "Carriable item data path is empty" );
			}
		}

		GD.Print( "Loaded player from save data" );
	}
}
