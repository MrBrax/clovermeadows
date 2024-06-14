using System;

namespace vcrossing.Code.WorldBuilder.Weather;

public partial class Rain : WeatherBase
{

    private int _level = 0;

    private Dictionary<int, int> _levelParticleAmount = new()
    {
        { 0, 0 },
        { 1, 200 },
        { 2, 400 },
        { 3, 600 }
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

                raindrops.AmountRatio = 0f;

                var tween = GetTree().CreateTween();
                tween.TweenProperty( raindrops, "amount_ratio", 1f, 10f );

            }
            else if ( _level > 0 && level == 0 )
            {
                // disable rain

                var tween = GetTree().CreateTween();
                tween.TweenProperty( raindrops, "amount_ratio", 0f, 10f );
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
            // rainsound.Playing = level > 0;
            var tween = GetTree().CreateTween();
            tween.TweenProperty( rainsound, "volume_db", level > 0 ? 0 : -80, 5f );
            tween.TweenCallback( Callable.From( () =>
            {
                rainsound.Playing = level > 0;
            } ) );
        }

        _level = level;

    }
}
