using System;
using Godot;
using vcrossing.Code.Helpers;
using vcrossing.Code.Objects;

namespace vcrossing.Code.WorldBuilder;

[Icon( "res://icons/editor/clock-solid.svg" )]
public partial class TimeManager : Node3D
{
	public DirectionalLight3D Sun;

	// public double TimeOfDaySeconds => Godot.Time.GetUnixTimeFromSystem();

	private WorldEnvironment _environment;

	/// <summary>
	/// The main source of truth for the current time. Is scaled by the Speed property.
	/// </summary>
	public DateTime Time => DateTime.Now; // .AddSeconds( Godot.Time.GetTicksMsec() * 3f );

	private const float SecondsPerDay = 86400f;

	[Signal]
	public delegate void OnNewHourEventHandler( int hour );

	[Signal]
	public delegate void OnNewMinuteEventHandler( int minute );

	public bool IsNight => Time.Hour < 6 || Time.Hour > 18;
	public bool IsDay => !IsNight;

	public override void _Ready()
	{
		/* if ( Sun == null )
		{
			throw new System.Exception( "Sun not set." );
		} */

		OnNewHour += PlayChime;

		NodeManager.WorldManager.WorldLoaded += ( world ) =>
		{
			FindSun();
			FindEnvironment();
		};

		FindSun();

		_lastHour = Time.Hour;

		OnNewMinute += ( minute ) =>
		{
			if ( GD.Randf() > 0.95f )
			{
				GiftCarrier.SpawnRandom();
			}
		};

	}

	private void FindEnvironment()
	{
		var worldEnvironment = GetTree().GetNodesInGroup( "worldenvironment" );
		if ( worldEnvironment.Count == 0 )
		{
			Logger.Warn( "DayNightCycle", "No WorldEnvironment found." );
			return;
		}

		if ( worldEnvironment.Count > 1 )
		{
			Logger.Warn( "DayNightCycle", $"Multiple WorldEnvironments found: {worldEnvironment.Count}" );
		}

		_environment = worldEnvironment[0] as WorldEnvironment;
	}

	private void FindSun()
	{
		var suns = GetTree().GetNodesInGroup( "sunlight" );
		if ( suns.Count == 0 )
		{
			Logger.Warn( "DayNightCycle", "No sun found in sunlight group." );
			return;
		}

		if ( suns.Count > 1 )
		{
			Logger.Warn( "DayNightCycle", $"Multiple suns found in sunlight group: {suns.Count}" );
		}

		Sun = suns[0] as DirectionalLight3D;
	}

	private void PlayChime( int hour )
	{
		Logger.Info( "DayNightCycle", $"Playing chime for hours {hour}" );
		var chime = GetNode<AudioStreamPlayer>( "Chime" );
		/* for ( int i = 0; i < hour; i++ )
		{
			ToSignal( GetTree().CreateTimer( i * 1f ), Timer.SignalName.Timeout ).OnCompleted( () =>
			{
				Logger.Info( "DayNightCycle", $"Playing chime for hour {i}" );
				chime.Play();
			} );
		} */
		chime.Play();

	}

	private readonly List<Color> _skyColors = new()
	{
		Color.Color8 ( 1, 1, 3 ), // 00:00
		Color.Color8 ( 1, 1, 3 ), // 01:00
		Color.Color8 ( 1, 1, 3 ), // 02:00
		Color.Color8 ( 1, 1, 3 ), // 03:00
		Color.Color8 ( 1, 1, 3 ), // 04:00
		Color.Color8 ( 1, 1, 3 ), // 05:00
		Color.Color8 ( 1, 1, 3 ), // 06:00
		Color.Color8 ( 255, 100, 100 ), // 07:00 -- sunrise
		Color.Color8 ( 255, 255, 255 ), // 08:00
		Color.Color8 ( 255, 255, 255 ), // 09:00
		Color.Color8 ( 255, 255, 255 ), // 10:00
		Color.Color8 ( 255, 255, 255 ), // 11:00
		Color.Color8 ( 255, 255, 255 ), // 12:00
		Color.Color8 ( 255, 255, 255 ), // 13:00
		Color.Color8 ( 255, 255, 255 ), // 14:00
		Color.Color8 ( 255, 255, 255 ), // 15:00
		Color.Color8 ( 250, 240, 240 ), // 16:00
		Color.Color8 ( 220, 100, 100 ), // 17:00 -- sunset
		Color.Color8 ( 1, 1, 3 ), // 18:00
		Color.Color8 ( 1, 1, 3 ), // 19:00
		Color.Color8 ( 1, 1, 3 ), // 20:00
		Color.Color8 ( 1, 1, 3 ), // 21:00
		Color.Color8 ( 1, 1, 3 ), // 22:00
		Color.Color8 ( 1, 1, 3 ), // 23:00
	};

