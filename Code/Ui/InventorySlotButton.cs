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
		Eat = 7,
		SetFlooring = 8,
		Plant = 9
	}

	[Export] public ProgressBar DurabilityBar;

	private InventorySlot<PersistentItem> _slot;

	public Components.Inventory PlayerInventory;

	public int Index;

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

		GuiInput += OnGuiInput;

		ButtonDown += () => UiSounds.PlaySound( "res://sound/inventory/inventory_pick_up.ogg" );
		// ButtonUp += () => UiSounds.PlaySound( "res://sound/inventory/item_drop2.ogg" );

		if ( Slot == null ) return;

		UpdateSlot();
	}

	private void UpdateSlot()
	{
		Text = "";
		var item = Slot?.GetItem();
		if ( item != null )
		{
			var itemData = item.ItemData;
			if ( itemData == null )
			{
				Text = $"Error ({item.GetType().Name})";
				Logger.LogError( "InventorySlotButton", $"Item data is null for {item.GetType().Name}" );
				return;
			}
			else if ( Slot.GetIconTexture() != null )
			{
				Icon = Slot.GetIconTexture();
				// Text = "";
			}
			else
			{
				// Text = item.GetName();
				// Icon = Loader.LoadResource<CompressedTexture2D>( "res://icons/default_item.png" );
			}

			// TooltipText = itemData.Name + (itemData.Description != null ? $"\n{itemData.Description}" : "");
			TooltipText = item.GetTooltip();
		}
		else
		{
			// Text = "Empty";

		}

		// DurabilityBar.Visible = HasDurability;

		if ( HasDurability )
		{
			if ( Item is Persistence.BaseCarriable carriable && carriable.ItemData is ToolData toolData )
			{
				DurabilityBar.Value = ((float)carriable.Durability / (float)toolData.MaxDurability) * 100;
				// Logger.Info( "InventorySlotButton", $"Durability: {carriable.Durability}, Max: {toolData.MaxDurability}" );
			}
		}
	}

	private PersistentItem Item => Slot?.GetItem();

	private bool HasDurability =>
		Slot != null && Slot.HasItem && Item is Persistence.BaseCarriable carriable && carriable.Durability > 0;

	private bool _isDragging;

	private void OnGuiInput( InputEvent @event )
	{
		// open context menu on right click
		if ( @event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Right )
		{
			if ( Slot == null || !Slot.HasItem ) return;
			Logger.Info( $"Pressed item button for {Slot.GetItem().ItemData.Name}" );
			// Slot.Place();

			var itemData = Slot.GetItem().ItemData;

			var contextMenu = GenerateContextMenu( itemData );

			AddChild( contextMenu );

			contextMenu.Position = new Vector2I( (int)(GlobalPosition.X + GetRect().Size.X), (int)GlobalPosition.Y );
			contextMenu.Popup();
		}
	}

	public override Variant _GetDragData( Vector2 atPosition )
	{

		if ( Slot == null || !Slot.HasItem ) return default;

		Logger.Info( $"{Name} Get drag data {Slot.GetItem().ItemData.Name}" );

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

		// return Slot != null ? Slot.Index : -1;

		return new Godot.Collections.Dictionary<string, Variant>
		{
			{ "type", "item" },
			{ "inventory", Slot.InventoryContainer },
			{ "slot", Slot.Index },
			{ "item", Slot.GetItem().ItemData.Name },
			{ "button", this }
		};
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

			// Logger.Info( $"fromInventory: {fromInventory.Id}, slotIndex: {slotIndex}, itemName: {itemName}" );

			Logger.Info( $"Dropped item {itemName} from {fromInventory?.Id} to {Slot?.InventoryContainer?.Id} (slot {slotIndex} => {Index})" );

			// TODO: move between inventories

			fromInventory.MoveSlot( slotIndex, Index );

			UiSounds.PlaySound( "res://sound/inventory/item_drop.ogg" );

		}
		else if ( dropType == "equip" )
		{

			var equipSlot = dict["slot"].AsInt32();
			// var itemName = dict["item"].As
			var button = dict["button"].As<InventoryEquipButton>();

			button.Unequip( Index );

			UiSounds.PlaySound( "res://sound/inventory/item_drop.ogg" );

		}
		else
		{
			Logger.Warn( $"Unknown drop type {dropType}" );
		}
	}

	public override bool _CanDropData( Vector2 atPosition, Variant data )
	{
		var dict = data.AsGodotDictionary();
		if ( dict == null ) return false;
		// Logger.Info( $"{Name} Can drop data {dict["type"].AsString()}" );
		var dropType = dict["type"].AsString();
		if ( dropType != "item" && dropType != "equip" ) return false;
		return true;
	}

	/* public override void _Pressed()
	{
		if ( Slot == null || !Slot.HasItem ) return;
		Logger.Info( $"Pressed item button for {Slot.GetItem().GetItemData().Name}" );
		// Slot.Place();

		var itemData = Slot.GetItem().GetItemData();

		var contextMenu = GenerateContextMenu( itemData );

		AddChild( contextMenu );

		contextMenu.Position = new Vector2I( (int)(GlobalPosition.X + GetRect().Size.X), (int)GlobalPosition.Y );
		contextMenu.Popup();
	} */

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

		if ( itemData is FlooringData flooringData )
		{
			contextMenu.AddItem( "Set Flooring", (int)ContextMenuAction.SetFlooring );
		}

		/* if ( (itemData is ToolData || itemData is ClothingData) && itemData.CarryScene != null )
		{
			contextMenu.AddItem( "Equip", (int)ContextMenuAction.Equip );
		} */

		if ( itemData is IEquipableData )
		{
			contextMenu.AddItem( "Equip", (int)ContextMenuAction.Equip );
		}

		if ( CanPlantItem )
		{
			contextMenu.AddItem( "Plant", (int)ContextMenuAction.Plant );
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
					Slot.Delete();
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
				case (int)ContextMenuAction.SetFlooring:
					Slot.SetFlooring();
					break;
				case (int)ContextMenuAction.Plant:
					Slot.Plant();
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

			if ( Slot.InventoryContainer.Player == null ) throw new Exception( "Player/Owner is null" );
			if ( Slot.InventoryContainer.Player.Equips == null ) throw new Exception( "Player equips is null" );

			if ( !Slot.InventoryContainer.Player.Equips.IsEquippedItemType<Shovel>( Components.Equips.EquipSlot.Tool ) )
			{
				return false;
			}

			if ( !Slot.GetItem().ItemData.Placements.HasFlag( World.ItemPlacement.Underground ) )
			{
				return false;
			}

			var pos = Slot.InventoryContainer.Player.Interact.GetAimingGridPosition();
			var floorItem = Slot.InventoryContainer.Player.World.GetItem( pos, World.ItemPlacement.Floor );
			if ( floorItem != null && floorItem.Node is Hole hole )
			{
				return true;
			}

			return false;

		}
	}

	public bool CanPlantItem
	{
		get
		{
			if ( Slot == null || !Slot.HasItem )
			{
				return false;
			}

			if ( Slot.InventoryContainer.Player == null ) throw new Exception( "Player/Owner is null" );
			if ( Slot.InventoryContainer.Player.Equips == null ) throw new Exception( "Player equips is null" );

			if ( Slot.GetItem().ItemData is not SeedData and not PlantData )
			{
				return false;
			}

			if ( !Slot.InventoryContainer.Player.Equips.IsEquippedItemType<Shovel>( Components.Equips.EquipSlot.Tool ) )
			{
				return false;
			}

			var pos = Slot.InventoryContainer.Player.Interact.GetAimingGridPosition();
			var floorItem = Slot.InventoryContainer.Player.World.GetItem( pos, World.ItemPlacement.Floor );
			if ( floorItem != null && floorItem.Node is Hole )
			{
				return true;
			}

			return false;

		}
	}
}
