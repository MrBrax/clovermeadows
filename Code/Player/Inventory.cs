using System;
using System.Collections.Immutable;
using vcrossing.Code.Helpers;
using vcrossing.Code.Inventory;
using vcrossing.Code.Persistence;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Player;

public partial class Inventory : Node3D
{

	// private List<InventoryItem> Items = new();
	[Export] public int MaxItems { get; set; } = 20;
	private List<InventorySlot<PersistentItem>> Slots = new();

	internal World World => GetNode<WorldManager>( "/root/Main/WorldManager" ).ActiveWorld;
	internal PlayerController Player => GetNode<PlayerController>( "../" );
	internal Node3D PlayerModel => GetNode<Node3D>( "../PlayerModel" );
	internal PlayerInteract PlayerInteract => GetNode<PlayerInteract>( "../PlayerInteract" );

	// internal Carriable.BaseCarriable CurrentCarriable => Player.CurrentCarriable;

	[Signal]
	public delegate void InventoryChangedEventHandler();

	// public event InventoryChanged OnInventoryChanged;

	public InventorySlot<PersistentItem> GetSlotByIndex( int index )
	{
		return Slots.FirstOrDefault( slot => slot.Index == index );
	}

	/* public InventorySlot<PersistentItem> GetFirstFreeSlot()
	{
		return Slots.FirstOrDefault( slot => !slot.HasItem );
	} */

	public struct InventoryContainerEntry
	{
		public int Index;
		public InventorySlot<PersistentItem> Slot;
		public bool HasSlot => Slot != null;
	}

	public IEnumerable<InventoryContainerEntry> GetEnumerator()
	{
		for ( var i = 0; i < MaxItems; i++ )
		{
			yield return new InventoryContainerEntry { Index = i, Slot = GetSlotByIndex( i ) };
		}
	}

	/// <summary>
	/// Returns the index of the first empty slot in the inventory.
	/// </summary>
	/// <returns>The index of the first empty slot, or -1 if no empty slot is found.</returns>
	public int GetFirstFreeEmptyIndex()
	{
		foreach ( var slot in GetEnumerator() )
		{
			if ( slot.Slot == null )
			{
				return slot.Index;
			}
		}

		return -1;
	}


	/* private bool AddItem( PersistentItem inventoryItem )
	{
		var index = GetFirstFreeEmptyIndex();
		if ( index == -1 )
		{
			throw new InventoryFullException( "Inventory is full." );
		}

		var slot = new InventorySlot<PersistentItem>( this );
		slot.Index = index;
		slot.SetItem( inventoryItem );

		Slots.Add( slot );

		// slot.SetItem( inventoryItem );

		// OnInventoryChanged?.Invoke();
		EmitSignal( SignalName.InventoryChanged );

		return true;

	} */

	/*public void AddItem( InventoryItem item )
	{
		// Items.Add( item );
		var slot = GetFirstFreeSlot();
		if ( slot == null )
		{
			throw new System.Exception( "No free slots." );
		}
		
		slot.SetItem( item );
		
		OnInventoryChanged?.Invoke();
	}
	
	public void RemoveItem( InventoryItem item )
	{
		// Items.Remove( item );
		
		OnInventoryChanged?.Invoke();
	}*/

	/*public IEnumerable<InventoryItem> GetItems()
	{
		return Items;
	}*/

	public IEnumerable<InventorySlot<PersistentItem>> GetUsedSlots()
	{
		return Slots.ToImmutableList();
	}

	public void RemoveSlots()
	{
		Logger.Debug( "Inventory", "Removing slots" );
		Slots.Clear();
	}

	public void ImportSlot( InventorySlot<PersistentItem> slot )
	{
		if ( Slots.Count >= MaxItems )
		{
			// throw new System.Exception( "Inventory is full." );
			throw new InventoryFullException( "Inventory is full." );
		}

		slot.Inventory = this;

		// if the slot has no index or the index is already taken, assign a new index
		if ( slot.Index == -1 || GetSlotByIndex( slot.Index ) != null )
		{
			slot.Index = GetFirstFreeEmptyIndex();
		}

		Slots.Add( slot );

		RecalculateIndexes();
	}

	private void PlayPickupSound()
	{
		GetNode<AudioStreamPlayer3D>( "ItemPickup" ).Play();
	}

	public void AddItem( PersistentItem item )
	{
		var index = GetFirstFreeEmptyIndex();
		if ( index == -1 )
		{
			throw new InventoryFullException( "Inventory is full." );
		}

		var slot = new InventorySlot<PersistentItem>( this );
		slot.Index = index;
		slot.SetItem( item );

		Slots.Add( slot );

		RecalculateIndexes();

		// OnInventoryChanged?.Invoke();
		EmitSignal( SignalName.InventoryChanged );
	}

	public void AddItem( PersistentItem item, int index )
	{
		if ( GetSlotByIndex( index ) != null )
		{
			throw new SlotTakenException( $"Slot {index} is already taken." );
		}

		var slot = new InventorySlot<PersistentItem>( this );
		slot.Index = index;
		slot.SetItem( item );

		Slots.Add( slot );

		RecalculateIndexes();

		// OnInventoryChanged?.Invoke();
		EmitSignal( SignalName.InventoryChanged );
	}

	/// <summary>
	///  the exact same as AddItem, but with a sound effect
	/// </summary>
	public void PickUpItem( PersistentItem item )
	{
		AddItem( item );
		PlayPickupSound();
	}

