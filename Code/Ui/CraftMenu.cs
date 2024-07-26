using System;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using static vcrossing.Code.Data.ShopInventoryData;

namespace vcrossing.Code.Ui;

public partial class CraftMenu : Control, IStopInput
{

	[Export] public Control CraftRecipesContainer;

	[Export] public PackedScene CraftRecipeButtonScene;

	[Export] public Label ItemNameLabel;
	[Export] public Label ItemDescriptionLabel;
	// [Export] public SpinBox ItemAmountSpinBox;
	[Export] public Node3D PreviewModelContainer;
	[Export] public Button CraftButton;

	[Export] public Node3D CameraPivot;

	[Export] public AudioStream CraftSound;

	// [Export] public MenuButton SortButton;

	private RecipeData SelectedItem;

	private int SortMode = 1;
	private bool SortAscending = true;

	private IList<RecipeData> Recipes;

	public override void _Ready()
	{
		base._Ready();
		Hide();

		// SortButton.GetPopup().AddItem( "Name", 0 );
		// SortButton.GetPopup().AddItem( "Price", 1 );
		// SortButton.GetPopup().AddItem( "Category", 2 );
		// SortButton.GetPopup().IdPressed += OnSortButtonPressed;
	}

	/* private void SetDefaultSort()
	{
		SortMode = 0;
		SortAscending = true;
		SortButton.Text = $"Sort ({SortButton.GetPopup().GetItemText( 0 )} {(SortAscending ? "↑" : "↓")})";
	} */

	/* private void OnSortButtonPressed( long id )
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

	} */

	private void ClearList()
	{
		CraftRecipesContainer.QueueFreeAllChildren();
	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		if ( IsVisibleInTree() )
		{
			CameraPivot.Rotation = new Vector3( 0, Mathf.PingPong( Time.GetTicksMsec() / 1000.0f, 360 ), 0 );
		}
	}

	public void LoadRecipes( IList<RecipeData> recipes, string craftStationName )
	{
		// ClearList();
		Recipes = recipes;
		// SortItems();
		PopulateRecipeList();
	}

	public void LoadRecipes( CraftingStation.CraftingStationType type, string title )
	{
		var recipeFiles = Resources.GetFiles( $"res://recipes", ".*\\.tres" );

		Recipes = new List<RecipeData>();
		foreach ( var recipeFile in recipeFiles )
		{
			var recipe = Loader.LoadResource<RecipeData>( recipeFile );
			if ( recipe.CraftingStation == type )
			{
				Recipes.Add( recipe );
			}
		}

		PopulateRecipeList();

	}

	/* private void SortItems()
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
	} */

	private void PopulateRecipeList()
	{
		ClearList();
		var buttonGroup = new ButtonGroup();
		foreach ( var recipe in Recipes )
		{
			// var button = new Button();
			var button = CraftRecipeButtonScene.Instantiate<Button>();
			button.Text = $"{recipe.GetDisplayName()}";
			// button.Icon = recipe.ItemData.GetIcon();
			// button.GetNode<Label>( "Price" ).Text = recipe.Price.ToString();
			button.Pressed += () => SelectRecipe( recipe );
			button.ButtonGroup = buttonGroup;
			button.Disabled = !recipe.HasIngredients( NodeManager.Player.Inventory.Container );
			CraftRecipesContainer.AddChild( button );
		}

		if ( Recipes.Count > 0 )
		{
			SelectRecipe( Recipes[0] );
		}
	}

	private void SelectRecipe( RecipeData shopItem )
	{
		SelectedItem = shopItem;
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		ItemNameLabel.Text = SelectedItem.GetDisplayName();
		ItemDescriptionLabel.Text = !string.IsNullOrWhiteSpace( SelectedItem.GetDescription() ) ? SelectedItem.GetDescription() : "No description";
		// ItemAmountSpinBox.Value = 1;
		// SetPreviewModel( SelectedItem.ItemData );
		OnAmountChanged( 1 );
	}

	public void OnAmountChanged( float value )
	{
		// CraftButton.Text = $"Buy for {SelectedItem.Price * value}";
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

	private void CraftCurrentRecipe()
	{
		if ( SelectedItem == null )
		{
			// Logger.Info( "No item selected" );
			NodeManager.UserInterface.ShowWarning( "No item selected" );
			return;
		}

		CraftItem( SelectedItem );
	}

	private void CraftItem( RecipeData recipe, int amount = 1 )
	{
		var player = NodeManager.Player;

		if ( !recipe.HasIngredients( player.Inventory.Container ) )
		{
			NodeManager.UserInterface.ShowWarning( "Not enough ingredients." );
			return;
		}

		var results = recipe.GetResults();
		if ( !player.Inventory.Container.CanFit( results ) )
		{
			NodeManager.UserInterface.ShowWarning( "Not enough space in inventory" );
			return;
		}

		recipe.TakeIngredients( player.Inventory.Container );

		foreach ( var result in results )
		{
			player.Inventory.Container.AddItem( result, true );
		}

		NodeManager.UserInterface.ShowWarning( $"Crafted {recipe.GetDisplayName()}" );

		UiSounds.PlaySound( CraftSound );

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