	private int _lastHour = -1;
	private int _lastMinute = -1;

	public override void _Process( double delta )
	{
		if ( IsInstanceValid( Sun ) )
		{
			Sun.Rotation = CalculateSunRotation( Sun );
			Sun.LightEnergy = CalculateSunEnergy( Sun );
			Sun.LightColor = CalculateSunColor( Sun );
			// GetTree().CallGroup( "debugdraw", "add_line", Sun.GlobalTransform.Origin, Sun.GlobalTransform.Origin + Sun.GlobalTransform.Basis.Z * 0.5f, new Color( 1, 1, 1 ), 0.2f );
		}

		if ( IsInstanceValid( _environment ) )
		{
			_environment.Environment.BackgroundEnergyMultiplier = CalculateSunEnergy( Sun );
		}

		var hour = Time.Hour;
		// Logger.Info( "DayNightCycle", $"Hour: {hour}" );
		if ( hour != _lastHour )
		{
			// Logger.Info( "DayNightCycle", $"New hour: {hour}" );
			_lastHour = hour;
			EmitSignal( SignalName.OnNewHour, hour );
		}

		var minute = Time.Minute;
		if ( minute != _lastMinute )
		{
			// Logger.Info( "DayNightCycle", $"New minute: {minute}" );
			_lastMinute = minute;
			EmitSignal( SignalName.OnNewMinute, minute );
		}
	}

	private float DayFraction => (float)(Time.Hour * 3600 + Time.Minute * 60 + Time.Second) / SecondsPerDay;

	private Color GetComputedSkyColor()
	{
		var baseIndex = MathF.Floor( DayFraction * _skyColors.Count );
		var nextIndex = (int)Math.Ceiling( DayFraction * _skyColors.Count ) % _skyColors.Count;
		var baseColor = _skyColors[(int)baseIndex];
		var nextColor = _skyColors[nextIndex];
		var lerp = DayFraction * _skyColors.Count - baseIndex;
		var color = baseColor.Lerp( nextColor, lerp );
		return color;
	}

	private Color CalculateSunColor( DirectionalLight3D sun )
	{
		return GetComputedSkyColor();
	}

	private Vector3 CalculateSunRotation( DirectionalLight3D sun )
	{

		var time = Time;
		var hours = time.Hour;
		var minutes = time.Minute;
		var seconds = time.Second;
		var msec = time.Millisecond;

		var totalSeconds = hours * 3600 + minutes * 60 + seconds + msec / 1000f;
		var totalSecondsInDay = 24 * 3600;

		var angle = Mathf.Pi * 2 * totalSeconds / totalSecondsInDay;

		var rotation = new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), 0 );

		return rotation;

	}

	/// <summary>
	///  A value between 0 and 1 used to calculate the sun's energy (0 at night, 1 at midday).
	/// </summary>
	/// <param name="sun"></param>
	/// <returns></returns>
	private float CalculateSunEnergy( DirectionalLight3D sun )
	{
		var time = Time;
		var hours = time.Hour;
		var minutes = time.Minute;
		var seconds = time.Second;
		var msec = time.Millisecond;

		var totalSeconds = hours * 3600 + minutes * 60 + seconds + msec / 1000f;
		var totalSecondsInDay = 24 * 3600;

		var energy = Mathf.Abs( Mathf.Sin( Mathf.Pi * 2 * totalSeconds / totalSecondsInDay ) );

		return energy;

	}

	internal string GetDate()
	{
		return Time.ToString( "yyyy-MM-dd HH:mm:ss" );
	}

	internal string GetTime()
	{
		return Time.ToString( "h:mm tt" );
	}
}
