using System;
using vcrossing.Code.Carriable.Actions;
using vcrossing.Code.Data;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Carriable;

public partial class WateringCan : BaseCarriable
{

	public override void OnUse( PlayerController player )
	{
		if ( !CanUse() )
		{
			return;
		}

		base.OnUse( player );
		_timeUntilUse = UseTime;

		var pos = player.Interact.GetAimingGridPosition();

		var worldItems = player.World.GetItems( pos ).ToList();

		if ( worldItems.Count == 0 )
		{
			return;
		}

		var floorItem = worldItems.FirstOrDefault( x => x.GridPlacement == World.ItemPlacement.Floor && x.Node is IWaterable );

		if ( floorItem != null )
		{
			WaterItem( pos, floorItem );
			return;
		}

	}

	private void WaterItem( Vector2I pos, WorldNodeLink floorItem )
	{
		Logger.Info( "Watering item." );
		(floorItem.Node as IWaterable)?.OnWater( this );
	}
}
