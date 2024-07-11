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

	[Export] public MenuButton SortButton;

	private ShopItem SelectedItem;

	private int SortMode = 1;
	private bool SortAscending = true;

	private List<ShopItem> ShopItems;

	public override void _Ready()
	{
		base._Ready();
		Hide();

		SortButton.GetPopup().AddItem( "Name", 0 );
		SortButton.GetPopup().AddItem( "Price", 1 );
		SortButton.GetPopup().AddItem( "Category", 2 );
		SortButton.GetPopup().IdPressed += OnSortButtonPressed;
	}

	private void SetDefaultSort()
	{
		SortMode = 0;
		SortAscending = true;
		SortButton.Text = $"Sort ({SortButton.GetPopup().GetItemText( 0 )} {(SortAscending ? "↑" : "↓")})";
	}

	private void OnSortButtonPressed( long id )
	{
		Logger.Info( $"Sort button pressed: {id}" );

		if ( SortMode == id )
		{
			SortAscending = !SortAscending;
		}
		else
		{
			SortAscending = true;
		}

		SortMode = (int)id;

		SortButton.Text = $"Sort ({SortButton.GetPopup().GetItemText( (int)id )} {(SortAscending ? "↑" : "↓")})";

		SortItems();

		PopulateShopItemList();

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
		// ClearList();
		ShopItems = shopItems;
		SetDefaultSort();
		SortItems();
		PopulateShopItemList();
	}

	private void SortItems()
	{
		switch ( SortMode )
		{
			case 0:
				ShopItems = SortAscending ? ShopItems.OrderBy( i => i.ItemData.Name ).ToList() : ShopItems.OrderByDescending( i => i.ItemData.Name ).ToList();
				break;
			case 1:
				ShopItems = SortAscending ? ShopItems.OrderBy( i => i.Price ).ToList() : ShopItems.OrderByDescending( i => i.Price ).ToList();
				break;
				// case 3:
				// 	ShopItems = SortAscending ? ShopItems.OrderBy( i => i.ItemData.Category.Name ).ToList() : ShopItems.OrderByDescending( i => i.ItemData.Category.Name ).ToList();
				// 	break;
		}
	}

	private void PopulateShopItemList()
	{
		ClearList();
		var buttonGroup = new ButtonGroup();
		foreach ( var shopItem in ShopItems )
		{
			// var button = new Button();
			var button = BuyItemButtonScene.Instantiate<Button>();
			button.Text = $"{shopItem.ItemDataName}";
			button.Icon = shopItem.ItemData.Icon;
			button.GetNode<Label>( "Price" ).Text = shopItem.Price.ToString();
			button.Pressed += () => SelectItem( shopItem );
			button.ButtonGroup = buttonGroup;
			ShopItemsContainer.AddChild( button );
		}

		if ( ShopItems.Count > 0 )
		{
			SelectItem( ShopItems[0] );
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

		BuyItem( SelectedItem, (int)ItemAmountSpinBox.Value );
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
