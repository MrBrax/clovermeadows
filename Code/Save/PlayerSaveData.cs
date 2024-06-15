using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using vcrossing.Code.Helpers;
using vcrossing.Code.Inventory;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using BaseCarriable = vcrossing.Code.Carriable.BaseCarriable;

namespace vcrossing.Code.Save;

public partial class PlayerSaveData : BaseSaveData
{
	[JsonIgnore] public string PlayerId { get; set; }
	[JsonInclude] public string PlayerName { get; set; }
	[JsonInclude] public List<InventorySlot<PersistentItem>> InventorySlots = new();
	// [JsonInclude] public PersistentItem Carriable { get; set; }
	[JsonInclude] public Dictionary<PlayerController.EquipSlot, PersistentItem> EquippedItems = new();

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
		InventorySlots.Clear();
		var inventory = playerNode.GetNode<Player.Inventory>( "PlayerInventory" );
		foreach ( var item in inventory.GetUsedSlots() )
		{
			InventorySlots.Add( item );
		}

		// Carriable = playerNode.CurrentCarriable != null ? PersistentItem.Create( playerNode.CurrentCarriable ) : null;

		EquippedItems.Clear();
		foreach ( var (slot, item) in playerNode.EquippedItems )
		{
			EquippedItems.Add( slot, PersistentItem.Create( item ) );
		}

		PlayerName = playerNode.Name;
		Logger.Info( "Added player to save data" );
	}

	public static PlayerSaveData LoadFile( string filePath )
	{
		if ( !FileAccess.FileExists( filePath ) )
		{
			// throw new System.Exception( $"File {filePath} does not exist" );
			Logger.Warn( $"File {filePath} does not exist" );
			return null;
		}

		using var file = FileAccess.Open( filePath, FileAccess.ModeFlags.Read );
		var json = file.GetAsText();
		var saveData =
			JsonSerializer.Deserialize<PlayerSaveData>( json, new JsonSerializerOptions { IncludeFields = true, } );

		/* PlayerId = saveData.PlayerId;
		PlayerName = saveData.PlayerName;
		InventorySlots = saveData.InventorySlots;
		Carriable = saveData.Carriable; */

		Logger.Info( "Loaded save data from file" );

		return saveData;
	}

	public void LoadPlayer( PlayerController playerController )
	{
		var inventory = playerController.GetNode<Player.Inventory>( "PlayerInventory" );
		inventory.RemoveSlots();

		if ( InventorySlots.Count > inventory.MaxItems )
		{
			Logger.LogError( $"Imported inventory slots count is greater than max items: {InventorySlots.Count} > {inventory.MaxItems}" );
			InventorySlots = InventorySlots.Take( inventory.MaxItems ).ToList();
		}

		foreach ( var slot in InventorySlots )
		{
			// inventory.Items.Add( item );
			inventory.ImportSlot( slot );
		}

		// add missing slots
		while ( inventory.GetUsedSlots().Count() < inventory.MaxItems )
		{
			Logger.Debug( "Adding missing slot to inventory" );
			inventory.ImportSlot( new InventorySlot<PersistentItem>() );
		}

		/* if ( Carriable != null && !string.IsNullOrEmpty( Carriable.ItemDataPath ) )
		{
			var carriable = Carriable.Create<BaseCarriable>();
			if ( carriable != null )
			{
				carriable.Inventory = inventory;
				playerController.Equip.AddChild( carriable );
				playerController.CurrentCarriable = carriable;
				carriable.OnEquip( playerController );
			}
			else
			{
				GD.PushError( "Failed to create carriable" );
			}
		} */

		foreach ( var (slot, item) in EquippedItems )
		{
			var carriable = item.Create<BaseCarriable>();
			if ( carriable != null )
			{
				carriable.Inventory = inventory;
				playerController.ToolEquip.AddChild( carriable );
				playerController.EquippedItems[slot] = carriable;
				carriable.OnEquip( playerController );
			}
			else
			{
				Logger.LogError( "Failed to create carriable" );
			}
		}

		Logger.Info( "Loaded player from save data" );
	}
}
