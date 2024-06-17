using System;

namespace vcrossing.Code.WorldBuilder.Weather;

public partial class Fog : WeatherBase
{

	public void SetEnabledSmooth( bool state )
	{
		Logger.Info( "Fog", $"SetEnabled {state}" );
		_enabled = state;
		var fog = GetNodeOrNull<GpuParticles3D>( "Fog" );
		if ( fog == null )
		{
			throw new System.Exception( "Fog particles not found" );
		}

		if ( !fog.Emitting && state )
		{
			fog.Amount = 20;
		}

		fog.Emitting = state && !WeatherManager.IsInside;

		var worldEnvironment = GetTree().GetNodesInGroup( "worldenvironment" );
		if ( worldEnvironment.Count == 0 )
		{
			Logger.LogError( "Fog", "WorldEnvironment not found" );
			return;
		}

		var environment = worldEnvironment.FirstOrDefault() as WorldEnvironment;
		if ( environment == null )
		{
			Logger.LogError( "Fog", "WorldEnvironment not found" );
			return;
		}

		var timeManager = GetNodeOrNull<TimeManager>( "/root/Main/TimeManager" );

		var fogDensity = timeManager.IsNight ? 0.005f : 0.02f;

		if ( WeatherManager.IsInside ) fogDensity = 0.0f;

		//  environment.Environment.FogDensity = state ? 0.02f : 0.0f;
		var tween = GetTree().CreateTween();
		tween.TweenProperty( environment.Environment, "fog_density", state ? fogDensity : 0.0f, _fadeTime );


		var nightColor = new Color( 0.2f, 0.2f, 0.2f );
		var dayColor = new Color( 0.8f, 0.8f, 0.8f );

		// TODO: fix fog curve
		// environment.Environment.FogLightColor = dayColor.Lerp( nightColor, Mathf.Cos( timeManager.Time.Hour * Mathf.Pi / 24 ) );

	}

	public void SetEnabled( bool state )
	{
		Logger.Info( "Fog", $"SetEnabled {state}" );
		_enabled = state;
		var fog = GetNodeOrNull<GpuParticles3D>( "Fog" );
		if ( fog == null )
		{
			throw new System.Exception( "Fog particles not found" );
		}
		fog.Emitting = state && !WeatherManager.IsInside;

		if ( !state )
		{
			fog.Amount = 1;
		}
		else
		{
			fog.Amount = 20;
		}

		var worldEnvironment = GetTree().GetNodesInGroup( "worldenvironment" );
		if ( worldEnvironment.Count == 0 )
		{
			Logger.LogError( "Fog", "WorldEnvironment not found" );
			return;
		}

		var environment = worldEnvironment.FirstOrDefault() as WorldEnvironment;
		if ( environment == null )
		{
			Logger.LogError( "Fog", "WorldEnvironment not found" );
			return;
		}

		var timeManager = GetNodeOrNull<TimeManager>( "/root/Main/TimeManager" );

		var fogDensity = timeManager.IsNight ? 0.005f : 0.02f;

		if ( WeatherManager.IsInside ) fogDensity = 0.0f;

		//  environment.Environment.FogDensity = state ? 0.02f : 0.0f;
		environment.Environment.FogDensity = state ? fogDensity : 0.0f;

		var nightColor = new Color( 0.2f, 0.2f, 0.2f );
		var dayColor = new Color( 0.8f, 0.8f, 0.8f );

		// TODO: fix fog curve
		// environment.Environment.FogLightColor = dayColor.Lerp( nightColor, Mathf.Cos( timeManager.Time.Hour * Mathf.Pi / 24 ) );

	}

}
