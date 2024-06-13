using System;
using vcrossing.Code.Player;

namespace vcrossing.Code.WorldBuilder;

public partial class WeatherManager : Node3D
{

    [Export] public bool IsInside { get; set; } = false;


    private void Setup()
    {
        // remove all weather nodes
        foreach ( var child in GetChildren() )
        {
            child.QueueFree();
        }

    }

    public override void _Process( double delta )
    {
        base._Process( delta );

        var player = GetNode<PlayerController>( "/root/Main/Player" );
        if ( player == null )
        {
            return;
        }

        GlobalPosition = player.GlobalPosition;
    }

}
