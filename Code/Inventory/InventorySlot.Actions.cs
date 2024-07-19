using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Carriable;
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
			InventoryContainer.Player.World.SpawnPersistentNode( _persistentItem, position, playerRotation, World.ItemPlacement.Floor, true );
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
		Logger.Info( "InventorySlot", "Placing item" );

		var aimingGridPosition = InventoryContainer.Player.Interact.GetAimingGridPosition();

		var playerRotation =
			InventoryContainer.Player.World.GetItemRotationFromDirection(
				InventoryContainer.Player.World.Get4Direction( InventoryContainer.Player.Model.RotationDegrees.Y ) );

		// handle floor decals separately
		// TODO: just split this into a separate method
		/* if ( _item.ItemData.Placements.HasFlag( World.ItemPlacement.FloorDecal ) )
		{
			var floorDecalItem = InventoryContainer.Player.World.GetItem( aimingGridPosition, World.ItemPlacement.FloorDecal );

			if ( floorDecalItem != null && floorDecalItem.Node is Items.FloorDecal floorDecalNode )
			{
				floorDecalNode.TexturePath = (_item as Persistence.FloorDecal).TexturePath;
				floorDecalNode.UpdateDecal();
			}
			else
			{
				InventoryContainer.Player.World.SpawnPersistentNode( _item, aimingGridPosition, playerRotation, World.ItemPlacement.FloorDecal, false );
			}

			Delete();
			return;
		} */

		var floorItem = InventoryContainer.Player.World.GetItem( aimingGridPosition, World.ItemPlacement.Floor );

		if ( floorItem != null )
		{
			var placeableNodes = floorItem.GetPlaceableNodes();
			if ( placeableNodes.Count > 0 )
			{

				var onTopItem = InventoryContainer.Player.World.GetItem( aimingGridPosition, World.ItemPlacement.OnTop );
				if ( onTopItem != null )
				{
					Logger.Warn( "On top item already exists." );
					return;
				}

				try
				{
					InventoryContainer.Player.World.SpawnPersistentNode( _persistentItem, aimingGridPosition, playerRotation, World.ItemPlacement.OnTop,
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
			InventoryContainer.Player.World.SpawnPersistentNode( _persistentItem, aimingGridPosition, playerRotation, World.ItemPlacement.Floor,
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
		if ( _persistentItem is Persistence.BaseCarriable )
		{
			slot = Components.Equips.EquipSlot.Tool;
		}
		else if ( _persistentItem is ClothingItem clothingItem )
		{
			slot = clothingItem.EquipSlot;
			if ( slot == 0 ) throw new Exception( $"Invalid equip slot for {clothingItem} ({clothingItem.GetName()}) {clothingItem.ItemData.GetType()}" );
		}
		else
		{
			throw new Exception( $"Item {_persistentItem} is not equipable." );
		}


		PersistentItem currentEquip = null;
		if ( InventoryContainer.Player.Equips.HasEquippedItem( slot ) )
		{
			currentEquip = PersistentItem.Create( InventoryContainer.Player.Equips.GetEquippedItem( slot ) );
		}

		if ( _persistentItem is Persistence.BaseCarriable carriable )
		{
			var carriableNode = carriable.Create();
			InventoryContainer.Player.Equips.SetEquippedItem( Components.Equips.EquipSlot.Tool, carriableNode );
		}
		else if ( _persistentItem is ClothingItem clothingItem )
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
		InventoryContainer.Player.World.SpawnPersistentNode( _persistentItem, pos, World.ItemRotation.North, World.ItemPlacement.Underground,
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

		if ( _persistentItem.ItemData is SeedData seedData )
		{

			var plantItemData = seedData.SpawnedItemData;

			if ( plantItemData == null )
			{
				throw new System.Exception( "Seed data does not have a spawned item data." );
			}

			// remove hole so it isn't obstructing the plant that will be spawned next
			InventoryContainer.Player.World.RemoveItem( hole );

			if ( plantItemData is PlantData plantData )
			{
				InventoryContainer.Player.World.SpawnNode( plantItemData, plantData.PlantedScene, pos, World.ItemRotation.North, World.ItemPlacement.Floor );
			}
			else
			{
				throw new System.Exception( "Spawned item data is not a plant data. Unsupported for now." );
			}

			Delete();

			return;

		}
		else if ( _persistentItem.ItemData is PlantData plantData )
		{

			var plantScene = plantData.PlantedScene;

			if ( plantScene == null )
			{
				throw new System.Exception( "Plant data does not have a planted scene." );
			}

			// remove hole so it isn't obstructing the plant that will be spawned next
			InventoryContainer.Player.World.RemoveItem( hole );

			InventoryContainer.Player.World.SpawnNode( plantData, plantScene, pos, World.ItemRotation.North, World.ItemPlacement.Floor );

			Delete();

			return;

		}

		throw new System.Exception( "Item data is not a seed or plant data." );

		// Inventory.World.RemoveItem( floorItem );
	}

	public void SetWallpaper()
	{

		if ( _persistentItem.ItemData is not WallpaperData wallpaperData )
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

		if ( _persistentItem.ItemData is not FlooringData floorData )
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
		if ( _persistentItem.ItemData is not FruitData foodData )
		{
			throw new System.Exception( "Item data is not a food data." );
		}

		InventoryContainer.Player.Inventory.GetNode<AudioStreamPlayer3D>( "ItemEat" ).Play();

		Logger.Info( "Eating food" );

		Delete();

		// Inventory.Player.Save();

	}

	/* public void EquipPaint()
	{

		if ( _item is not Persistence.FloorDecal floorDecal )
		{
			throw new System.Exception( "Item data is not a paint data." );
		}

		if ( !InventoryContainer.Player.Equips.IsEquippedItemType<Paintbrush>( Components.Equips.EquipSlot.Tool ) )
		{
			throw new System.Exception( "No paintbrush equipped." );
		}

		var paintbrush = InventoryContainer.Player.Equips.GetEquippedItem<Paintbrush>( Components.Equips.EquipSlot.Tool );

		paintbrush.CurrentTexturePath = floorDecal.TexturePath;

		Logger.Info( "Equipping paint" );

		// Inventory.Player.Save();

	} */

	/* public string GetIcon()
	{
		return _item.GetIcon();
	} */

	public Texture2D GetIconTexture()
	{
		return _persistentItem.GetIconTexture();
	}

}
