using System;
using System.Text.Json.Serialization;

namespace vcrossing.Code.Persistence;

public partial class Gift : PersistentItem
{

	[JsonInclude] public IList<PersistentItem> Items { get; private set; } = [];

	public override void GetNodeData( Node3D node )
	{
		base.GetNodeData( node );
		if ( node is Items.Gift gift )
		{
			Items = gift.Items;
		}
	}

	public override void SetNodeData( Node3D node )
	{
		base.SetNodeData( node );
		if ( node is Items.Gift gift )
		{
			gift.Items = Items;
		}
	}

	public void AddItem( PersistentItem item )
	{
		if ( item is Gift ) throw new Exception( "Cannot add a Gift to a Gift. May cause infinite recursion." );
		Items.Add( item );
	}

}
