using System.Linq;
using Godot;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Items;
using vcrossing2.Code.Persistence;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Carriable;

public partial class FishingRod : BaseCarriable
{
	public override void OnEquip( PlayerController player )
	{
		Logger.Info( "Equipped shovel." );
	}

	public override void OnUnequip( PlayerController player )
	{
		Logger.Info( "Unequipped shovel." );
	}

	public override void OnUse( PlayerController player )
	{
		if ( !CanUse() )
		{
			return;
		}

		Logger.Info( "FishingRod", "Using fishing rod." );

	}

}
