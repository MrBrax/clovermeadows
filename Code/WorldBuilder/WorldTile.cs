using Godot;

namespace vcrossing.Code.WorldBuilder;

public partial class WorldTile : Node3D
{
	
	[Export] public Node[] GridBlockers { get; set; }

}
