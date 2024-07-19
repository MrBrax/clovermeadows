using vcrossing.Code.Dependencies;
using vcrossing.Code.Inventory;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using static vcrossing.Code.Data.ShopInventoryData;

namespace vcrossing.Code.Ui;

public partial class InventoryUi : Control, IStopInput
{
	[Export, Require] public PlayerController Player;
	[Export, Require] public PackedScene InventorySlotButtonScene;

	[Export, Require] public GridContainer InventoryGrid;

	[Export, Require] public InventoryEquipButton EquipHatButton;
	[Export, Require] public InventoryEquipButton EquipShirtButton;
	[Export, Require] public InventoryEquipButton EquipPantsButton;
	[Export, Require] public InventoryEquipButton EquipShoesButton;
	[Export, Require] public InventoryEquipButton EquipToolButton;

	[Export] public Label CloverAmountLabel;

	private Components.Inventory Inventory => Player.Inventory;

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
		Player.Inventory.Container.InventoryChanged += UpdateInventory;
		Player.PlayerCloversChanged += ( int oldClovers, int newClovers ) => CloverAmountLabel.Text = newClovers.ToString( "N0" );

		UpdateInventory();
		Visible = false;

		EquipHatButton?.SetInventory( Inventory );
		EquipShirtButton?.SetInventory( Inventory );
		EquipPantsButton?.SetInventory( Inventory );
		EquipShoesButton?.SetInventory( Inventory );
		EquipToolButton?.SetInventory( Inventory );

	}

	public void DeleteAll()
	{
		Inventory.Container.DeleteAll();
	}

	public void UpdateInventory()
	{
		foreach ( Node child in InventoryGrid.GetChildren() )
		{
			child.QueueFree();
		}

		Logger.Info( "UpdateInventory", $"Inventory has {Inventory.Container.GetUsedSlots().Count()} items out of {Inventory.Container.MaxItems}." );

		foreach ( var entry in Player.Inventory.Container.GetEnumerator() )
		{
			// var itemButton = new InventorySlotButton( slot );
			var itemButton = InventorySlotButtonScene.Instantiate<InventorySlotButton>();

			itemButton.Index = entry.Index;
			itemButton.Slot = entry.HasSlot ? entry.Slot : null;
			itemButton.Name = $"InventorySlotButton{entry.Index}";
			itemButton.PlayerInventory = Inventory;

			/*if ( slot.HasItem )
			{
				itemButton.Text = slot.GetItem().GetItemData().Name;
			}*/

			// itemButton.RectMinSize = new Vector2( 100, 100 );
			InventoryGrid.AddChild( itemButton );

			// itemButton.Connect( "pressed", this, nameof( OnItemButtonPressed ), new Godot.Collections.Array { item } );
			// itemButton.Pressed += () => OnItemButtonPressed( slot );*/
		}

		// EquipToolButton.SetEquipment( Inventory.Player.GetEquippedItem<Carriable.BaseCarriable>( PlayerController.EquipSlot.Tool ) );
		EquipHatButton.UpdateSlot();
		EquipShirtButton.UpdateSlot();
		EquipPantsButton.UpdateSlot();
		EquipShoesButton.UpdateSlot();
		EquipToolButton.UpdateSlot();

		CloverAmountLabel.Text = Inventory.Player.Clovers.ToString( "N0" );
	}

	public void OnItemButtonPressed( InventorySlot<PersistentItem> slot )
	{
		var item = slot.GetItem();
		if ( item == null )
		{
			return;
		}

		GD.Print( $"Pressed item button for {item.GetName()}" );
		slot.Place();
	}

	public void OnSortButtonPressed()
	{
		Inventory.Container.SortByName();
		UpdateInventory();
		UiSounds.ButtonDown();
	}

	public void GiveMoney()
	{
		Player.AddClovers( 1000 );
	}

	public void ClearMoney()
	{
		Player.SetClovers( 0 );
	}

	/* public override void _Process( double delta )
	{
		if ( Input.IsActionJustPressed( "Inventory" ) )
		{
			Visible = !Visible;
			if ( Visible ) UpdateInventory();
		}
	} */

	public override void _Input( InputEvent @event )
	{
		if ( GetNode<UserInterface>( "/root/Main/UserInterface" ).IsPaused ) return;

		if ( @event.IsActionPressed( "Inventory" ) )
		{
			Visible = !Visible;
			if ( Visible ) UpdateInventory();
		}

	}

	public void OpenPatterns()
	{
		// GetNode<PatternUi>( "/root/Main/UserInterface/PatternUi" ).Visible = true;
	}
}
