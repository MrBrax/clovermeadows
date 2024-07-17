using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Inventory;

public sealed partial class InventorySlot<TItem> where TItem : PersistentItem
{

	public void Drop()
	{
		Logger.Info( "Dropping item" );
		var position = InventoryContainer.Player.Interact.GetAimingGridPosition();
		var playerRotation =
			InventoryContainer.Player.World.GetItemRotationFromDirection(
				InventoryContainer.Player.World.Get4Direction( InventoryContainer.Player.Model.RotationDegrees.Y ) );

		try
		{
			// Inventory.World.SpawnDroppedItem( _item.GetItemData(), position, World.ItemPlacement.Floor, playerRotation );
			InventoryContainer.Player.World.SpawnPersistentNode( _item, position, playerRotation, World.ItemPlacement.Floor, true );
		}
		catch ( System.Exception e )
		{
			Logger.Info( e );
			return;
		}

		InventoryContainer.Player.Inventory.GetNode<AudioStreamPlayer3D>( "ItemDrop" ).Play();

		// Items.Remove( item );
		Delete();
		// Inventory.World.Save();

		// Inventory.GetNode<PlayerController>( "../" ).Save();
	}

	public void Place()
	{
		Logger.Info( "Placing item" );
		var aimingGridPosition = InventoryContainer.Player.Interact.GetAimingGridPosition();
		var playerRotation =
			InventoryContainer.Player.World.GetItemRotationFromDirection(
				InventoryContainer.Player.World.Get4Direction( InventoryContainer.Player.Model.RotationDegrees.Y ) );

		var floorItem = InventoryContainer.Player.World.GetItem( aimingGridPosition, World.ItemPlacement.Floor );

		if ( floorItem != null )
		{
			var placeableNodes = floorItem.GetPlaceableNodes();
			if ( placeableNodes.Count > 0 )
			{
				/*var nodeNearestToAimPosition = placeableNodes.MinBy( n =>
					n.GlobalPosition.DistanceTo( Inventory.World.ItemGridToWorld( aimingGridPosition ) ) );
				
				var nodeGridPosition = Inventory.World.WorldToItemGrid( nodeNearestToAimPosition.GlobalPosition );
				*/
				var onTopItem = InventoryContainer.Player.World.GetItem( aimingGridPosition, World.ItemPlacement.OnTop );
				if ( onTopItem != null )
				{
					Logger.Warn( "On top item already exists." );
					return;
				}

				try
				{
					InventoryContainer.Player.World.SpawnPersistentNode( _item, aimingGridPosition, playerRotation, World.ItemPlacement.OnTop,
						false );
				}
				catch ( System.Exception e )
				{
					Logger.LogError( e.Message );
					return;
				}

				Delete();

				InventoryContainer.Player.Inventory.GetNode<AudioStreamPlayer3D>( "ItemDrop" ).Play();

				return;
			}

			Logger.Warn( "Can't place item on this position." );
			return;
		}

		try
		{
			// Inventory.World.SpawnPlacedItem<PlacedItem>( _item.GetItemData(), position, World.ItemPlacement.Floor,
			// 	playerRotation );
			InventoryContainer.Player.World.SpawnPersistentNode( _item, aimingGridPosition, playerRotation, World.ItemPlacement.Floor,
				false );
		}
		catch ( System.Exception e )
		{
			Logger.LogError( e.Message );
			return;
		}

		InventoryContainer.Player.Inventory.GetNode<AudioStreamPlayer3D>( "ItemDrop" ).Play();

		// Items.Remove( item );
		Delete();
		// Inventory.World.Save();

		// Inventory.Player.Save();
	}

