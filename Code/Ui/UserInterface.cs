using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Ui;

public partial class UserInterface : Control
{

	[Export] public Label DateLabel;

	[Export] public TextureRect WeatherIcon;

	public bool IsPaused { get; set; }

	public override void _Ready()
	{
		base._Ready();
		GetNode<TimeManager>( "/root/Main/TimeManager" ).OnNewHour += ( hour ) =>
		{
			UpdateWeatherIcon();
		};

		UpdateWeatherIcon();

		GetNode<Control>( "PauseMenu" ).Visible = false;
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
			if ( keyEvent.IsActionPressed( "ui_pause" ) || keyEvent.IsActionPressed( "ui_cancel" ) )
			{
				IsPaused = !IsPaused;
				// GetTree().Paused = IsPaused; // don't actually pause the game

				GetNode<Control>( "PauseMenu" ).Visible = IsPaused;
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
	}

}
