using System;
using vcrossing.Code.Data;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using vcrossing.Code.Ui;
using vcrossing.Code.WorldBuilder;
using YarnSpinnerGodot;
using static vcrossing.Code.Data.ShopInventoryData;

namespace vcrossing.Code.Items;

public partial class ShopTallDisplay : Node3D, IUsable
{

	[Export] public ItemCategoryData Category { get; set; }

	public List<ShopItem> Items { get; set; }

	public bool CanUse( PlayerController player )
	{
		return true;
	}

	public void OnUse( PlayerController player )
	{

		var ui = GetNode<UserInterface>( "/root/Main/UserInterface" );
		ui.CreateBuyMenu( Items, "Shop" );

	}
}
