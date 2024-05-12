using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code.Carriable;
using vcrossing2.Code.DTO;
using vcrossing2.Code.Items;
using vcrossing2.Inventory;

namespace vcrossing2.Code.Player;

public class InventorySlot<T> where T : BaseDTO
{
	[JsonInclude] internal InventoryItem<T> _item;

	public InventorySlot( Inventory inventory )
	{
		Inventory = inventory;
	}

	public InventorySlot()
	{
	}


	[JsonIgnore] public Code.Player.Inventory Inventory { get; set; }
	
	[JsonIgnore] public bool HasItem => _item != null;
	
	public void SetItem( InventoryItem<BaseDTO> item )
	{
		_item = item;
		Inventory.OnChange();
	}
	
	public InventoryItem<T> GetItem()
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

		Inventory.Player.Save();
	}

	public void Equip()
	{
		
		if (Inventory.Player.CurrentCarriable != null)
		{
			throw new System.Exception("Player already has an equipped item.");
		}
		
		var itemScene = GetItem().GetItemData().CarryScene;
		
		if ( itemScene == null )
		{
			throw new System.Exception( "Item does not have a carry scene." );
		}

		/*if ( GetItem().DTO is not BaseCarriableDTO dto )
		{
			throw new System.Exception( "Item DTO is not a BaseCarriableDTO." );
		}*/
		
		// var dto = GetItem().GetDTO<BaseCarriableDTO>();
		
		var item = itemScene.Instantiate<BaseCarriable>();
		
		if ( item.DTO is not BaseCarriableDTO dto )
		{
			throw new System.Exception( "Item DTO is not a BaseCarriableDTO." );
		}
		
		item.DTO = dto;
		
		item.Inventory = Inventory;
		
		Inventory.Player.Equip.AddChild( item );
		Inventory.Player.CurrentCarriable = item;
		// item.Transform = new Transform3D( Basis.Identity, Inventory.Player.GlobalPosition + Inventory.Player.GlobalTransform.Basis.Z * 1 );
		// item.GlobalPosition = Inventory.Player.GlobalPosition + Inventory.Player.GlobalTransform.Basis.Z * 0.5f +
		//                       Vector3.Up * 1f;
		item.Position = Vector3.Zero;
		item.RotationDegrees = new Vector3( 0, 0, 0 );
		
		RemoveItem();
		Inventory.Player.Save();
	}
}
