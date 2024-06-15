using Godot;
using vcrossing.Code.Carriable;
using vcrossing.Code.Inventory;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Ui;

public partial class InventoryEquipButton : Button
{

	[Export] public Player.Inventory Inventory;
	[Export] public Node3D Equipment;

	public void UpdateSlot()
	{

		if ( Equipment == null )
		{
			Text = "";
			return;
		}

		if ( Equipment is Carriable.BaseCarriable carriable )
		{
			Text = carriable.GetName();
			return;
		}

		Text = "";

	}

	public void SetEquipment( Carriable.BaseCarriable playerCurrentCarriable )
	{
		Equipment = playerCurrentCarriable;
		UpdateSlot();
	}

	public override void _Pressed()
	{
		base._Pressed();
		Unequip();
	}

	private void Unequip()
	{

		if ( Equipment == null ) return;

		var index = Inventory.GetFirstFreeEmptyIndex();
		if ( index == -1 )
		{
			Logger.Warn( "No free slots available" ); // TODO: Show message to player
			return;
		}

		var item = PersistentItem.Create( Equipment );

		// var slot = new InventorySlot<PersistentItem>( Inventory );
		// slot.SetItem( item );
		// slot.Index = index;
		Inventory.PickUpItem( item );
		// Inventory.GetFirstFreeSlot()?.SetItem( item );
		// Inventory.Player.CurrentCarriable.QueueFree();
		// Inventory.Player.CurrentCarriable = null;

		Equipment = null;
		UpdateSlot();
		// Inventory.Player.Save();
	}

}
