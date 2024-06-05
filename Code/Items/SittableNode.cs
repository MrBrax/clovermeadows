using Godot;

namespace vcrossing.Code.Items;

[GlobalClass]
public partial class SittableNode : Node3D
{
	
	public Node3D Occupant;
	
	public bool IsOccupied => Occupant != null;
	
	
}
