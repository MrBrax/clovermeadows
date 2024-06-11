using System.Text.Json.Serialization;

namespace vcrossing.Code.Persistence;

public partial class Plant : PersistentItem
{

    [JsonInclude] public System.DateTime LastWatered { get; set; }

    [JsonInclude] public float GrowProgress { get; set; }


    public override void GetNodeData( Node3D node )
    {
        base.GetNodeData( node );
        if ( node is Items.Plant plant )
        {
            LastWatered = plant.LastWatered;
            GrowProgress = plant.GrowProgress;
        }
    }

    public override void SetNodeData( Node3D node )
    {
        base.SetNodeData( node );
        if ( node is Items.Plant plant )
        {
            plant.LastWatered = LastWatered;
            plant.GrowProgress = GrowProgress;
        }
    }

}