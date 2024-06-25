using System;
using System.Text.Json.Serialization;

namespace vcrossing.Code.Persistence;

public class Tree : PersistentItem
{
	[JsonInclude] public DateTime LastFruitDrop { get; set; } = DateTime.UnixEpoch;

	public override void GetNodeData( Node3D node )
	{
		base.GetNodeData( node );
		if ( node is Items.Tree tree )
		{
			LastFruitDrop = tree.LastFruitDrop;
			// GrowProgress = plant.GrowProgress;
		}
	}

	public override void SetNodeData( Node3D node )
	{
		base.SetNodeData( node );
		if ( node is Items.Tree tree )
		{
			tree.LastFruitDrop = LastFruitDrop;
			// plant.GrowProgress = GrowProgress;
		}
	}

}
