using System.Text.Json.Serialization;

namespace vcrossing.Code.Persistence;

public partial class WorldItem : PersistentItem
{

    [JsonInclude] public System.DateTime Placed { get; set; }

    public override void GetNodeData( Node3D node )
    {
        base.GetNodeData( node );
        if ( node is Items.WorldItem worldItem )
        {
            Placed = worldItem.Placed;
        }
    }

    public override void SetNodeData( Node3D node )
    {
        base.SetNodeData( node );
        if ( node is Items.WorldItem worldItem )
        {
            worldItem.Placed = Placed;
        }
    }

}
