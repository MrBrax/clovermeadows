using Godot;

namespace vcrossing2.items;

public partial class LyingNode : Node3D
{
	
	public Node3D Occupant;
	
	public bool IsOccupied => Occupant != null;
	
}
