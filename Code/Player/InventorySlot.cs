using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code.Items;
using vcrossing2.Inventory;

namespace vcrossing2.Code.Player;

public class InventorySlot
{
	[JsonInclude] internal InventoryItem _item;

	public InventorySlot( Inventory inventory )
	{
		Inventory = inventory;
	}

	public InventorySlot()
	{
	}


	[JsonIgnore] public Code.Player.Inventory Inventory { get; set; }
	
	[JsonIgnore] public bool HasItem => _item != null;
	
	public void SetItem( InventoryItem item )
	{
		_item = item;
		Inventory.OnChange();
	}
	
	public InventoryItem GetItem()
	{
		return _item;
	}
	
	public void RemoveItem()
	{
		_item = null;
		Inventory.OnChange();
	}
	
	public void Drop()
	{
		GD.Print( "Dropping item" );
		var position = Inventory.PlayerInteract.GetAimingGridPosition();
		var playerRotation = Inventory.World.GetItemRotationFromDirection( Inventory.World.Get4Direction( Inventory.PlayerModel.RotationDegrees.Y ) );
		
		try
		{
			Inventory.World.SpawnDroppedItem( _item.GetItemData(), position, World.ItemPlacement.Floor, playerRotation );
		}
		catch ( System.Exception e )
		{
			GD.Print( e );
			return;
		}

		// Items.Remove( item );
		RemoveItem();
		Inventory.World.Save();

		Inventory.GetNode<PlayerController>( "../" ).Save();
	}

	public void Place()
	{
		GD.Print( "Placing item" );
		var position = Inventory.PlayerInteract.GetAimingGridPosition();
		var playerRotation = Inventory.World.GetItemRotationFromDirection( Inventory.World.Get4Direction( Inventory.PlayerModel.RotationDegrees.Y ) );
		
		try
		{
			Inventory.World.SpawnPlacedItem<PlacedItem>( _item.GetItemData(), position, World.ItemPlacement.Floor,
				playerRotation );
		}
		catch ( System.Exception e )
		{
			GD.Print( e );
			return;
		}

		// Items.Remove( item );
		RemoveItem();
		Inventory.World.Save();

		Inventory.GetNode<PlayerController>( "../" ).Save();
	}

}
