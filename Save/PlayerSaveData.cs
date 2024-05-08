using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;
using vcrossing.Inventory;
using vcrossing.Player;

namespace vcrossing.Save;

public class PlayerSaveData : BaseSaveData
{
	
	[JsonInclude] public string PlayerName { get; set; }
	[JsonInclude] public List<InventoryItem> Items = new();

	public void AddPlayer( PlayerController playerNode )
	{
		var inventory = playerNode.GetNode<Player.Inventory>( "PlayerInventory" );
		foreach ( var item in inventory.Items )
		{
			Items.Add( item );
		}
		PlayerName = playerNode.Name;
		GD.Print( "Added player to save data" );
	}
	
}
