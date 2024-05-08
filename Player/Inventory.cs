using System.Collections.Generic;
using System.Linq;
using Godot;
using vcrossing.Carriable;
using vcrossing.Inventory;

namespace vcrossing.Player;

public partial class Inventory : Node3D
{
	public BaseCarriable CurrentCarriable;

	public List<InventoryItem> Items = new();

	private World World => GetNode<World>( "/root/Main/World" );
	private PlayerController Player => GetNode<PlayerController>( "../" );
	private Node3D PlayerModel => GetNode<Node3D>( "../PlayerModel" );
	private PlayerInteract PlayerInteract => GetNode<PlayerInteract>( "../PlayerInteract" );

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
		GD.Print( "Dropping item" );
		var position = PlayerInteract.GetAimingGridPosition();
		var playerRotation = World.GetItemRotationFromDirection( World.Get4Direction( PlayerModel.RotationDegrees.Y ) );
		try
		{
			World.SpawnPlacedItem( item.GetItemData(), position, World.ItemPlacement.Floor, playerRotation );
		} catch ( System.Exception e )
		{
			GD.Print( e );
			return;
		}

		Items.Remove( item );
		World.Save();

		GetNode<PlayerController>( "../" ).Save();
	}

	public override void _Ready()
	{
		// CurrentCarriable = GetNode<BaseCarriable>( "CurrentCarriable" );

		// Items.Add( new InventoryItem( GD.Load<ItemData>( "res://items/misc/hole.tres" ), 1 ) );
	}

	public override void _Process( double delta )
	{
		if ( Input.IsActionJustPressed( "UseTool" ) )
		{
			if ( CurrentCarriable != null )
			{
				CurrentCarriable.OnUse( Player );
			}
		}
		else if ( Input.IsActionJustPressed( "Drop" ) )
		{
			DropItem( Items.FirstOrDefault() );
		}
	}
}
