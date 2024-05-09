using Godot;

namespace vcrossing.WorldBuilder;

[Tool]
public partial class GridBlocker : Node3D
{
	
	public override void _Ready()
	{
		var world = GetNode<World>( "/root/Main/World" );
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
