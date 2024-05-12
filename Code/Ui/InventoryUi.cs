using Godot;
using vcrossing2.Code.Player;
using vcrossing2.Inventory;

namespace vcrossing2.Code.Ui;

public partial class InventoryUi : Control
{
	
	[Export] public PlayerController Player;
	[Export] public GridContainer InventoryGrid;
	[Export] public PackedScene InventorySlotButtonScene;
	
	private Player.Inventory Inventory => Player.Inventory;
	
	public override void _Ready()
	{
		if ( Player == null )
		{
			throw new System.Exception( "Player not set." );
		} else if ( InventoryGrid == null )
		{
			throw new System.Exception( "InventoryGrid not set." );
		}
		Player.Inventory.OnInventoryChanged += UpdateInventory;
		UpdateInventory();
		Visible = false;
	}
	
	public void UpdateInventory()
	{
		foreach ( Node child in InventoryGrid.GetChildren() )
		{
			child.QueueFree();
		}
		
		foreach ( var slot in Player.Inventory.GetSlots() )
		{
			// var itemButton = new InventorySlotButton( slot );
			var itemButton = InventorySlotButtonScene.Instantiate<InventorySlotButton>();
			itemButton.Slot = slot;
			/*if ( slot.HasItem )
			{
				itemButton.Text = slot.GetItem().GetItemData().Name;
			}*/

			// itemButton.RectMinSize = new Vector2( 100, 100 );
			InventoryGrid.AddChild( itemButton );
			
			// itemButton.Connect( "pressed", this, nameof( OnItemButtonPressed ), new Godot.Collections.Array { item } );
			// itemButton.Pressed += () => OnItemButtonPressed( slot );*/
		}
	}
	
	public void OnItemButtonPressed( InventorySlot slot )
	{
		var item = slot.GetItem();
		if ( item == null )
		{
			return;
		}
		GD.Print( $"Pressed item button for {item.GetItemData().Name}" );
		slot.Place();
	}

	public override void _Process( double delta )
	{
		
		if ( Input.IsActionJustPressed( "Inventory" ) )
		{
			Visible = !Visible;
		}
		
	}
}
