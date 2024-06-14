using System;

namespace vcrossing.Code.WorldBuilder.Weather;

public partial class WeatherBase : Node3D
{

	protected WeatherManager WeatherManager => GetParent<WeatherManager>();

	protected bool _enabled = false;

	[Export] public DirectionalLight3D SunLight { get; set; }

	/* public virtual void SetEnabled( bool state )
	{
		_enabled = state;
		Visible = state;
		foreach ( var child in GetChildren() )
		{
			if ( child is GpuParticles3D node )
			{
				node.Emitting = state;
				Logger.Info( "WeatherBase", $"Set {node.Name} emitting to {state}" );
			}
			else if ( child is AudioStreamPlayer3D player )
			{
				player.Playing = state;
				Logger.Info( "WeatherBase", $"Set {player.Name} playing to {state}" );
			}
			else
			{
				// Logger.Warn( "WeatherBase", $"Unknown child type {child.GetType()}" );
			}
		}
	} */
}
