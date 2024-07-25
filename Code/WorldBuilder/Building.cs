using Godot;
using vcrossing.Code.Helpers;

namespace vcrossing.Code.WorldBuilder;

public partial class Building : Node3D
{
	[Export] public Area3D PlacementBlocker { get; set; }

	public override void _Ready()
	{
		Logger.Debug( "Building ready." );
		var world = NodeManager.WorldManager.ActiveWorld;
		world.AddPlacementBlocker( PlacementBlocker );
	}
}
