using Godot;
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
			var carriable = Item as BaseCarriable;
			if ( carriable != null )
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

		var contextMenu = new PopupMenu();

		if ( itemData.DropScene != null ) contextMenu.AddItem( "Drop", 1 );

		if ( itemData.PlaceScene != null ) contextMenu.AddItem( "Place", 2 );

		if ( itemData.CanEquip && itemData.CarryScene != null )
		{
			contextMenu.AddItem( "Equip", 3 );
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
			}
		};

		AddChild( contextMenu );

		contextMenu.Position = new Vector2I( (int)(GlobalPosition.X + GetRect().Size.X), (int)GlobalPosition.Y );
		contextMenu.Popup();
	}
}
