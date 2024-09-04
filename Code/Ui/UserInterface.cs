using System;
using vcrossing.Code.WorldBuilder;
using static vcrossing.Code.Data.ShopInventoryData;

namespace vcrossing.Code.Ui;

public partial class UserInterface : Control
{

	[Export] public Label DateLabel;

	[Export] public TextureRect WeatherIcon;

	[Export] public Label FpsLabel;

	[Export] public Godot.Collections.Array<Control> Windows = new();

	private bool _isPaused;
	public bool IsPaused
	{
		get => _isPaused;
		set
		{
			_isPaused = value;
			GetNode<Control>( "PauseMenu" ).Visible = value;
		}
	}

	public bool AreWindowsOpen
	{
		get
		{
			return Windows.Any( window => window is IStopInput && window.Visible );
		}
	}

	public override void _Ready()
	{
		base._Ready();
		NodeManager.TimeManager.OnNewHour += ( hour ) =>
		{
			UpdateWeatherIcon();
		};

		UpdateWeatherIcon();
		IsPaused = false;

		Input.SetCustomMouseCursor( Loader.LoadResource<Texture>( "res://icons/cursor/hand_closed.png" ), Input.CursorShape.Drag );
	}

	private void UpdateWeatherIcon()
	{
		var weatherManager = NodeManager.WeatherManager;
		WeatherIcon.Texture = weatherManager.GetWeatherIcon();
	}

	public override void _Input( InputEvent @event )
	{
		base._Input( @event );
		if ( @event.IsActionPressed( "ui_cancel" ) )
		{
			IsPaused = !IsPaused;
			// GetTree().Paused = IsPaused; // don't actually pause the game
		}
	}

	/* public override void _GuiInput( InputEvent @event )
	{
		if ( @event.IsActionPressed( "ui_cancel" ) )
		{
			IsPaused = !IsPaused;
			// GetTree().Paused = IsPaused; // don't actually pause the game
		}
	} */

	public override void _Process( double delta )
	{
		base._Process( delta );

		var timeManager = NodeManager.TimeManager;
		DateLabel.Text = timeManager.GetTime();

		// var weatherManager = GetNode<WeatherManager>( "/root/Main/WeatherManager" );
		// WeatherIcon.Texture = weatherManager.GetWeatherIcon();

		FpsLabel.Text = Engine.GetFramesPerSecond().ToString();
	}

	public void CreateBuyMenu( IList<ShopItem> shopItems, string shopName )
	{
		var buyMenu = GetNodeOrNull<BuyMenu>( "BuyMenu" );
		if ( buyMenu == null ) throw new System.Exception( "BuyMenu not found" );
		buyMenu.LoadShopItems( shopItems, shopName );
		buyMenu.Show();
	}

	public void CreateCraftMenu( Items.CraftingStation.CraftingStationType type, string title )
	{
		var craftMenu = GetNodeOrNull<CraftMenu>( "CraftingMenu" );
		if ( craftMenu == null ) throw new System.Exception( "CraftMenu not found" );
		craftMenu.LoadRecipes( type, title );
		craftMenu.Show();
	}

	public void ShowWarning( string text )
	{
		var dialog = NodeManager.UserInterface.GetNode<AcceptDialog>( "AcceptDialog" );
		dialog.DialogText = text;
		dialog.PopupCentered();

		Logger.Warn( "UserWarning", text );
	}

	public void HideWarning()
	{
		NodeManager.UserInterface.GetNode<AcceptDialog>( "AcceptDialog" ).Hide();
	}

}
