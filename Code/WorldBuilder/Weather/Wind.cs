using System;

namespace vcrossing.Code.WorldBuilder.Weather;

public partial class Wind : WeatherBase
{

    public void SetEnabled( bool state )
    {
        _enabled = state;

        var player = GetNode<AudioStreamPlayer3D>( "AudioStreamPlayer3D" );
        player.Playing = state;
        player.Bus = WeatherManager.IsInside ? "AmbienceOutside" : "Ambience";
    }

}
