using System;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder.Weather;

namespace vcrossing.Code.WorldBuilder;

[Icon( "res://icons/editor/cloud.svg" )]
public partial class WeatherManager : Node3D
{

	public enum WeatherEffects
	{
		None = 0,
		Rain = 1,
		Lightning = 2,
		Wind = 4,
		Fog = 8
	}

	[Export] public bool IsInside { get; set; } = false;
	[Export] public WorldEnvironment Environment { get; set; }
	[Export] public DirectionalLight3D SunLight { get; set; }

	[Export] public TimeManager TimeManager { get; set; }
	[Export] public WorldManager WorldManager { get; set; }

	private int StringToInt( string input )
	{
		var hash = 0;
		for ( int i = 0; i < input.Length; i++ )
		{
			hash = input[i] + (hash << 6) + (hash << 16) - hash;
		}
		return hash;
	}

	public bool PrecipitationEnabled { get; private set; }
	public bool LightningEnabled { get; private set; }
	public bool WindEnabled { get; private set; }
	public bool FogEnabled { get; private set; }

	protected float GetStaticFloat( string seed )
	{
		return GetStaticFloat( StringToInt( seed ) );
	}

	protected float GetStaticFloat( int seed )
	{
		var random = new Random( seed );
		return (float)random.NextSingle();
	}

	protected float GetPrecipitationChance( DateTime time )
	{
		var input = $"{time.DayOfYear}{time.Hour}-precipitation";
		return GetStaticFloat( input );
	}

	protected int GetPrecipitationLevel( DateTime time )
	{
		var input = $"{time.DayOfYear}{time.Hour}-precipitation-level";
		var value = GetStaticFloat( input );
		if ( value < 0.2f )
		{
			return 1;
		}
		else if ( value < 0.5f )
		{
			return 2;
		}
		else
		{
			return 3;
		}
	}

	/// <summary>
	/// Returns a float between -180 and 180 representing the wind direction.
	/// </summary>
	protected float GetWindDirection( DateTime time )
	{
		var input = $"{time.DayOfYear}{time.Hour}-wind-direction";
		return GetStaticFloat( input ) * 360f - 180f;
	}

	protected float GetLightningChance( DateTime time )
	{
		var input = $"{time.DayOfYear}{time.Hour}-lightning";
		return GetStaticFloat( input );
	}

	protected float GetFogChance( DateTime time )
	{
		var input = $"{time.DayOfYear}{time.Hour}-fog";
		return GetStaticFloat( input );
	}

	protected float GetCloudDensity( DateTime time )
	{
		var input = $"{time.DayOfYear}{time.Hour}-cloud-density";
		return GetStaticFloat( input );
	}

	public override void _Ready()
	{
		base._Ready();

		// Logger.Info( "WeatherManager", $"Precipitation chance: {GetPrecipitationChance( DateTime.Now )}" );
		// Logger.Info( "WeatherManager", $"Lightning chance: {GetLightningChance( DateTime.Now )}" );

		// debug check today's weather
		for ( int i = 0; i < 24; i++ )
		{
			var time = new DateTime( 2024, 6, 14, i, 0, 0 );
			var weather = GetWeather( time );
			Logger.Info( "WeatherManager", $"Weather @ {time.ToString( "h tt" )}: Rain: {weather.RainLevel}, Lightning: {weather.Lightning}, Wind: {weather.WindLevel}, Fog: {weather.Fog}, CloudDensity: {weather.CloudDensity}" );
		}

		Setup();

		TimeManager.OnNewHour += ( hour ) =>
		{
			Setup();
		};

		WorldManager.WorldLoaded += ( world ) =>
		{
			IsInside = world.IsInside;
			Setup( true );
		};

	}

	public struct WeatherReport
	{
		// public bool Rain;
		public int RainLevel;
		public bool Lightning;
		// public bool Wind;
		public int WindLevel;
		public bool Fog;
		public float CloudDensity;

		public float WindDirection;
		// public float WindSpeed;

		public readonly bool Rain => RainLevel > 0;
		public readonly bool Wind => WindLevel > 0;
	}

	public WeatherReport GetWeather( DateTime time )
	{
		var weather = new WeatherReport();
		var precipitationChance = GetPrecipitationChance( time );
		var lightningChance = GetLightningChance( time );
		var fogChance = GetFogChance( time );

		if ( precipitationChance > 0.8f )
		{
			weather.RainLevel = GetPrecipitationLevel( time );
			weather.Lightning = lightningChance > 0.8f;
			weather.Fog = fogChance > 0.6f;
			weather.WindLevel = 1;
			weather.CloudDensity = 0.5f + GetCloudDensity( time ) * 0.5f;
		}
		else
		{
			weather.RainLevel = 0;
			weather.Lightning = lightningChance > 0.9f;

			// higher chance of fog in the morning
			weather.Fog = time.Hour > 3 && time.Hour < 7 ? fogChance > 0.2f : fogChance > 0.8f;

			weather.WindLevel = 0;
			weather.CloudDensity = GetCloudDensity( time ) * 0.5f;
		}

		weather.WindDirection = GetWindDirection( time );

		return weather;

	}

	public WeatherReport GetCurrentWeather()
	{
		return GetWeather( TimeManager.Time );
	}


