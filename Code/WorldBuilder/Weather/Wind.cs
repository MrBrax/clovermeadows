using System;

namespace vcrossing.Code.WorldBuilder.Weather;

public partial class Wind : WeatherBase
{

    public void SetEnabled( bool state )
    {
        if ( _enabled == state ) return;

        var player = GetNode<AudioStreamPlayer3D>( "AudioStreamPlayer3D" );
        player.Bus = WeatherManager.IsInside ? "AmbienceOutside" : "Ambience";

        player.Playing = state;

        Logger.Info( "Wind", $"SetEnabled {state}" );

        _enabled = state;
    }

    public void SetEnabledSmooth( bool state )
    {
        if ( _enabled == state ) return;

        var player = GetNode<AudioStreamPlayer3D>( "AudioStreamPlayer3D" );
        player.Bus = WeatherManager.IsInside ? "AmbienceOutside" : "Ambience";

        player.Playing = state;

        if ( state ) player.VolumeDb = -10.0f;

        Logger.Info( "Wind", $"SetEnabled {state}" );

        var tween = GetTree().CreateTween();
        tween.TweenProperty( player, "volume_db", state ? 0f : -10.0f, _fadeTime );
        tween.TweenCallback( Callable.From( () =>
        {
            if ( !state )
            {
                Logger.Info( "Wind", "Stop" );
                player.Stop();
            }
        } ) );


        _enabled = state;
    }

}
