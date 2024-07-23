using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Inventory;

public sealed partial class InventoryContainer : RefCounted
{

	[JsonInclude] public Guid Id { get; set; } = Guid.NewGuid();

	[JsonIgnore] public Node3D Owner;

	[JsonIgnore] public PlayerController Player => Owner as PlayerController;

	[JsonInclude] public int MaxItems { get; set; } = 20;

	[JsonInclude] public List<InventorySlot<PersistentItem>> Slots = new();

	[Signal]
	public delegate void InventoryChangedEventHandler();

	public InventoryContainer()
	{
		Logger.Info( "Inventory", "Creating inventory with default slots" );
	}

	public InventoryContainer( int slots )
	{
		Logger.Info( "Inventory", $"Creating inventory with {slots} slots" );
		MaxItems = slots;
	}

	public InventorySlot<PersistentItem> GetSlotByIndex( int index )
	{
		return Slots.FirstOrDefault( slot => slot.Index == index );
	}

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

	public IEnumerable<InventorySlot<PersistentItem>> GetUsedSlots()
	{
		return Slots.ToImmutableList();
	}

	public int FreeSlots => MaxItems - Slots.Count;

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

		slot.InventoryContainer = this;

		// if the slot has no index or the index is already taken, assign a new index
		if ( slot.Index == -1 || GetSlotByIndex( slot.Index ) != null )
		{
			Logger.Warn( "Inventory", "Imported slot has no index or index is already taken, assigning new index" );
			slot.Index = GetFirstFreeEmptyIndex();
		}

		Slots.Add( slot );

