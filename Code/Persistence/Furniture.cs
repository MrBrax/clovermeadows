using System.Text.Json.Serialization;

namespace vcrossing.Code.Persistence;

public partial class Furniture : PersistentItem
{

	[JsonInclude] public bool LightState { get; set; } = false;

	public override void GetNodeData( Node3D node )
	{
		base.GetNodeData( node );
		if ( node is Items.Furniture furniture )
		{
			LightState = furniture.LightState;
		}
	}

	public override void SetNodeData( Node3D node )
	{
		base.SetNodeData( node );
		if ( node is Items.Furniture furniture )
		{
			furniture.LightState = LightState;
		}
	}

}
