using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Inventory;

public sealed partial class InventorySlot<TItem> where TItem : PersistentItem
{


	[JsonInclude] public int Index { get; set; } = -1;

	[JsonInclude, JsonPropertyName( "_item" )] public TItem _persistentItem;

	[JsonInclude] public int Amount { get; private set; } = 1;

	public InventorySlot( InventoryContainer inventory )
	{
		InventoryContainer = inventory;
	}

	public InventorySlot()
	{
	}


	[JsonIgnore] public InventoryContainer InventoryContainer { get; set; }

	[JsonIgnore] public bool HasItem => _persistentItem != null;

	public void SetItem( TItem item )
	{
		_persistentItem = item;
		InventoryContainer.OnChange();
	}

	public TItem GetItem()
	{
		return _persistentItem;
	}

	public T GetItem<T>() where T : TItem
	{
		return (T)_persistentItem;
	}

	public string GetName()
	{
		return _persistentItem.GetName();
	}

	public void Delete()
	{
		InventoryContainer.RemoveSlot( Index );
		// _item = null;
		// Inventory.OnChange();
		// Inventory.Player.Save();
	}

	public void SetAmount( int amount )
	{
		Amount = amount;
		InventoryContainer.OnChange();
	}

	public bool CanMergeWith( InventorySlot<TItem> other )
	{

		// abort if either item is null
		if ( _persistentItem == null || other._persistentItem == null )
		{
			Logger.Info( "InventoryContainerSlot", "CanMerge: Item is null" );
			return false;
		}

		// abort if item types are not the same
		if ( _persistentItem.GetType() != other._persistentItem.GetType() )
		{
			Logger.Info( "InventoryContainerSlot", "CanMerge: Item types are not the same" );
			return false;
		}

		// abort if either item is not stackable
		if ( _persistentItem.Stackable == false || other._persistentItem.Stackable == false )
		{
			Logger.Info( "InventoryContainerSlot", "CanMerge: Item is not stackable" );
			return false;
		}

		// abort if amount is zero. this should never happen
		if ( Amount <= 0 )
		{
			Logger.Info( "InventoryContainerSlot", "CanMerge: Amount is zero" );
			return false;
		}

		// abort if stack can't hold the amount
		if ( _persistentItem.MaxStack < Amount + other.Amount )
		{
			Logger.Info( "InventoryContainerSlot", $"CanMerge: Stack cannot hold the amount ({_persistentItem.MaxStack} < {Amount + other.Amount})" );
			return false;
		}

		try
		{
			_persistentItem.CanMergeWith( other._persistentItem );
		}
		catch ( Exception e )
		{
			// XLog.Error( "InventoryContainerSlot",
			// 	$"CanMerge: Item cannot merge with other item in slot {Index} or other slot {other.Index}" );
			Logger.Warn( "InventoryContainerSlot", $"CanMerge: {e.Message}" );
			return false;
		}

		return true;
	}

	/// <summary>
	/// Merge this slot into another slot. It will delete this slot and sync the other slot.
	/// </summary>
	/// <param name="other"></param>
	/// <exception cref="Exception"></exception>
	public void MergeWith( InventorySlot<TItem> other )
	{
		// sanity check!
		if ( other == null )
		{
			throw new Exception( "Cannot merge with null slot" );
		}

		// sanity check!!
		if ( other.InventoryContainer == null )
		{
			throw new Exception( "Cannot merge with slot with null inventory" );
		}

		// this is the same as the sanity check called in CanMergeWith
		if ( !CanMergeWith( other ) )
		{
			throw new Exception( "Cannot merge with slot with incompatible item" );
		}

		// sanity check!!!
		if ( !_persistentItem.CanMergeWith( other._persistentItem ) )
		{
			throw new Exception( "Cannot merge with slot with incompatible item" );
		}

		/* if ( !InventoryContainer.AllowSlotInteract )
		{
			throw new Exception( "You cannot interact with this inventory" );
		}

		if ( !other.InventoryContainer.AllowSlotInteract )
		{
			throw new Exception( "You cannot interact with the receiving inventory" );
		} */

		// merge into other
		other.Amount += Amount;
		// other.InventoryContainer.FixEventRegistration();
		// other.InventoryContainer.SyncToPlayerList();

		// call merge on the item, by default it does nothing
		_persistentItem.MergeWith( other._persistentItem );

		// delete and sync this one
		Delete();
		// FixEventRegistration();
		// InventoryContainer.SyncToPlayerList();

		// XLog.Info( "InventoryContainerSlot", $"Merged {other.Amount} items into slot {Index}" );
	}

	public void Open()
	{
		if ( _persistentItem is not Persistence.Gift gift ) throw new Exception( "Item is not a gift" );

		if ( gift.Items.Count == 0 ) throw new Exception( "Gift is empty" );

		if ( InventoryContainer.FreeSlots < gift.Items.Count ) throw new Exception( "Not enough free slots" );

		foreach ( var item in gift.Items )
		{
			InventoryContainer.AddItem( item );
		}

		Delete();

	}

}