		RecalculateIndexes();
	}

	/* private void PlayPickupSound()
	{
		GetNode<AudioStreamPlayer3D>( "ItemPickup" ).Play();
	} */

	public InventorySlot<PersistentItem> AddItem( PersistentItem item )
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
		// EmitSignal( SignalName.InventoryChanged );
		OnChange();

		return slot;
	}

	public void AddItem( PersistentItem item, int index = -1 )
	{

		if ( index == -1 )
		{
			index = GetFirstFreeEmptyIndex();
			if ( index == -1 )
			{
				throw new InventoryFullException( "Inventory is full." );
			}
		}

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
		OnChange();
	}




	/* 
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
		} */

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

	/* public void SortSlots()
	{
		Slots.Sort( SlotSortingFunc );
	} */

	/* public void SortByType()
	{
		// MergeAllSlots();
		// XLog.Info( "Inventory", $"Sorting inventory {Id} by type" );
		Slots.Sort( ( a, b ) => string.Compare( a.ItemType, b.ItemType, StringComparison.Ordinal ) );
		// RecalculateIndexes();
		ResetIndexes();
		// OnInventoryChanged?.Invoke( this );
		// SyncToPlayerList();
	} */

	public void SortByName()
	{
		// MergeAllSlots();
		// XLog.Info( "Inventory", $"Sorting inventory {Id} by name" );
		Slots.Sort( ( a, b ) => string.Compare( a.GetName(), b.GetName(), StringComparison.Ordinal ) );
		// RecalculateIndexes();
		ResetIndexes();
		// OnInventoryChanged?.Invoke( this );
		// SyncToPlayerList();
	}

	public void SortByIndex()
	{
		// MergeAllSlots();
		// XLog.Info( "Inventory", $"Sorting inventory {Id} by index" );
		Slots.Sort( ( a, b ) => a.Index.CompareTo( b.Index ) );
		// RecalculateIndexes();
		ResetIndexes();
		// OnInventoryChanged?.Invoke( this );
		// SyncToPlayerList();
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

	/* public override void _Process( double delta )
	{
		if ( Input.IsActionJustPressed( "UseTool" ) )
		{
			/* if ( CurrentCarriable != null )
			{
				CurrentCarriable.OnUse( Player );
			} *

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
			
			slot.SetItem( testItem );*
		}
		else if ( Input.IsActionJustPressed( "Drop" ) )
		{
			/*var item = Items.FirstOrDefault();
			if ( item != null )
			{
				DropItem( item );
			}*
		}
	} */

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

	public bool MoveSlot( int slotIndexFrom, int slotIndexTo )
	{
		if ( slotIndexFrom < 0 || slotIndexFrom >= MaxItems )
		{
			// Log.Error( $"SlotIndexFrom {slotIndexFrom} is out of range" );
			throw new ArgumentOutOfRangeException( $"Move: SlotIndexFrom {slotIndexFrom} is out of range" );
		}

		if ( slotIndexTo < 0 || slotIndexTo >= MaxItems )
		{
			// Log.Error( $"SlotIndexTo {slotIndexTo} is out of range" );
			// return false;
			throw new ArgumentOutOfRangeException( $"Move: SlotIndexTo {slotIndexTo} is out of range" );
		}

		/* if ( !AllowSlotMoving )
		{
			throw new Exception( "You cannot move items in this inventory" );
		} */

		var slotFrom = GetSlotByIndex( slotIndexFrom );
		var slotTo = GetSlotByIndex( slotIndexTo );

		if ( slotFrom == null )
		{
			// Log.Error( $"SlotFrom {slotIndexFrom} is null" );
			// return false;
			throw new Exception( $"Move: SlotFrom {slotIndexFrom} is null" );
		}

		if ( slotFrom == slotTo )
		{
			// throw new Exception( $"SlotFrom {slotIndexFrom} is the same as SlotTo {slotIndexTo}" );
			return false; // don't throw an exception, just error silently
		}

		if ( slotTo != null )
		{
			if ( slotFrom.CanMergeWith( slotTo ) )
			{
				slotFrom.MergeWith( slotTo );
				return true;
			}

			return SwapSlot( slotIndexFrom, slotIndexTo );
		}

		slotFrom.Index = slotIndexTo;

		// Slots.Sort( ( a, b ) => a.Index.CompareTo( b.Index ) );
		// SortByIndex();
		RecalculateIndexes();

		// FixEventRegistration();

		// SyncToPlayerList();

		// OnInventoryChanged?.Invoke( this );
		OnChange();

		return true;
	}

	public bool SwapSlot( int slotIndexFrom, int slotIndexTo )
	{
		if ( slotIndexFrom < 0 || slotIndexFrom >= MaxItems )
		{
			// Log.Error( $"SlotIndexFrom {slotIndexFrom} is out of range" );
			// return false;
			throw new ArgumentOutOfRangeException( $"Swap: SlotIndexFrom {slotIndexFrom} is out of range" );
		}

		if ( slotIndexTo < 0 || slotIndexTo >= MaxItems )
		{
			// Log.Error( $"SlotIndexTo {slotIndexTo} is out of range" );
			// return false;
			throw new ArgumentOutOfRangeException( $"Swap: SlotIndexTo {slotIndexTo} is out of range" );
		}

		/* if ( !AllowSlotMoving )
		{
			throw new Exception( "You cannot move items in this inventory" );
		} */

		var slotFrom = GetSlotByIndex( slotIndexFrom );
		var slotTo = GetSlotByIndex( slotIndexTo );

		if ( slotFrom == null )
		{
			// Log.Error( $"SlotFrom {slotIndexFrom} is null" );
			// return false;
			throw new Exception( $"Swap: SlotFrom {slotIndexFrom} is null" );
		}

		if ( slotTo == null )
		{
			// Log.Error( $"SlotTo {slotIndexTo} is null" );
			// return false;
			throw new Exception( $"Swap: SlotTo {slotIndexTo} is null" );
		}

		slotFrom.Index = slotIndexTo;
		slotTo.Index = slotIndexFrom;

		// Slots.Sort( ( a, b ) => a.Index.CompareTo( b.Index ) );
		// SortByIndex();
		RecalculateIndexes();

		// SyncToPlayerList();

		// OnInventoryChanged?.Invoke( this );
		OnChange();

		return true;
	}

	public void DeleteAll()
	{
		Slots.Clear();
		// OnInventoryChanged?.Invoke();
		OnChange();
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