	public void PickUpItem( WorldNodeLink nodeLink )
	{
		if ( string.IsNullOrEmpty( nodeLink.ItemDataPath ) ) throw new System.Exception( "Item data path is null" );

		Logger.Info( $"Picking up item {nodeLink.ItemDataPath}" );

		var inventoryItem = PersistentItem.Create( nodeLink );
		// worldItem.UpdateDTO();

		if ( inventoryItem == null )
		{
			throw new System.Exception( "Failed to create inventory item" );
		}

		inventoryItem.ItemDataPath = nodeLink.ItemDataPath;
		// inventoryItem.PlacementType = nodeLink.PlacementType;
		// inventoryItem.DTO = worldItem.DTO;
		// inventoryItem.Quantity = item.Quantity;

		var index = GetFirstFreeEmptyIndex();
		if ( index == -1 )
		{
			// throw new System.Exception( "No free slots." );
			throw new InventoryFullException( "Inventory is full." );
		}

		var slot = new InventorySlot<PersistentItem>( this );
		slot.Index = index;

		slot.SetItem( inventoryItem );

		Logger.Info( $"Picked up item {nodeLink.ItemDataPath}" );

		NodeExtensions.SetCollisionState( nodeLink.Node, false );

		Player.InCutscene = true;
		Player.CutsceneTarget = Vector3.Zero;
		Player.Velocity = Vector3.Zero;

		// TODO: needs dupe protection
		var tween = GetTree().CreateTween();
		var t = tween.Parallel().TweenProperty( nodeLink.Node, "global_position", Player.GlobalPosition + Vector3.Up * 0.5f, 0.2f );
		tween.Parallel().TweenProperty( nodeLink.Node, "scale", Vector3.One * 0.1f, 0.3f ).SetTrans( Tween.TransitionType.Cubic ).SetEase( Tween.EaseType.Out );
		tween.TweenCallback( Callable.From( () =>
		{
			World.RemoveItem( nodeLink );
			Player.InCutscene = false;

			PlayPickupSound();
			// World.Save();
		} ) );

		// World.RemoveItem( nodeLink );

		// World.Save();

		// GetNode<PlayerController>( "../" ).Save();
	}

	public override void _Ready()
	{
		// CurrentCarriable = GetNode<BaseCarriable>( "CurrentCarriable" );
		// Items.Add( new InventoryItem( GD.Load<ItemData>( "res://items/misc/hole.tres" ), 1 ) );

		// add slots
		for ( var i = 0; i < MaxItems; i++ )
		{
			var slot = new InventorySlot<PersistentItem>( this );
			Slots.Add( slot );
		}
	}

	/// <summary>
	///  Recalculate the indexes of all slots in the inventory, keeping old indexes
	/// </summary>
	public void RecalculateIndexes()
	{
		var index = 0;
		foreach ( var slot in GetUsedSlots() )
		{
			// slot.Index = index++;

			if ( slot.Index == -1 )
			{
				Logger.Debug( "Inventory", "Slot has no index, assigning new index" );
				slot.Index = index++;
			}
			else
			{
				Logger.Debug( "Inventory", $"Slot has index {slot.Index}, keeping index" );
				index = slot.Index + 1;
			}
		}

		// OnInventoryChanged?.Invoke( this );
	}

	/// <summary>
	///    Reset the index for all slots in the inventory based on their location in the list
	/// </summary>
	public void ResetIndexes()
	{
		var index = 0;
		foreach ( var slot in GetUsedSlots() )
		{
			slot.Index = index++;
		}

		// OnInventoryChanged?.Invoke( this );
	}

	public void SortSlots()
	{
		Slots.Sort( SlotSortingFunc );
	}

	private static int SlotSortingFunc( InventorySlot<PersistentItem> a, InventorySlot<PersistentItem> b )
	{
		var itemA = a.GetItem();
		var itemB = b.GetItem();

		if ( itemA == null && itemB == null )
		{
			return 0;
		}

		if ( itemA == null )
		{
			return 1;
		}

		if ( itemB == null )
		{
			return -1;
		}

		return string.Compare( itemA.GetName(), itemB.GetName(), System.StringComparison.Ordinal );
	}

	public override void _Process( double delta )
	{
		if ( Input.IsActionJustPressed( "UseTool" ) )
		{
			/* if ( CurrentCarriable != null )
			{
				CurrentCarriable.OnUse( Player );
			} */

			if ( Player.HasEquippedItem( PlayerController.EquipSlot.Tool ) )
			{
				Player.GetEquippedItem<Carriable.BaseCarriable>( PlayerController.EquipSlot.Tool ).OnUse( Player );
			}

			/*var testItem = new InventoryItem( this );
			testItem.ItemDataPath = "res://items/furniture/polka_chair/polka_chair.tres";
			testItem.DTO = new BaseDTO
			{
				ItemDataPath = "res://items/furniture/polka_chair/polka_chair.tres",
			};
			
			var slot = GetFirstFreeSlot();
			if ( slot == null )
			{
				throw new System.Exception( "No free slots." );
				return;
			}
			
			slot.SetItem( testItem );*/
		}
		else if ( Input.IsActionJustPressed( "Drop" ) )
		{
			/*var item = Items.FirstOrDefault();
			if ( item != null )
			{
				DropItem( item );
			}*/
		}
	}

	public void OnChange()
	{
		// OnInventoryChanged?.Invoke();
		EmitSignal( SignalName.InventoryChanged );
	}

	public void RemoveSlot( InventorySlot<PersistentItem> inventorySlot )
	{
		Slots.Remove( inventorySlot );
		RecalculateIndexes();
		OnChange();
	}

	public void RemoveSlot( int index )
	{
		var slot = GetSlotByIndex( index );
		if ( slot == null )
		{
			throw new Exception( $"Slot {index} not found." );
		}

		RemoveSlot( slot );
	}

}

public class InventoryFullException : System.Exception
{
	public InventoryFullException( string message ) : base( message )
	{
	}
}

public class SlotTakenException : System.Exception
{
	public SlotTakenException( string message ) : base( message )
	{
	}
}
