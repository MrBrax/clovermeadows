using System;

namespace vcrossing.Code.WorldBuilder.Weather;

public partial class Rain : WeatherBase
{

	private int _level = 0;

	private Dictionary<int, int> _levelParticleAmount = new()
	{
		{ 0, 0 },
		{ 1, 200 },
		{ 2, 600 },
		{ 3, 1500 }
	};

	public override void _Ready()
	{
		base._Ready();

		var raindrops = GetNodeOrNull<GpuParticles3D>( "Raindrops" );
		if ( raindrops != null )
		{
			raindrops.Emitting = false;
		}

		var rainsound = GetNode<AudioStreamPlayer3D>( "Rain" );
		if ( rainsound != null )
		{
			rainsound.Playing = false;
		}
	}

	public void SetWind( float angle, float speed )
	{
		var raindrops = GetNodeOrNull<GpuParticles3D>( "Raindrops" );
		if ( raindrops != null )
		{
			if ( raindrops.ProcessMaterial is ParticleProcessMaterial processMaterial )
			{
				processMaterial.Direction = new Vector3( Mathf.Cos( angle ) * speed, -10, Mathf.Sin( angle ) * speed );
				Logger.Info( "Rain", $"Set rain direction to {angle}" );
			}
		}
	}

	public void SetLevel( int level )
	{
		if ( _level == level ) return;
		Logger.Info( "Rain", $"SetLevel {level} ({Name})" );
		var raindrops = GetNodeOrNull<GpuParticles3D>( "Raindrops" );
		if ( raindrops != null )
		{
			// enable rain
			if ( _level == 0 && level > 0 )
			{
				raindrops.Emitting = true;

				raindrops.Amount = _levelParticleAmount[level];

				raindrops.AmountRatio = 1f;
			}
			else if ( _level > 0 && level == 0 )
			{
				// clear raindrops
				raindrops.Amount = 1;

				// disable rain
				raindrops.Emitting = false;

			}
			else
			{
				// change rain level
				raindrops.Amount = _levelParticleAmount[level];
			}
		}

		var rainsound = GetNode<AudioStreamPlayer3D>( "Rain" );
		if ( rainsound != null )
		{
			Logger.Info( "Rain", $"Playing rain sound" );
			rainsound.Playing = level > 0;
		}

		_level = level;

	}

	public void SetLevelSmooth( int level )
	{
		if ( _level == level ) return;
		Logger.Info( "Rain", $"SetLevel {level} ({Name})" );
		var raindrops = GetNodeOrNull<GpuParticles3D>( "Raindrops" );
		if ( raindrops != null )
		{
			// enable rain
			if ( _level == 0 && level > 0 )
			{
				raindrops.Emitting = true;

				raindrops.Amount = _levelParticleAmount[level];

				raindrops.AmountRatio = 0f;

				var tween = GetTree().CreateTween();
				tween.TweenProperty( raindrops, "amount_ratio", 1f, _fadeTime );

			}
			else if ( _level > 0 && level == 0 )
			{
				// disable rain

				var tween = GetTree().CreateTween();
				tween.TweenProperty( raindrops, "amount_ratio", 0f, _fadeTime );
				tween.TweenCallback( Callable.From( () =>
				{
					raindrops.Emitting = false;
				} ) );
			}
			else
			{
				// change rain level

				// TODO: can't tween amount without restarting the particles
			}

			/*  int wantedAmountRatio = 0;
             switch ( level )
             {
                 case 0:
                     // raindrops.Visible = false;
                     // raindrops.Emitting = false;
                     break;
                 case 1:
                     // raindrops.Visible = true;
                     // raindrops.Emitting = true;
                     wantedAmountRatio = 1;
                     raindrops.SpeedScale = 1f;
                     break;
                 case 2:
                     // raindrops.Visible = true;
                     // raindrops.Emitting = true;
                     wantedAmountRatio = 2;
                     raindrops.SpeedScale = 1.2f;
                     break;
                 case 3:
                     // raindrops.Visible = true;
                     // raindrops.Emitting = true;
                     wantedAmountRatio = 3;
                     raindrops.SpeedScale = 1.5f;
                     break;
                 default:
                     throw new System.Exception( $"Invalid rain level {level}" );
             }

             raindrops.Emitting = true;

             var tween = GetTree().CreateTween();
             tween.TweenProperty( raindrops, "amount", wantedAmountRatio, 10f );
             tween.TweenCallback( Callable.From( () =>
             {
                 Logger.Info( "Rain", $"Raindrops tween finished, amount: {raindrops.Amount}" );
                 raindrops.Emitting = level > 0;
             } ) ); */

		}

		var rainsound = GetNode<AudioStreamPlayer3D>( "Rain" );
		if ( rainsound != null )
		{
			Logger.Info( "Rain", $"Playing rain sound" );
			rainsound.Playing = true;
			if ( _level == 0 )
			{
				Logger.Info( "Rain", $"Setting rain sound volume to mute" );
				rainsound.VolumeDb = -10f;
			}

			var tween = GetTree().CreateTween();
			tween.TweenProperty( rainsound, "volume_db", level > 0 ? 5f : -10f, _fadeTime );
			tween.TweenCallback( Callable.From( () =>
			{
				Logger.Info( "Rain", $"Rain sound tween finished, volume: {rainsound.VolumeDb}" );
				rainsound.Playing = level > 0;
			} ) );
		}

		_level = level;

	}
}
