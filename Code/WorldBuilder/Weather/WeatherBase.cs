using System;

namespace vcrossing.Code.WorldBuilder.Weather;

public partial class WeatherBase : Node3D
{

	protected bool _enabled = false;

	[Export] public DirectionalLight3D SunLight { get; set; }

	public virtual void SetEnabled( bool state )
	{
		_enabled = state;
		foreach ( var child in GetChildren() )
		{
			if ( child is GpuParticles3D node )
			{
				node.Emitting = state;
			}
			else if ( child is AudioStreamPlayer3D player )
			{
				player.Playing = state;
			}
		}
	}
}
