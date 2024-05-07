using Godot;

namespace vcrossing;

public class WorldItem : Node3D
{
	
	[Export] public string Name { get; set; }
	[Export] public Vector2I GridPosition { get; set; }
	[Export] public ItemData ItemData { get; set; }
	[Export] public NodePath Model { get; set; }
	[Export] public World.ItemPlacement Placement { get; set; } = World.ItemPlacement.Floor;
	
}
