using Godot;
using vcrossing.Code.Carriable;
using vcrossing.Code.Components;
using vcrossing.Code.Inventory;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;

namespace vcrossing.Code.Ui;

public partial class InventoryEquipButton : Button
{

	[Export] public Components.Inventory Inventory;
	[Export] public Equips.EquipSlot EquipSlot;

	public override void _Ready()
	{
		base._Ready();
		UpdateSlot();
	}

	public void UpdateSlot()
	{

		if ( Inventory == null )
		{
			Logger.Warn( "Inventory not set" );
			Text = "ERROR";
			return;
		}

		var equipment = Inventory.Player.Equips.GetEquippedItem( EquipSlot );

		if ( equipment == null )
		{
			Icon = null;
			Text = EquipSlot.ToString();
			return;
		}

		if ( equipment is Carriable.BaseCarriable carriable )
		{
			// Text = carriable.GetName();
			Text = "";
			Icon = carriable.ItemData.GetIcon();
			TooltipText = $"{carriable.ItemData.Name}\n{carriable.ItemData.Description}\n{carriable.Durability}";
			return;
		}

		Icon = null;
		Text = EquipSlot.ToString();

	}

	/* public void SetEquipment( Carriable.BaseCarriable playerCurrentCarriable )
	{
		Equipment = playerCurrentCarriable;
		UpdateSlot();
	} */

	public override void _Pressed()
	{
		base._Pressed();
		Unequip();
	}

	private void Unequip()
	{

		if ( !Inventory.Player.Equips.HasEquippedItem( EquipSlot ) )
		{
			Logger.Warn( $"No item equipped in slot {EquipSlot}" ); // TODO: Show message to player
			return;
		}

		var index = Inventory.Container.GetFirstFreeEmptyIndex();
		if ( index == -1 )
		{
			Logger.Warn( "No free slots available" ); // TODO: Show message to player
			return;
		}

		var item = PersistentItem.Create( Inventory.Player.Equips.GetEquippedItem( EquipSlot ) );

		// var slot = new InventorySlot<PersistentItem>( Inventory );
		// slot.SetItem( item );
		// slot.Index = index;
		Inventory.PickUpItem( item );
		// Inventory.GetFirstFreeSlot()?.SetItem( item );
		// Inventory.Player.CurrentCarriable.QueueFree();
		// Inventory.Player.CurrentCarriable = null;

		// Inventory.Player.GetEquippedItem( EquipSlot ).QueueFree();

		Inventory.Player.Equips.RemoveEquippedItem( EquipSlot, true );

		// Equipment = null;
		UpdateSlot();
		// Inventory.Player.Save();
	}

}
