namespace vcrossing.Code.WorldBuilder.Weather;

public partial class Rain : WeatherBase
{

    public void SetFogState( bool state )
    {
        var fog = GetNode<Node3D>( "Fog" );
        if ( fog == null )
        {
            Logger.Warn( "Fog node not found" );
            return;
        }
        fog.Visible = state;
    }


}
