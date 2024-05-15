using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code.Carriable;
using vcrossing2.Code.DTO;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Items;
using vcrossing2.Code.Persistence;
using vcrossing2.Inventory;

namespace vcrossing2.Code.Player;

public class InventorySlot<TItem> where TItem : PersistentItem
{
	[JsonInclude] public TItem _item;

	public InventorySlot( Inventory inventory )
	{
		Inventory = inventory;
	}

	public InventorySlot()
	{
	}


	[JsonIgnore] public Code.Player.Inventory Inventory { get; set; }
	
	[JsonIgnore] public bool HasItem => _item != null;
	
	public void SetItem( TItem item )
	{
		_item = item;
		Inventory.OnChange();
	}
	
	public TItem GetItem()
	{
		return _item;
	}
	
	public T GetItem<T>() where T : TItem
	{
		return (T) _item;
	}
	
	public void RemoveItem()
	{
		_item = null;
		Inventory.OnChange();
		// Inventory.Player.Save();
	}
	
	public void Drop()
	{
		Logger.Info( "Dropping item" );
		var position = Inventory.PlayerInteract.GetAimingGridPosition();
		var playerRotation = Inventory.World.GetItemRotationFromDirection( Inventory.World.Get4Direction( Inventory.PlayerModel.RotationDegrees.Y ) );
		
		try
		{
			// Inventory.World.SpawnDroppedItem( _item.GetItemData(), position, World.ItemPlacement.Floor, playerRotation );
			Inventory.World.SpawnPersistentNode( _item, position, playerRotation, World.ItemPlacement.Floor, true );
		}
		catch ( System.Exception e )
		{
			Logger.Info( e );
			return;
		}

		// Items.Remove( item );
		RemoveItem();
		// Inventory.World.Save();

		// Inventory.GetNode<PlayerController>( "../" ).Save();
	}

	public void Place()
	{
		Logger.Info( "Placing item" );
		var position = Inventory.PlayerInteract.GetAimingGridPosition();
		var playerRotation = Inventory.World.GetItemRotationFromDirection( Inventory.World.Get4Direction( Inventory.PlayerModel.RotationDegrees.Y ) );
		
		try
		{
			// Inventory.World.SpawnPlacedItem<PlacedItem>( _item.GetItemData(), position, World.ItemPlacement.Floor,
			// 	playerRotation );
			Inventory.World.SpawnPersistentNode( _item, position, playerRotation, World.ItemPlacement.Floor, false );
		}
		catch ( System.Exception e )
		{
			Logger.Info( e );
			return;
		}

		// Items.Remove( item );
		RemoveItem();
		// Inventory.World.Save();

		// Inventory.Player.Save();
	}

	public void Equip()
	{
		
		if (Inventory.Player.CurrentCarriable != null)
		{
			throw new System.Exception("Player already has an equipped item.");
		}
		
		if ( Inventory.Player.Equip == null ) throw new System.Exception( "Player equip node is null." );
		
		/*var itemScene = GetItem().GetItemData().CarryScene;
		
		if ( itemScene == null )
		{
			throw new System.Exception( "Item does not have a carry scene." );
		}*/

		/*if ( GetItem().DTO is not BaseCarriableDTO dto )
		{
			throw new System.Exception( "Item DTO is not a BaseCarriableDTO." );
		}*/
		
		// var dto = GetItem().GetDTO<BaseCarriableDTO>();

		var itemDataPath = GetItem().ItemDataPath;
		
		if ( string.IsNullOrEmpty( itemDataPath ) )
		{
			throw new System.Exception( "Item data path is empty." );
		}

		var item = GetItem().CreateCarry();
		item.ItemDataPath = itemDataPath;
		item.Inventory = Inventory;
		
		Inventory.Player.Equip.AddChild( item );
		Inventory.Player.CurrentCarriable = item;
		
		item.Position = Vector3.Zero;
		item.RotationDegrees = new Vector3( 0, 0, 0 );
		
		RemoveItem();
		// Inventory.Player.Save();
	}

	public void Bury()
	{
		
		var pos = Inventory.Player.Interact.GetAimingGridPosition();
		var floorItem = Inventory.World.GetItem( pos, World.ItemPlacement.Floor );
		if ( floorItem.Node is not Hole hole )
		{
			return;
		}
		
		Inventory.World.SpawnPersistentNode( _item, pos, World.ItemRotation.North, World.ItemPlacement.Underground, true );
		
		RemoveItem();

		// Inventory.World.RemoveItem( floorItem );
		floorItem.QueueFree();

	}
}
