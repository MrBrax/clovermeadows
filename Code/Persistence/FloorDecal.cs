using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Persistence;

public class FloorDecal : PersistentItem
{

    [JsonInclude] public string TexturePath { get; set; }

    public override void GetNodeData( Node3D node )
    {
        base.GetNodeData( node );
        if ( node is Code.Items.FloorDecal decal )
        {
            TexturePath = decal.TexturePath;
        }
    }

    public override void SetNodeData( Node3D node )
    {
        base.SetNodeData( node );
        if ( node is Code.Items.FloorDecal decal )
        {
            decal.TexturePath = TexturePath;
            decal.UpdateDecal();
        }
    }

}
