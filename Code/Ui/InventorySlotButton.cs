using System;
using vcrossing.Code.Carriable;
using vcrossing.Code.Data;
using vcrossing.Code.Inventory;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Ui;

public partial class InventorySlotButton : Button
{

	public enum ContextMenuAction
	{
		Drop = 1,
		Place = 2,
		Equip = 3,
		Delete = 4,
		Bury = 5,
		SetWallpaper = 6,
		Eat = 7
	}

	[Export] public ProgressBar DurabilityBar;

	private InventorySlot<PersistentItem> _slot;

	public InventorySlot<PersistentItem> Slot
	{
		get => _slot;
		set
		{
			_slot = value;
			UpdateSlot();
		}
	}

	public InventorySlotButton()
	{
	}

	public InventorySlotButton( InventorySlot<PersistentItem> slot )
	{
		Slot = slot;
	}

	public override void _Ready()
	{
		base._Ready();

		if ( Slot == null ) return;

		UpdateSlot();
	}

	private void UpdateSlot()
	{
		var item = Slot?.GetItem();
		if ( item != null )
		{
			var itemData = item.GetItemData();
			if ( itemData == null )
			{
				Text = $"Error ({item.GetType().Name})";
				Logger.LogError( "InventorySlotButton", $"Item data is null for {item.GetType().Name}" );
			}
			else if ( itemData.Icon != null )
			{
				Icon = itemData.Icon;
				Text = "";
			}
			else
			{
				Text = item.GetName();
			}
		}
		else
		{
			// Text = "Empty";
			Text = "";
		}

		// DurabilityBar.Visible = HasDurability;

		if ( HasDurability )
		{
			if ( Item is Persistence.BaseCarriable carriable )
			{
				DurabilityBar.Value =
					((float)carriable.Durability / (float)carriable.GetItemData().MaxDurability) * 100;
				// Logger.Info( "InventorySlotButton", $"Durability: {carriable.Durability}, Max: {carriable.GetItemData().MaxDurability}" );
			}
		}
	}

	private PersistentItem Item => Slot?.GetItem();

	private bool HasDurability =>
		Slot != null && Slot.HasItem && Item is Persistence.BaseCarriable carriable && carriable.Durability > 0;

	public override void _Pressed()
	{
		if ( Slot == null || !Slot.HasItem ) return;
		Logger.Info( $"Pressed item button for {Slot.GetItem().GetItemData().Name}" );
		// Slot.Place();

		var itemData = Slot.GetItem().GetItemData();

		var contextMenu = GenerateContextMenu( itemData );

		AddChild( contextMenu );

		contextMenu.Position = new Vector2I( (int)(GlobalPosition.X + GetRect().Size.X), (int)GlobalPosition.Y );
		contextMenu.Popup();
	}

	private PopupMenu GenerateContextMenu( ItemData itemData )
	{
		var contextMenu = new PopupMenu();

		// TODO: add interface or something to check if item is edible
		if ( itemData is FruitData fruitData )
		{
			contextMenu.AddItem( "Eat", (int)ContextMenuAction.Eat );
		}

		if ( itemData is WallpaperData wallpaperData )
		{
			contextMenu.AddItem( "Set Wallpaper", (int)ContextMenuAction.SetWallpaper );
		}

		if ( itemData.CanEquip && itemData.CarryScene != null )
		{
			contextMenu.AddItem( "Equip", (int)ContextMenuAction.Equip );
		}

		if ( itemData.PlaceScene != null ) contextMenu.AddItem( "Place", (int)ContextMenuAction.Place );

		if ( CanBuryItem )
		{
			contextMenu.AddItem( "Bury", (int)ContextMenuAction.Bury );
		}
		else if ( itemData.CanDrop )
		{
			contextMenu.AddItem( "Drop", (int)ContextMenuAction.Drop );
		}

		contextMenu.AddItem( "Delete", (int)ContextMenuAction.Delete );

		contextMenu.IdPressed += id =>
		{
			Logger.Info( $"Pressed context menu item {id}" );
			switch ( id )
			{
				case (int)ContextMenuAction.Drop:
					Slot.Drop();
					break;
				case (int)ContextMenuAction.Place:
					Slot.Place();
					break;
				case (int)ContextMenuAction.Equip:
					Slot.Equip();
					break;
				case (int)ContextMenuAction.Delete:
					Slot.RemoveItem();
					break;
				case (int)ContextMenuAction.Bury:
					Slot.Bury();
					break;
				case (int)ContextMenuAction.SetWallpaper:
					Slot.SetWallpaper();
					break;
				case (int)ContextMenuAction.Eat:
					Slot.Eat();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		};
		return contextMenu;
	}

	public bool CanBuryItem
	{
		get
		{
			if ( Slot == null || !Slot.HasItem ) return false;

			if ( Slot.Inventory.Player.GetEquippedItem<Carriable.BaseCarriable>( Player.PlayerController.EquipSlot.Tool ) is not Shovel )
			{
				return false;
			}

			if ( !Slot.GetItem().GetItemData().Placements.HasFlag( World.ItemPlacement.Underground ) )
			{
				return false;
			}

			var pos = Slot.Inventory.Player.Interact.GetAimingGridPosition();
			var floorItem = Slot.Inventory.World.GetItem( pos, World.ItemPlacement.Floor );
			if ( floorItem != null && floorItem.Node is Hole hole )
			{
				return true;
			}

			return false;

		}
	}
}
