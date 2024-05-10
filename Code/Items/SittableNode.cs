using Godot;

namespace vcrossing2.Code.Items;

[GlobalClass]
public partial class SittableNode : Node3D
{
	
	public Node3D Occupant;
	
	public bool IsOccupied => Occupant != null;
	
	
}
