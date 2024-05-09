using Godot;

namespace vcrossing.items;

public partial class SittableNode : Node3D
{
	
	public Node3D Occupant;
	
	public bool IsOccupied => Occupant != null;
	
	
}
