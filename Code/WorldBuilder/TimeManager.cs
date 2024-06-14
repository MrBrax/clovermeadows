using System;
using Godot;
using vcrossing.Code.Helpers;

namespace vcrossing.Code.WorldBuilder;

public partial class TimeManager : Node3D
{
	public DirectionalLight3D Sun;
	// [Export] public float Speed = 1;
	[Export] public float MaxSunAngle = 70f;

	public double TimeOfDaySeconds => Godot.Time.GetUnixTimeFromSystem();

	/// <summary>
	/// The main source of truth for the current time. Is scaled by the Speed property.
	/// </summary>
	public DateTime Time => DateTime.Now.AddSeconds( Godot.Time.GetTicksMsec() * 0.3f );

	[Signal]
	public delegate void OnNewHourEventHandler( int hour );

	public override void _Ready()
	{
		/* if ( Sun == null )
		{
			throw new System.Exception( "Sun not set." );
		} */

		OnNewHour += PlayChime;

		GetNode<WorldManager>( "/root/Main/WorldManager" ).WorldLoaded += ( world ) =>
		{
			FindSun();
		};

		FindSun();

		_lastHour = Time.Hour;

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

	private int _lastHour = -1;

	public override void _Process( double delta )
	{
		if ( IsInstanceValid( Sun ) )
		{
			// var sunAngleDegrees = (Godot.Time.GetTicksMsec() * Speed) % 360;
			// Sun.RotationDegrees = new Vector3( Mathf.Clamp( sunAngleDegrees, -MaxSunAngle, MaxSunAngle ), 45, 0 );

			if ( Math.Abs( Sun.RotationDegrees.X ) >= MaxSunAngle )
			{
				Sun.LightEnergy = 0;
			}
			else
			{
				Sun.LightEnergy = 1;
			}
		}

		var hour = Time.Hour;
		// Logger.Info( "DayNightCycle", $"Hour: {hour}" );
		if ( hour != _lastHour )
		{
			Logger.Info( "DayNightCycle", $"New hour: {hour}" );
			_lastHour = hour;
			EmitSignal( SignalName.OnNewHour, hour );
		}
	}

	internal string GetDate()
	{
		return Time.ToString( "yyyy-MM-dd HH:mm:ss" );
	}
}
