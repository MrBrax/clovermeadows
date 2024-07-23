using System;
using vcrossing.Code.Carriable;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Npc.Fauna;

public partial class BaseFloorFauna : BaseFauna, INettable, IPersistence, IShootable
{
	// public Type PersistentType => typeof( Animal );

	[Export] public string PersistentItemType { get; set; } = nameof( Animal );

	public System.Collections.Generic.Dictionary<string, object> GetNodeData()
	{
		return default;
	}

	public void SetNodeData( PersistentItem item, System.Collections.Generic.Dictionary<string, object> data )
	{

	}

	public void OnNetted( Net net )
	{
		var owner = net.Player;

		if ( owner == null )
		{
			Logger.LogError( "Netted without owner" );
			return;
		}

		var item = PersistentItem.Create( this );

		owner.Inventory.PickUpItem( item );

		Logger.Info( $"Netted {Name}" );

		QueueFree();

	}

	// public float LookAtWhenShotTimeout => 1f;
	// public Node3D LookAtWhenShotTarget { get; set; }
	// public bool LookAtWhenShot => false;

	public void OnShot( Node3D pellet )
	{
		// QueueFree();
		Disappear();
	}
}
