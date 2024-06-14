using System;

namespace vcrossing.Code.WorldBuilder.Weather;

public partial class Fog : WeatherBase
{

	public void SetEnabled( bool state )
	{
		Logger.Info( "Fog", $"SetEnabled {state}" );
		_enabled = state;
		var fog = GetNodeOrNull<GpuParticles3D>( "Fog" );
		if ( fog == null )
		{
			throw new System.Exception( "Fog particles not found" );
		}
		fog.Emitting = state;

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

		//  environment.Environment.FogDensity = state ? 0.02f : 0.0f;
		var tween = GetTree().CreateTween();
		tween.TweenProperty( environment.Environment, "fog_density", state ? 0.01f : 0.0f, _fadeTime );

		var timeManager = GetNodeOrNull<TimeManager>( "/root/Main/TimeManager" );

		var nightColor = new Color( 0.1f, 0.1f, 0.1f );
		var dayColor = new Color( 0.8f, 0.8f, 0.8f );

		environment.Environment.FogLightColor = dayColor.Lerp( nightColor, Mathf.Sin( timeManager.Time.Hour * Mathf.Pi / 24 ) );

	}

}
