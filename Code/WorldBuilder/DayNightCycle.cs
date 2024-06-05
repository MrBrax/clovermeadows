using System;
using Godot;
using vcrossing.Code.Helpers;

namespace vcrossing.Code.WorldBuilder;

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
		var sunAngleDegrees = (Time.GetTicksMsec() * Speed) % 360;
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
}
