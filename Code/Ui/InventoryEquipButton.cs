using Godot;
using vcrossing.Code.Carriable;
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

		if ( Equipment == null ) return;
		
		var item = PersistentItem.Create( Equipment );
		Inventory.GetFirstFreeSlot()?.SetItem( item );
		Inventory.Player.CurrentCarriable.QueueFree();
		Inventory.Player.CurrentCarriable = null;
		Equipment = null;
		UpdateSlot();
		// Inventory.Player.Save();
	}
}
