﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Persistence;
using vcrossing2.Code.Player;
using BaseCarriable = vcrossing2.Code.Carriable.BaseCarriable;

namespace vcrossing2.Code.Save;

public class PlayerSaveData : BaseSaveData
{
	[JsonIgnore] public string PlayerId { get; set; }
	[JsonInclude] public string PlayerName { get; set; }
	[JsonInclude] public List<InventorySlot<PersistentItem>> InventorySlots = new();
	[JsonInclude] public PersistentItem Carriable { get; set; }

	public PlayerSaveData()
	{
		// PlayerId = Guid.NewGuid().ToString();
	}

	public PlayerSaveData( string playerId )
	{
		PlayerId = playerId;
	}

	public void AddPlayer( PlayerController playerNode )
	{
		var inventory = playerNode.GetNode<Player.Inventory>( "PlayerInventory" );
		foreach ( var item in inventory.GetSlots() )
		{
			InventorySlots.Add( item );
		}

		Carriable = playerNode.CurrentCarriable != null ? PersistentItem.Create( playerNode.CurrentCarriable ) : null;

		PlayerName = playerNode.Name;
		Logger.Info( "Added player to save data" );
	}

	public bool LoadFile( string filePath )
	{
		if ( !FileAccess.FileExists( filePath ) )
		{
			// throw new System.Exception( $"File {filePath} does not exist" );
			Logger.Warn( $"File {filePath} does not exist" );
			return false;
		}

		using var file = FileAccess.Open( filePath, FileAccess.ModeFlags.Read );
		var json = file.GetAsText();
		var saveData =
			JsonSerializer.Deserialize<PlayerSaveData>( json, new JsonSerializerOptions { IncludeFields = true, } );

		PlayerId = saveData.PlayerId;
		PlayerName = saveData.PlayerName;
		InventorySlots = saveData.InventorySlots;
		Carriable = saveData.Carriable;

		Logger.Info( "Loaded save data from file" );

		return true;
	}

	public void LoadPlayer( PlayerController playerController )
	{
		var inventory = playerController.GetNode<Player.Inventory>( "PlayerInventory" );
		inventory.RemoveSlots();

		if ( InventorySlots.Count > inventory.MaxItems )
		{
			Logger.LogError( $"Imported inventory slots count is greater than max items: {InventorySlots.Count} > {inventory.MaxItems}" );
		}
		else
		{
			foreach ( var slot in InventorySlots )
			{
				// inventory.Items.Add( item );
				inventory.ImportSlot( slot );
			}

		}

		// add missing slots
		while ( inventory.GetSlots().Count() < inventory.MaxItems )
		{
			Logger.Debug( "Adding missing slot to inventory" );
			inventory.ImportSlot( new InventorySlot<PersistentItem>() );
		}

		if ( Carriable != null && !string.IsNullOrEmpty( Carriable.ItemDataPath ) )
		{
			var carriable = Carriable.Create<BaseCarriable>();
			if ( carriable != null )
			{
				carriable.Inventory = inventory;
				playerController.Equip.AddChild( carriable );
				playerController.CurrentCarriable = carriable;
			}
			else
			{
				GD.PushError( "Failed to create carriable" );
			}
			/*if ( !string.IsNullOrEmpty( Carriable.ItemDataPath ) )
			{
				var carriableItemData = GD.Load<ItemData>( Carriable.ItemDataPath );
				var carriable = carriableItemData.CarryScene.Instantiate<BaseCarriable>();
				// carriable.DTO = Carriable;
				carriable.Inventory = inventory;
				playerController.Equip.AddChild( carriable );
				playerController.CurrentCarriable = carriable;
			}
			else
			{
				Logger.Warn( "Carriable item data path is empty" );
			}*/
		}

		Logger.Info( "Loaded player from save data" );
	}
}
