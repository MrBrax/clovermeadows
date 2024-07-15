using Godot;

namespace vcrossing.Code.Items;

public sealed partial class LyingNode : Node3D
{

	public Node3D Occupant;

	public bool IsOccupied => Occupant != null;

}
