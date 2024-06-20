using vcrossing.Code.Data;

namespace vcrossing.Code;

public partial class WorldMesh : Node3D
{

	[Export] public SurfaceData Surface;

	[Export] public Node3D MeshHidePosition;

	public override void _Process( double delta )
	{
		base._Process( delta );

		if ( MeshHidePosition == null ) return;

		var player = GetNodeOrNull<Player.PlayerController>( "/root/Main/Player" );
		if ( player == null ) return;

		if ( player.GlobalPosition.Z < MeshHidePosition.GlobalPosition.Z )
		{
			Visible = false;
		}
		else
		{
			Visible = true;
		}
	}


}
