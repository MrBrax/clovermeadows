using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Ui;

public partial class UserInterface : Control
{

	[Export] public Label DateLabel;

	[Export] public TextureRect WeatherIcon;

	public override void _Ready()
	{
		base._Ready();
		GetNode<TimeManager>( "/root/Main/TimeManager" ).OnNewHour += ( hour ) =>
		{
			UpdateWeatherIcon();
		};

		UpdateWeatherIcon();
	}

	private void UpdateWeatherIcon()
	{
		var weatherManager = GetNode<WeatherManager>( "/root/Main/WeatherManager" );
		WeatherIcon.Texture = weatherManager.GetWeatherIcon();
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
