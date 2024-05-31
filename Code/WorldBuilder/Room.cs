namespace vcrossing2.Code.WorldBuilder;

[GlobalClass]
public partial class Room : Resource
{
	[Export] public NodePath Wall { get; set; }
	[Export] public NodePath Floor { get; set; }

	public MeshInstance3D GetWall( Node3D root )
	{
		return root.GetNode<MeshInstance3D>( Wall );
	}

	public MeshInstance3D GetFloor( Node3D root )
	{
		return root.GetNode<MeshInstance3D>( Floor );
	}
}
