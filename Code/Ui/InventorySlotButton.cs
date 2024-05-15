using Godot;
using vcrossing2.Code.Items;
using vcrossing2.Code.Persistence;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Ui;

public partial class InventorySlotButton : Button
{
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
		var item = Slot.GetItem();
		if ( item != null )
		{
			var itemData = item.GetItemData();
			if ( itemData.Icon != null )
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
			if ( Item is BaseCarriable carriable )
			{
				DurabilityBar.Value =
					((float)carriable.Durability / (float)carriable.GetItemData().MaxDurability) * 100;
				GD.Print( $"Durability: {carriable.Durability}, Max: {carriable.GetItemData().MaxDurability}" );
			}
		}
	}

	private PersistentItem Item => Slot?.GetItem();

	private bool HasDurability =>
		Slot != null && Slot.HasItem && Item is BaseCarriable carriable && carriable.Durability > 0;

	public override void _Pressed()
	{
		if ( Slot == null || !Slot.HasItem ) return;
		GD.Print( $"Pressed item button for {Slot.GetItem().GetItemData().Name}" );
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


		if ( itemData.PlaceScene != null ) contextMenu.AddItem( "Place", 2 );

		if ( itemData.CanEquip && itemData.CarryScene != null )
		{
			contextMenu.AddItem( "Equip", 3 );
		}

		if ( CanBuryItem )
		{
			contextMenu.AddItem( "Bury", 5 );
		}
		else if ( itemData.DropScene != null )
		{
			contextMenu.AddItem( "Drop", 1 );
		}


		contextMenu.AddItem( "Delete", 4 );

		contextMenu.IdPressed += id =>
		{
			switch ( id )
			{
				case 1:
					Slot.Drop();
					break;
				case 2:
					Slot.Place();
					break;
				case 3:
					Slot.Equip();
					break;
				case 4:
					Slot.RemoveItem();
					break;
				case 5:
					Slot.Bury();
					break;
			}
		};
		return contextMenu;
	}

	public bool CanBuryItem
	{
		get
		{
			if ( Slot == null || !Slot.HasItem ) return false;

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
