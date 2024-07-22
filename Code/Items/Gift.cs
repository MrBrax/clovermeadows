using System;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Items;

public partial class Gift : WorldItem
{

	public List<PersistentItem> Items { get; set; } = new();

	public void AddItem( PersistentItem item )
	{
		if ( item is Persistence.Gift ) throw new Exception( "Cannot add a Gift to a Gift. May cause infinite recursion." );
		Items.Add( item );
	}

}