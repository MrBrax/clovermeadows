using Godot;
using vcrossing.Code.Helpers;

namespace vcrossing.Code.WorldBuilder;

public partial class GridBlocker : Node3D
{

	public override void _Ready()
	{
		Logger.Debug( "GridBlocker ready." );
		var world = GetNode<WorldManager>( "/root/Main/WorldManager" ).ActiveWorld;
		// world.AddPlacementBlocker( PlacementBlocker );
		foreach ( var child in GetChildren() )
		{
			if ( child is Area3D area )
			{
				world.AddPlacementBlocker( area );
			}
		}
	}

}
