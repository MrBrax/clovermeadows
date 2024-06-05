using Godot;

namespace vcrossing.Code.Items;

public partial class LyingNode : Node3D
{
	
	public Node3D Occupant;
	
	public bool IsOccupied => Occupant != null;
	
}
