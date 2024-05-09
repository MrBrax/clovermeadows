using Godot;

namespace vcrossing.WorldBuilder;

public partial class Building : Node3D
{
	[Export] public Area3D PlacementBlocker { get; set; }

	public override void _Ready()
	{
		var world = GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;
		world.AddPlacementBlocker( PlacementBlocker );
	}
}
