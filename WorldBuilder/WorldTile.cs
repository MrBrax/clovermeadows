using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace vcrossing.WorldBuilder;

public partial class WorldTile : Node3D
{
	
	[Export] public Node[] GridBlockers { get; set; }

}
