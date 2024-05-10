using Godot;
using vcrossing2.Code.Player;
using vcrossing2.Inventory;

namespace vcrossing2.Code.Ui;

public partial class InventoryUi : Control
{
	
	[Export] public PlayerController Player;
	[Export] public GridContainer InventoryGrid;
	
	private Player.Inventory Inventory => Player.Inventory;
	
	public override void _Ready()
	{
		Player.Inventory.OnInventoryChanged += UpdateInventory;
		UpdateInventory();
	}
	
	public void UpdateInventory()
	{
		foreach ( Node child in InventoryGrid.GetChildren() )
		{
			child.QueueFree();
		}
		
		foreach ( var item in Player.Inventory.GetItems() )
		{
			var itemButton = new Button();
			itemButton.Text = item.GetItemData().Name;
			// itemButton.RectMinSize = new Vector2( 100, 100 );
			InventoryGrid.AddChild( itemButton );
			
			// itemButton.Connect( "pressed", this, nameof( OnItemButtonPressed ), new Godot.Collections.Array { item } );
			itemButton.Pressed += () => OnItemButtonPressed( item );
		}
	}
	
	public void OnItemButtonPressed( InventoryItem item )
	{
		GD.Print( $"Pressed item button for {item.GetItemData().Name}" );
		Inventory.PlaceItem( item );
	}
	
}
