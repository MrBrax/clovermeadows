using System.Collections.Generic;
using System.Linq;
using Godot;
using vcrossing2.Code.Carriable;
using vcrossing2.Code.DTO;
using vcrossing2.Code.Items;
using vcrossing2.Code.Persistence;
using vcrossing2.Inventory;

namespace vcrossing2.Code.Player;

public partial class Inventory : Node3D
{

	// private List<InventoryItem> Items = new();
	[Export] public int MaxItems { get; set; } = 20;
	private List<InventorySlot<PersistentItem>> Slots = new();

	internal World World => GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;
	internal PlayerController Player => GetNode<PlayerController>( "../" );
	internal Node3D PlayerModel => GetNode<Node3D>( "../PlayerModel" );
	internal PlayerInteract PlayerInteract => GetNode<PlayerInteract>( "../PlayerInteract" );

	internal BaseCarriable CurrentCarriable => Player.CurrentCarriable;
	
	public delegate void InventoryChanged();

	public event InventoryChanged OnInventoryChanged;
	
	public InventorySlot<PersistentItem> GetSlot( int index )
	{
		return Slots[index];
	}
	
	public InventorySlot<PersistentItem> GetFirstFreeSlot()
	{
		return Slots.FirstOrDefault( slot => !slot.HasItem );
	}

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
	
	public IEnumerable<InventorySlot<PersistentItem>> GetSlots()
	{
		return Slots;
	}
	
	public void RemoveSlots()
	{
		Slots.Clear();
	}
	
	public void ImportSlot( InventorySlot<PersistentItem> slot )
	{
		slot.Inventory = this;
		Slots.Add( slot );
	}

	public void PickUpItem( WorldItem worldItem )
	{
		if ( string.IsNullOrEmpty( worldItem.ItemDataPath ) ) throw new System.Exception( "Item data path is null" );
		
		var inventoryItem = PersistentItem.Create( worldItem );
		// worldItem.UpdateDTO();
		
		if ( inventoryItem == null )
		{
			throw new System.Exception( "Failed to create inventory item" );
			return;
		}
		

		inventoryItem.ItemDataPath = worldItem.ItemDataPath;
		// inventoryItem.DTO = worldItem.DTO;
		// inventoryItem.Quantity = item.Quantity;
		
		var slot = GetFirstFreeSlot();
		if ( slot == null )
		{
			throw new System.Exception( "No free slots." );
			return;
		}
		
		slot.SetItem( inventoryItem );
		
		World.RemoveItem( worldItem );
		GD.Print( "Picked up item" );

		World.Save();

		GetNode<PlayerController>( "../" ).Save();
	}

	public override void _Ready()
	{
		// CurrentCarriable = GetNode<BaseCarriable>( "CurrentCarriable" );
		// Items.Add( new InventoryItem( GD.Load<ItemData>( "res://items/misc/hole.tres" ), 1 ) );
		
		// add slots
		for ( var i = 0; i < MaxItems; i++ )
		{
			var slot = new InventorySlot<PersistentItem>(this);
			Slots.Add( slot );
		}
	}

	public override void _Process( double delta )
	{
		if ( Input.IsActionJustPressed( "UseTool" ) )
		{
			if ( CurrentCarriable != null )
			{
				CurrentCarriable.OnUse( Player );
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
		OnInventoryChanged?.Invoke();
	}
}
