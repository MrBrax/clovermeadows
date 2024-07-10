using System;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Inventory;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using static vcrossing.Code.Data.ShopInventoryData;

namespace vcrossing.Code.Ui;

public partial class BuyMenu : Control, IStopInput
{

	[Export] public Control ShopItemsContainer;

	public void LoadShopItems( List<ShopItem> shopItems, string shopName )
	{
		foreach ( Node child in ShopItemsContainer.GetChildren() )
		{
			child.QueueFree();
		}

		foreach ( var shopItem in shopItems )
		{
			var button = new Button();
			button.Text = $"{shopItem.ItemDataName} - {shopItem.Price}";
			button.Pressed += () => BuyItem( shopItem );
			ShopItemsContainer.AddChild( button );
		}
	}

	private void BuyItem( ShopItem shopItem )
	{
		var player = GetNode<PlayerController>( "/root/Main/Player" ); // TODO: bind
		if ( !player.CanAfford( shopItem.Price ) )
		{
			Logger.Info( $"Player cannot afford item {shopItem.ItemData.Name}" );
			return;
		}

		var item = PersistentItem.Create( shopItem.ItemData );
		player.Inventory.PickUpItem( item );

		player.SpendClovers( shopItem.Price );

		player.Save();

		GetNode<AudioStreamPlayer>( "ItemSold" ).Play();

	}

	/* public override void _UnhandledInput( InputEvent @event )
	{
		AcceptEvent();
	}*/

	/* public override void _Input( InputEvent @event )
	{
		AcceptEvent();
	}  */

	/* public override void _GuiInput( InputEvent @event )
	{
		AcceptEvent();
	} */

	public void Close()
	{
		Hide();
	}
}
