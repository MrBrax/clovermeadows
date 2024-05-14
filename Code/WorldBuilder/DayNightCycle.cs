using System;
using Godot;
using vcrossing2.Code.Helpers;

namespace vcrossing2.Code.WorldBuilder;

public partial class DayNightCycle : Node3D
{
	[Export] public DirectionalLight3D Sun;
	[Export] public float Speed = 1;
	[Export] public float MaxSunAngle = 70f;

	public double TimeOfDay => Time.GetUnixTimeFromSystem();

	public override void _Ready()
	{
		if ( Sun == null )
		{
			throw new System.Exception( "Sun not set." );
		}
	}

	public override void _Process( double delta )
	{
		var time = TimeOfDay;
		var hours = time / 3600;
		var minutes = (time % 3600) / 60;
		var seconds = time % 60;

		/*var angle = (float)((hours % 24) / 24 * Mathf.Pi * 2) * Speed;
		angle = angle % 360;
		Logger.Info( "DayNightCycle", $"Time: {hours:00}:{minutes:00}:{seconds:00}, Angle: {angle}" );*/
		float sunAngleDegrees = 0;
		if ( hours >= 0 && hours < 6 )
		{
			sunAngleDegrees = Mathf.Lerp( 180, 90, (float)hours / 6 );
		}
		else if ( hours >= 6 && hours < 18 )
		{
			sunAngleDegrees = Mathf.Lerp( 90, -90, (float)(hours - 6) / 12 );
		}
		else if ( hours >= 18 && hours < 24 )
		{
			sunAngleDegrees = Mathf.Lerp( -90, -180, (float)(hours - 18) / 6 );
		}
		Sun.RotationDegrees = new Vector3( Mathf.Clamp( sunAngleDegrees, -MaxSunAngle, MaxSunAngle ), 45, 0 );
		
		if ( Math.Abs( Sun.RotationDegrees.X ) >= MaxSunAngle )
		{
			Sun.LightEnergy = 0;
		}
		else
		{
			Sun.LightEnergy = 1;
		}
	}
}
