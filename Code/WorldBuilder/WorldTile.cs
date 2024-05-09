using Godot;

namespace vcrossing2.Code.WorldBuilder;

public partial class WorldTile : Node3D
{
	
	[Export] public Node[] GridBlockers { get; set; }

}
