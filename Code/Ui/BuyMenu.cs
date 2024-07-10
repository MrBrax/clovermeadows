using System;
using vcrossing.Code.Data;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Inventory;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using static vcrossing.Code.Data.ShopInventoryData;

namespace vcrossing.Code.Ui;

public partial class BuyMenu : Control, IStopInput
{

	[Export] public Control ShopItemsContainer;

	[Export] public PackedScene BuyItemButtonScene;

	[Export] public Label ItemNameLabel;
	[Export] public Label ItemDescriptionLabel;
	[Export] public SpinBox ItemAmountSpinBox;
	[Export] public Node3D PreviewModelContainer;
	[Export] public Button BuyButton;

	[Export] public Node3D CameraPivot;

	private ShopItem SelectedItem;

	public override void _Ready()
	{
		base._Ready();
		Hide();
	}

	private void ClearList()
	{
		foreach ( Node child in ShopItemsContainer.GetChildren() )
		{
			child.QueueFree();
		}
	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		if ( IsVisibleInTree() )
		{
			CameraPivot.Rotation = new Vector3( 0, Mathf.PingPong( Time.GetTicksMsec() / 1000.0f, 360 ), 0 );
		}
	}

	public void LoadShopItems( List<ShopItem> shopItems, string shopName )
	{
		ClearList();

		foreach ( var shopItem in shopItems )
		{
			// var button = new Button();
			var button = BuyItemButtonScene.Instantiate<Button>();
			button.Text = $"{shopItem.ItemDataName} - {shopItem.Price}";
			button.Icon = shopItem.ItemData.Icon;
			button.GetNode<Label>( "Price" ).Text = shopItem.Price.ToString();
			button.Pressed += () => SelectItem( shopItem );
			ShopItemsContainer.AddChild( button );
		}

		if ( shopItems.Count > 0 )
		{
			SelectItem( shopItems[0] );
		}
	}

	private void SelectItem( ShopItem shopItem )
	{
		SelectedItem = shopItem;
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		ItemNameLabel.Text = SelectedItem.ItemData.Name;
		ItemDescriptionLabel.Text = !string.IsNullOrEmpty( SelectedItem.ItemData.Description ) ? SelectedItem.ItemData.Description : "No description";
		ItemAmountSpinBox.Value = 1;
		SetPreviewModel( SelectedItem.ItemData );
		OnAmountChanged( 1 );
	}

	public void OnAmountChanged( float value )
	{
		BuyButton.Text = $"Buy for {SelectedItem.Price * value}";
	}

	private void SetPreviewModel( ItemData itemData )
	{
		Logger.Info( $"Setting preview model for {itemData.Name}" );

		ClearPreviewModel();

		var model = itemData.CreateModelObject();
		if ( model == null )
		{
			Logger.Warn( $"Item {itemData.Name} does not have a model" );
			return;
		}

		PreviewModelContainer.AddChild( model );

		PreviewModelContainer.GetNodesOfType<MeshInstance3D>().ForEach( m => m.Layers = 1 << 15 ); // set culling layers
	}

	private void ClearPreviewModel()
	{
		foreach ( Node child in PreviewModelContainer.GetChildren() )
		{
			child.QueueFree();
		}
	}

	private void BuyCurrentItem()
	{
		if ( SelectedItem == null )
		{
			Logger.Info( "No item selected" );
			return;
		}

		BuyItem( SelectedItem );
	}

	private void BuyItem( ShopItem shopItem, int amount = 1 )
	{
		var player = GetNode<PlayerController>( "/root/Main/Player" ); // TODO: bind
		if ( !player.CanAfford( shopItem.Price * amount ) )
		{
			Logger.Warn( $"Player cannot afford item {shopItem.ItemData.Name}" );
			return;
		}

		if ( player.Inventory.Container.FreeSlots < amount )
		{
			Logger.Warn( $"Player does not have enough inventory space for {amount} items" );
			return;
		}

		for ( var i = 0; i < amount; i++ )
		{
			var item = PersistentItem.Create( shopItem.ItemData );
			player.Inventory.PickUpItem( item );
		}

		player.SpendClovers( shopItem.Price * amount );

		player.Save();

		GetNode<AudioStreamPlayer>( "ItemSold" ).Play();

		Logger.Info( $"Player bought {amount}x {shopItem.ItemData.Name}" );

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
		ClearList();
		ClearPreviewModel();
		Hide();
	}
}
