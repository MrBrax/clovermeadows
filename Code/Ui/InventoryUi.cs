using vcrossing2.Code.Dependencies;
using vcrossing2.Code.Inventory;
using vcrossing2.Code.Persistence;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Ui;

public partial class InventoryUi : Control
{
	[Export, Require] public PlayerController Player;
	[Export, Require] public PackedScene InventorySlotButtonScene;

	[Export, Require] public GridContainer InventoryGrid;

	[Export, Require] public InventoryEquipButton EquipHatButton;
	[Export, Require] public InventoryEquipButton EquipShirtButton;
	[Export, Require] public InventoryEquipButton EquipPantsButton;
	[Export, Require] public InventoryEquipButton EquipShoesButton;
	[Export, Require] public InventoryEquipButton EquipToolButton;

	private Player.Inventory Inventory => Player.Inventory;

	public override void _Ready()
	{
		if ( Player == null )
		{
			throw new System.Exception( "Player not set." );
		}
		else if ( InventoryGrid == null )
		{
			throw new System.Exception( "InventoryGrid not set." );
		}

		// Player.Inventory.OnInventoryChanged += UpdateInventory;
		// Inventory.Connect( nameof( Inventory.InventoryChanged ), this, nameof( UpdateInventory ) );
		Player.Inventory.InventoryChanged += UpdateInventory;
		UpdateInventory();
		Visible = false;

		if ( EquipHatButton == null ) throw new System.Exception( "EquipHatButton not set." );
		EquipHatButton.Inventory = Inventory;

		if ( EquipShirtButton == null ) throw new System.Exception( "EquipShirtButton not set." );
		EquipShirtButton.Inventory = Inventory;

		if ( EquipPantsButton == null ) throw new System.Exception( "EquipPantsButton not set." );
		EquipPantsButton.Inventory = Inventory;

		if ( EquipShoesButton == null ) throw new System.Exception( "EquipShoesButton not set." );
		EquipShoesButton.Inventory = Inventory;

		if ( EquipToolButton == null ) throw new System.Exception( "EquipToolButton not set." );
		EquipToolButton.Inventory = Inventory;

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

		EquipToolButton.SetEquipment( Inventory.Player.CurrentCarriable ?? null );
	}

	public void OnItemButtonPressed( InventorySlot<PersistentItem> slot )
	{
		var item = slot.GetItem();
		if ( item == null )
		{
			return;
		}

		GD.Print( $"Pressed item button for {item.GetItemData().Name}" );
		slot.Place();
	}

	public void OnSortButtonPressed()
	{
		Inventory.SortSlots();
		UpdateInventory();
	}

	public override void _Process( double delta )
	{
		if ( Input.IsActionJustPressed( "Inventory" ) )
		{
			Visible = !Visible;
			if ( Visible ) UpdateInventory();
		}
	}
}