	public void Equip()
	{

		Components.Equips.EquipSlot slot;

		// get slot from item
		if ( _item is BaseCarriable )
		{
			slot = Components.Equips.EquipSlot.Tool;
		}
		else if ( _item is ClothingItem clothingItem )
		{
			slot = clothingItem.EquipSlot;
			if ( slot == 0 ) throw new Exception( $"Invalid equip slot for {clothingItem} ({clothingItem.GetName()}) {clothingItem.ItemData.GetType()}" );
		}
		else
		{
			throw new Exception( $"Item {_item} is not equipable." );
		}


		PersistentItem currentEquip = null;
		if ( InventoryContainer.Player.Equips.HasEquippedItem( slot ) )
		{
			currentEquip = PersistentItem.Create( InventoryContainer.Player.Equips.GetEquippedItem( slot ) );
		}

		if ( _item is BaseCarriable carriable )
		{
			var carriableNode = carriable.Create();
			InventoryContainer.Player.Equips.SetEquippedItem( Components.Equips.EquipSlot.Tool, carriableNode );
		}
		else if ( _item is ClothingItem clothingItem )
		{
			var clothingNode = clothingItem.Create();
			InventoryContainer.Player.Equips.SetEquippedItem( slot, clothingNode );
		}

		var currentIndex = Index;

		// remove this item from inventory, making place for the previously equipped item
		Delete();

		// if there was a previously equipped item, add it back to the inventory
		if ( currentEquip != null )
		{
			InventoryContainer.AddItem( currentEquip, currentIndex );
		}

	}

	public void Bury()
	{
		var pos = InventoryContainer.Player.Interact.GetAimingGridPosition();
		var floorItem = InventoryContainer.Player.World.GetItem( pos, World.ItemPlacement.Floor );
		if ( floorItem.Node is not Hole hole )
		{
			return;
		}

		// spawn item underground
		InventoryContainer.Player.World.SpawnPersistentNode( _item, pos, World.ItemRotation.North, World.ItemPlacement.Underground,
			true );

		// remove hole so it isn't obstructing the dirt that will be spawned next
		InventoryContainer.Player.World.RemoveItem( hole );

		// spawn dirt on top
		InventoryContainer.Player.World.SpawnNode( Loader.LoadResource<ItemData>( ResourceManager.Instance.GetItemPathByName( "item:buried_item" ) ), pos,
			World.ItemRotation.North, World.ItemPlacement.Floor, false );

		Delete();

		// Inventory.World.RemoveItem( floorItem );
	}

	public void Plant()
	{
		var pos = InventoryContainer.Player.Interact.GetAimingGridPosition();
		var floorItem = InventoryContainer.Player.World.GetItem( pos, World.ItemPlacement.Floor );
		if ( floorItem.Node is not Hole hole )
		{
			return;
		}

		if ( _item.ItemData is not SeedData seedData ) return;

		var plantItemData = seedData.SpawnedItemData;

		if ( plantItemData == null )
		{
			throw new System.Exception( "Seed data does not have a spawned item data." );
		}

		// remove hole so it isn't obstructing the dirt that will be spawned next
		InventoryContainer.Player.World.RemoveItem( hole );

		InventoryContainer.Player.World.SpawnNode( plantItemData, pos, World.ItemRotation.North, World.ItemPlacement.Floor );

		Delete();

		// Inventory.World.RemoveItem( floorItem );
	}

	public void SetWallpaper()
	{

		if ( _item.ItemData is not WallpaperData wallpaperData )
		{
			throw new System.Exception( "Item data is not a wallpaper data." );
		}

		var interiorManager = InventoryContainer.Player.World.GetNodesOfType<InteriorManager>().FirstOrDefault();

		if ( interiorManager == null || !GodotObject.IsInstanceValid( interiorManager ) )
		{
			throw new System.Exception( "Interior manager not found." );
		}

		// TODO: get which room to set the wallpaper for
		interiorManager.SetWallpaper( "first", wallpaperData );

	}

	public void SetFlooring()
	{

		if ( _item.ItemData is not FlooringData floorData )
		{
			throw new System.Exception( "Item data is not a flooring data." );
		}

		var interiorManager = InventoryContainer.Player.World.GetNodesOfType<InteriorManager>().FirstOrDefault();

		if ( interiorManager == null || !GodotObject.IsInstanceValid( interiorManager ) )
		{
			throw new System.Exception( "Interior manager not found." );
		}

		// TODO: get which room to set the wallpaper for
		interiorManager.SetFloor( "first", floorData );

	}

	public void Eat()
	{

		// TODO: check with some kind of interface if the item is edible
		if ( _item.ItemData is not FruitData foodData )
		{
			throw new System.Exception( "Item data is not a food data." );
		}

		InventoryContainer.Player.Inventory.GetNode<AudioStreamPlayer3D>( "ItemEat" ).Play();

		Logger.Info( "Eating food" );

		Delete();

		// Inventory.Player.Save();

	}

	public string GetIcon()
	{
		return _item.GetIcon();
	}

	public Texture2D GetIconTexture()
	{
		return Loader.LoadResource<Texture2D>( _item.GetIcon() );
	}

}
