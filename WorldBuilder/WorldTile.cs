using Godot;
using Godot.Collections;

namespace vcrossing.WorldBuilder;

public partial class WorldTile : Node3D
{

	// 4x4 grid for blocking placement
	[Export] public bool[,] Grid = new bool[4, 4];

}
