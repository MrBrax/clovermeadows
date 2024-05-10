using Godot;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Ui;

public partial class InventorySlotButton : Button
{
	
	public InventorySlot Slot { get; set; }
	
	public InventorySlotButton( InventorySlot slot )
	{
		Slot = slot;
	}

	public override void _Ready()
	{
		base._Ready();

		if ( Slot == null ) return;
		
		var item = Slot.GetItem();
		if ( item != null )
		{
			Text = item.GetItemData().Name;
		} else {
			Text = "Empty";
		}
	}

	public override void _Pressed()
	{
		if ( Slot == null || !Slot.HasItem ) return;
		GD.Print( $"Pressed item button for {Slot.GetItem().GetItemData().Name}" );
		// Slot.Place();
		
		var contextMenu = new PopupMenu();
		contextMenu.AddItem( "Drop", 1 );
		contextMenu.AddItem( "Place", 2 );
		
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
			}
		};
		
		AddChild( contextMenu );

		contextMenu.Position = new Vector2I( (int)(GlobalPosition.X + GetRect().Size.X), (int)GlobalPosition.Y );
		contextMenu.Popup();
	}
	
}
