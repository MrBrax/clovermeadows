using Godot;

namespace vcrossing.World;

public class WorldTile : Node3D
{
	
	[Export]
	public bool[,] BlockedTiles { get; set; } = new bool[4, 4];
	
}
