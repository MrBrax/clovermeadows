using System;
using Godot;
using vcrossing.Code.Carriable;
using vcrossing.Code.Components;
using vcrossing.Code.Inventory;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;

namespace vcrossing.Code.Ui;

public partial class InventoryEquipButton : Button
{

	[Export] public Components.Inventory Inventory;
	[Export] public Equips.EquipSlot EquipSlot;

	protected Node3D Equipment => Inventory.Player.Equips.GetEquippedItem( EquipSlot );

	public override void _Ready()
	{
		base._Ready();
		// UpdateSlot();
	}

	public void UpdateSlot()
	{

		if ( Inventory == null )
		{
			Logger.Warn( "Inventory not set" );
			Text = "ERROR";
			return;
		}

		// var equipment = Inventory.Player.Equips.GetEquippedItem( EquipSlot );

		if ( Equipment == null )
		{
			Icon = null;
			Text = EquipSlot.ToString();
			return;
		}

		if ( Equipment is Carriable.BaseCarriable carriable )
		{
			// Text = carriable.GetName();
			Text = "";
			Icon = carriable.ItemData.GetIcon();
			TooltipText = $"{carriable.ItemData.Name}\n{carriable.ItemData.Description}\n{carriable.Durability}";
			return;
		}

		if ( Equipment is Clothing clothingItem )
		{
			// Text = clothingItem.GetName();
			Text = "";
			Icon = clothingItem.ItemData.GetIcon();
			TooltipText = $"{clothingItem.ItemData.Name}\n{clothingItem.ItemData.Description}";
			return;
		}

		Icon = null;
		Text = "ERROR";

	}

	/* public void SetEquipment( Carriable.BaseCarriable playerCurrentCarriable )
	{
		Equipment = playerCurrentCarriable;
		UpdateSlot();
	} */

	public override void _Pressed()
	{
		base._Pressed();
		Unequip();
	}

	public void Unequip( int slot = -1 )
	{

		if ( !Inventory.Player.Equips.HasEquippedItem( EquipSlot ) )
		{
			Logger.Warn( $"No item equipped in slot {EquipSlot}" ); // TODO: Show message to player
			return;
		}

		var index = Inventory.Container.GetFirstFreeEmptyIndex();
		if ( index == -1 )
		{
			Logger.Warn( "No free slots available" ); // TODO: Show message to player
			return;
		}

		var item = PersistentItem.Create( Inventory.Player.Equips.GetEquippedItem( EquipSlot ) );

		// var slot = new InventorySlot<PersistentItem>( Inventory );
		// slot.SetItem( item );
		// slot.Index = index;

		Inventory.Container.AddItemToIndex( item, slot );

		// Inventory.GetFirstFreeSlot()?.SetItem( item );
		// Inventory.Player.CurrentCarriable.QueueFree();
		// Inventory.Player.CurrentCarriable = null;

		// Inventory.Player.GetEquippedItem( EquipSlot ).QueueFree();

		Inventory.Player.Equips.RemoveEquippedItem( EquipSlot, true );

		// Equipment = null;
		UpdateSlot();
		// Inventory.Player.Save();
	}

	public void SetInventory( Components.Inventory inventory )
	{
		Inventory = inventory;
		UpdateSlot();
	}

	public override Variant _GetDragData( Vector2 atPosition )
	{

		if ( Equipment == null )
		{
			Logger.Warn( $"{Name} No item to drag" );
			return default;
		}

		// Logger.Info( $"{Name} Get drag data {Equipment.ItemData.Name}" );

		var image = new TextureRect
		{
			// Texture = Slot.GetItem().GetItemData().Icon,
			Texture = Icon,
			// Size = new Vector2( 40, 40 ),
			CustomMinimumSize = new Vector2( 60, 60 ),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			ClipContents = true,
			Modulate = new Color( 1, 1, 1, 0.5f )
		};

		SetDragPreview( image );

		return new Godot.Collections.Dictionary<string, Variant>
		{
			{ "type", "equip" },
			{ "slot", (int)EquipSlot },
			{ "item", Equipment },
			{ "button", this }
		};

		// return Slot != null ? Slot.Index : -1;
	}

	public override void _DropData( Vector2 atPosition, Variant data )
	{

		var dict = data.AsGodotDictionary();
		if ( dict == null ) return;

		var dropType = dict["type"].AsString();

		if ( dropType == "item" )
		{
			var fromInventory = dict["inventory"].As<InventoryContainer>();
			var slotIndex = dict["slot"].AsInt32();
			var itemName = dict["item"].AsString();
			var button = dict["button"].As<InventorySlotButton>();

			// Logger.Info( $"fromInventory: {fromInventory.Id}, slotIndex: {slotIndex}, itemName: {itemName}" );

			// Logger.Info( $"Dropped item {itemName} from {fromInventory?.Id} to {Slot?.InventoryContainer?.Id} (slot {slotIndex} => {Index})" );

			// TODO: move between inventories

			button.Slot.Equip();

			UiSounds.PlaySound( "res://sound/inventory/item_drop.ogg" );

		}
	}

	public override bool _CanDropData( Vector2 atPosition, Variant data )
	{
		var dict = data.AsGodotDictionary();
		if ( dict == null ) return false;
		// Logger.Info( $"{Name} Can drop data {dict["type"].AsString()}" );
		var dropType = dict["type"].AsString();
		if ( dropType != "item" ) return false;
		return true;
	}
}