	private void Setup( bool instant = false )
	{

		Logger.Info( "WeatherManager", "Setting up weather" );

		var now = TimeManager.Time;


		var weather = GetWeather( now );

		SetPrecipitation( weather.RainLevel, weather.WindDirection, weather.WindLevel * 3f, true );
		SetLightning( weather.Lightning, true );
		SetWind( weather.Wind, true );
		SetFog( weather.Fog, true );
		SetCloudDensity( weather.CloudDensity, true );

		Logger.Info( "WeatherManager", $"Weather {now.Hour}: Rain: {weather.RainLevel}, Lightning: {weather.Lightning}, Wind: {weather.WindLevel}, Fog: {weather.Fog}, CloudDensity: {weather.CloudDensity}" );
	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		var player = GetNode<PlayerController>( "/root/Main/Player" );
		if ( player == null )
		{
			return;
		}

		GlobalPosition = player.GlobalPosition;

	}

	private void SetPrecipitation( int level, float direction, float windSpeed, bool instant = false )
	{
		PrecipitationEnabled = level > 0;
		var rainInside = GetNode<Rain>( "RainInside" );
		var rainOutside = GetNode<Rain>( "RainOutside" );
		if ( IsInside )
		{
			rainOutside.SetLevel( 0 );
			// rainInside.SetLevelSmooth( level );
			if ( instant )
			{
				rainInside.SetLevel( level );
			}
			else
			{
				rainInside.SetLevelSmooth( level );
			}
			rainInside.SetWind( direction, windSpeed );
		}
		else
		{
			rainInside.SetLevel( 0 );
			// rainOutside.SetLevelSmooth( level );
			if ( instant )
			{
				rainOutside.SetLevel( level );
			}
			else
			{
				rainOutside.SetLevelSmooth( level );
			}
			rainOutside.SetWind( direction, windSpeed );
		}
	}

	private void SetLightning( bool state, bool instant = false )
	{
		LightningEnabled = state;
		if ( IsInside )
		{
			GetNode<Lightning>( "LightningInside" ).SetEnabled( state );
		}
		else
		{
			GetNode<Lightning>( "LightningOutside" ).SetEnabled( state );
		}
	}

	private void SetWind( bool state, bool instant = false )
	{
		WindEnabled = state;
		if ( state && IsInside ) return; // no wind inside
										 // GetNode<Wind>( "Wind" ).SetEnabledSmooth( state );
		if ( instant )
		{
			GetNode<Wind>( "Wind" ).SetEnabled( state );
		}
		else
		{
			GetNode<Wind>( "Wind" ).SetEnabledSmooth( state );
		}
	}

	private void SetFog( bool state, bool instant = false )
	{
		FogEnabled = state;
		// if ( Environment == null ) return;
		// Environment.Environment.FogDensity = state ? 0.04f : 0.0f;
		// GetNode<Fog>( "Fog" ).SetEnabledSmooth( state );
		/* if ( PrecipitationEnabled )
		{
			GetNode<Rain>( "RainOutside" )?.SetFogState( state );
		} */

		if ( instant )
		{
			GetNode<Fog>( "Fog" ).SetEnabled( state );
		}
		else
		{
			GetNode<Fog>( "Fog" ).SetEnabledSmooth( state );
		}
	}

	private void SetCloudDensity( float density, bool instant = false )
	{
		if ( SunLight == null )
		{
			Logger.LogError( "WeatherManager", "SunLight is null" );
			return;
		}
		Logger.Info( "WeatherManager", $"Setting cloud density to {density}" );
		SunLight.ShadowBlur = density * 2f;
	}

	public Texture2D GetWeatherIcon()
	{
		var now = TimeManager.Time;
		var weather = GetWeather( now );

		var isDay = TimeManager.IsDay;

		if ( isDay )
		{
			if ( weather.RainLevel > 0 )
			{
				return Loader.LoadResource<Texture2D>( "res://icons/weather/partly-cloudy-day-rain.png" );
			}

			if ( weather.Fog )
			{
				return Loader.LoadResource<Texture2D>( "res://icons/weather/fog-day.png" );
			}

			return Loader.LoadResource<Texture2D>( "res://icons/weather/clear-day.png" );

		}
		else
		{

			if ( weather.Rain )
			{
				return Loader.LoadResource<Texture2D>( "res://icons/weather/partly-cloudy-night-rain.png" );
			}

			if ( weather.Fog )
			{
				return Loader.LoadResource<Texture2D>( "res://icons/weather/fog-night.png" );
			}

			return Loader.LoadResource<Texture2D>( "res://icons/weather/clear-night.png" );

		}

		/* if ( weather.RainLevel > 0 )
		{
			return Loader.LoadResource<Texture2D>( "res://icons/weather/rainy.png" );
		}
		else if ( weather.Lightning )
		{
			return Loader.LoadResource<Texture2D>( "res://icons/weather/thunderstorm.png" );
		}
		else if ( weather.Fog )
		{
			// return Loader.LoadResource<Texture2D>( "res://assets/weather/fog.png" );
		}*/

		return Loader.LoadResource<Texture2D>( "res://icons/weather/barometer.png" );

	}
}
