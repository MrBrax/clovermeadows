using System;
using System.Text.Json.Serialization;

namespace vcrossing.Code.Persistence;

public partial class Plant : PersistentItem
{

	[JsonInclude] public System.DateTime LastWatered { get; set; }

	[JsonInclude] public DateTime LastProcess;
	[JsonInclude] public float Growth { get; set; } = 0f;
	[JsonInclude] public float Wilt { get; set; } = 0f;
	[JsonInclude] public float Water { get; set; } = 0f;


	public override void GetNodeData( Node3D node )
	{
		base.GetNodeData( node );
		if ( node is Items.Plant plant )
		{
			LastWatered = plant.LastWatered;
			LastProcess = plant.LastProcess;
			Growth = plant.Growth;
			Wilt = plant.Wilt;
			Water = plant.Water;
		}
	}

	public override void SetNodeData( Node3D node )
	{
		base.SetNodeData( node );
		if ( node is Items.Plant plant )
		{
			plant.LastWatered = LastWatered;
			plant.LastProcess = LastProcess;
			plant.Growth = Growth;
			plant.Wilt = Wilt;
			plant.Water = Water;
		}
	}

}
