using System.Collections.Generic;
using System.Linq;
using Godot;
using vcrossing2.Code.Carriable;
using vcrossing2.Code.Items;
using vcrossing2.Inventory;

namespace vcrossing2.Code.Player;

public partial class Inventory : Node3D
{
	public BaseCarriable CurrentCarriable;

	private List<InventoryItem> Items = new();

	private World World => GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;
	private PlayerController Player => GetNode<PlayerController>( "../" );
	private Node3D PlayerModel => GetNode<Node3D>( "../PlayerModel" );
	private PlayerInteract PlayerInteract => GetNode<PlayerInteract>( "../PlayerInteract" );

	public delegate void InventoryChanged();

	public event InventoryChanged OnInventoryChanged;

	public void AddItem( InventoryItem item )
	{
		Items.Add( item );
		OnInventoryChanged?.Invoke();
	}
	
	public void RemoveItem( InventoryItem item )
	{
		Items.Remove( item );
		OnInventoryChanged?.Invoke();
	}
	
	public IEnumerable<InventoryItem> GetItems()
	{
		return Items;
	}

	public void PickUpItem( WorldItem item )
	{
		var inventoryItem = new InventoryItem();
		item.UpdateDTO();

		inventoryItem.ItemDataPath = item.ItemDataPath;
		inventoryItem.DTO = item.DTO;
		// inventoryItem.Quantity = item.Quantity;
		AddItem( inventoryItem );
		World.RemoveItem( item );
		GD.Print( "Picked up item" );

		World.Save();

		GetNode<PlayerController>( "../" ).Save();
	}

	public void DropItem( InventoryItem item )
	{
		GD.Print( "Dropping item" );
		var position = PlayerInteract.GetAimingGridPosition();
		var playerRotation = World.GetItemRotationFromDirection( World.Get4Direction( PlayerModel.RotationDegrees.Y ) );
		try
		{
			World.SpawnDroppedItem( item.GetItemData(), position, World.ItemPlacement.Floor, playerRotation );
		}
		catch ( System.Exception e )
		{
			GD.Print( e );
			return;
		}

		// Items.Remove( item );
		RemoveItem( item );
		World.Save();

		GetNode<PlayerController>( "../" ).Save();
	}

	public void PlaceItem( InventoryItem item )
	{
		GD.Print( "Placing item" );
		var position = PlayerInteract.GetAimingGridPosition();
		var playerRotation = World.GetItemRotationFromDirection( World.Get4Direction( PlayerModel.RotationDegrees.Y ) );
		try
		{
			World.SpawnPlacedItem<PlacedItem>( item.GetItemData(), position, World.ItemPlacement.Floor,
				playerRotation );
		}
		catch ( System.Exception e )
		{
			GD.Print( e );
			return;
		}

		// Items.Remove( item );
		RemoveItem( item );
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
			var item = Items.FirstOrDefault();
			if ( item != null )
			{
				DropItem( item );
			}
		}
	}
}
