using Godot;

namespace vcrossing.WorldBuilder;

public partial class Building : Node3D
{
	[Export] public Area3D PlacementBlocker { get; set; }

	public override void _Ready()
	{
		var world = GetNode<World>( "/root/Main/WorldContainer/World" );
		world.AddPlacementBlocker( PlacementBlocker );
	}
}
