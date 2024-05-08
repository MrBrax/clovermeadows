using System.Collections.Generic;
using Godot;
using vcrossing.Carriable;
using vcrossing.Inventory;

namespace vcrossing.Player;

public partial class Inventory : Node3D
{
	public BaseCarriable CurrentCarriable;

	public List<InventoryItem> Items = new();
	
	private World World => GetNode<World>( "/root/Main/World" );
	private PlayerController Player => GetNode<PlayerController>( "../Player" );
	private PlayerInteract PlayerInteract => GetNode<PlayerInteract>( "../Player/PlayerInteract" );
	
	public void PickUpItem( WorldItem item )
	{
		var inventoryItem = new InventoryItem();
		item.UpdateDTO();
		
		inventoryItem.ItemDataPath = item.ItemDataPath;
		inventoryItem.DTO = item.DTO;
		Items.Add( inventoryItem );
		// item.QueueFree();
		World.RemoveItem( item );
		GD.Print( "Picked up item" );
		
		GetNode<PlayerController>( "../" ).Save();
	}
	
	public void DropItem( InventoryItem item )
	{
		var position = PlayerInteract.GetAimingGridPosition();
		World.SpawnDroppedItem( item.GetItemData(), position, World.ItemPlacement.Floor, World.ItemRotation.North );
	}

	public override void _Ready()
	{
		// CurrentCarriable = GetNode<BaseCarriable>( "CurrentCarriable" );
		
		// Items.Add( new InventoryItem( GD.Load<ItemData>( "res://items/misc/hole.tres" ), 1 ) );
	}

	public override void _Input( InputEvent @event )
	{
		if ( @event is InputEventAction eventAction )
		{
			if ( eventAction.Action == "Use" )
			{
				if ( CurrentCarriable != null )
				{
					CurrentCarriable.OnUse( GetNode<PlayerController>( "../Player" ) );
				}
			}
		}
	}
}
