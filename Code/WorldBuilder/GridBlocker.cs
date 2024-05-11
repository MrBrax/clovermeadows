using Godot;

namespace vcrossing2.Code.WorldBuilder;

public partial class GridBlocker : Node3D
{
	
	public override void _Ready()
	{
		GD.Print( "GridBlocker ready." );
		var world = GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;
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
