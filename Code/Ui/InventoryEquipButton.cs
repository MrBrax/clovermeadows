﻿using Godot;
using vcrossing2.Code.Carriable;
using vcrossing2.Code.Persistence;

namespace vcrossing2.Code.Ui;

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

		if ( Equipment is BaseCarriable carriable )
		{
			Text = carriable.GetItemData().Name;
			return;
		}
		
		Text = "";
		
	}

	public void SetEquipment( BaseCarriable playerCurrentCarriable )
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
		Inventory.Player.Save();
	}
}