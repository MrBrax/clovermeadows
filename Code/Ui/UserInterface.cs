using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Ui;

public partial class UserInterface : Control
{

	[Export] public Label DateLabel;

	[Export] public TextureRect WeatherIcon;

	[Export] public Label FpsLabel;

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

	public override void _Ready()
	{
		base._Ready();
		GetNode<TimeManager>( "/root/Main/TimeManager" ).OnNewHour += ( hour ) =>
		{
			UpdateWeatherIcon();
		};

		UpdateWeatherIcon();
		IsPaused = false;
	}

	private void UpdateWeatherIcon()
	{
		var weatherManager = GetNode<WeatherManager>( "/root/Main/WeatherManager" );
		WeatherIcon.Texture = weatherManager.GetWeatherIcon();
	}

	public override void _Input( InputEvent @event )
	{
		base._Input( @event );

		if ( @event is InputEventKey keyEvent )
		{
			if ( keyEvent.IsActionPressed( "ui_cancel" ) )
			{
				IsPaused = !IsPaused;
				// GetTree().Paused = IsPaused; // don't actually pause the game
			}
		}
	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		var timeManager = GetNode<TimeManager>( "/root/Main/TimeManager" );
		DateLabel.Text = timeManager.GetTime();

		// var weatherManager = GetNode<WeatherManager>( "/root/Main/WeatherManager" );
		// WeatherIcon.Texture = weatherManager.GetWeatherIcon();

		FpsLabel.Text = Engine.GetFramesPerSecond().ToString();
	}

}
