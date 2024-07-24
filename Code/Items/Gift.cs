using System;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Items;

public partial class Gift : WorldItem
{

	public IList<PersistentItem> Items { get; set; } = [];

	public void AddItem( PersistentItem item )
	{
		if ( item is Persistence.Gift ) throw new Exception( "Cannot add a Gift to a Gift. May cause infinite recursion." );
		Items.Add( item );
	}

}
