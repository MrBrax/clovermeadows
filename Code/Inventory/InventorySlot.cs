using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Inventory;

public partial class InventorySlot<TItem> where TItem : PersistentItem
{

	[JsonInclude] public int Index { get; set; } = -1;

	[JsonInclude] public TItem _item;

	[JsonInclude] public int Amount { get; set; } = 1;

	public InventorySlot( InventoryContainer inventory )
	{
		InventoryContainer = inventory;
	}

	public InventorySlot()
	{
	}


	[JsonIgnore] public InventoryContainer InventoryContainer { get; set; }

	[JsonIgnore] public bool HasItem => _item != null;

	public void SetItem( TItem item )
	{
		_item = item;
		InventoryContainer.OnChange();
	}

	public TItem GetItem()
	{
		return _item;
	}

	public T GetItem<T>() where T : TItem
	{
		return (T)_item;
	}

	public string GetName()
	{
		return _item.GetName();
	}

	public void Delete()
	{
		InventoryContainer.RemoveSlot( Index );
		// _item = null;
		// Inventory.OnChange();
		// Inventory.Player.Save();
	}

	public bool CanMergeWith( InventorySlot<TItem> other )
	{
		if ( _item == null || other._item == null )
		{
			// XLog.Error( "InventoryContainerSlot",
			// 	$"CanMerge: Item is null in slot {Index} or other slot {other.Index}" );
			return false;
		}

		if ( _item.GetType() != other._item.GetType() )
		{
			// XLog.Error( "InventoryContainerSlot",
			// 	$"CanMerge: _item types are not the same in slot {Index} or other slot {other.Index}" );
			return false;
		}

		if ( _item.Stackable == false || other._item.Stackable == false )
		{
			// XLog.Error( "InventoryContainerSlot",
			// 	$"CanMerge: _item is not stackable in slot {Index} or other slot {other.Index}" );
			return false;
		}

		if ( Amount <= 0 )
		{
			// XLog.Error( "InventoryContainerSlot",
			// 	$"CanMerge: _item is stackable but amount is 0 in slot {Index} or other slot {other.Index}" );
			return false;
		}

		if ( _item.MaxStack < Amount + other.Amount )
		{
			// XLog.Error( "InventoryContainerSlot",
			// 	$"CanMerge: _item is stackable but amount is over max stack in slot {Index} or other slot {other.Index}" );
			return false;
		}

		/*
		if ( !_item.CanMergeWith( other._item ) )
		{
			// XLog.Error( "InventoryContainerSlot",
			// 	$"CanMerge: _item cannot merge with other _item in slot {Index} or other slot {other.Index}" );
			return false;
		}
		*/

		try
		{
			_item.CanMergeWith( other._item );
		}
		catch ( Exception e )
		{
			// XLog.Error( "InventoryContainerSlot",
			// 	$"CanMerge: Item cannot merge with other item in slot {Index} or other slot {other.Index}" );
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
		if ( !_item.CanMergeWith( other._item ) )
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
		_item.MergeWith( other._item );

		// delete and sync this one
		Delete();
		// FixEventRegistration();
		// InventoryContainer.SyncToPlayerList();

		// XLog.Info( "InventoryContainerSlot", $"Merged {other.Amount} items into slot {Index}" );
	}
}
