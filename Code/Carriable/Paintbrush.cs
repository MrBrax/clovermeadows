using System;
using vcrossing.Code.Data;
using vcrossing.Code.Inventory;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Carriable;

public sealed partial class Paintbrush : BaseCarriable
{

	public string CurrentTexturePath { get; set; }

	public override void OnUse( PlayerController player )
	{
		base.OnUse( player );

		if ( string.IsNullOrWhiteSpace( CurrentTexturePath ) )
		{
			throw new System.Exception( "CurrentTexturePath is null or empty" );
		}

		var pos = player.Interact.GetAimingGridPosition();

		var item = World.GetItem( pos, World.ItemPlacement.FloorDecal );

		if ( item != null && item.Node is Items.FloorDecal decal )
		{
			decal.TexturePath = CurrentTexturePath;
			decal.UpdateDecal();
		}
		else
		{

			var playerRotation = World.GetItemRotationFromDirection(
				World.Get4Direction( player.Model.RotationDegrees.Y ) );

			var newItem = PersistentItem.Create( ResourceManager.Instance.LoadItemFromId<ItemData>( "floor_decal" ) ) as Persistence.FloorDecal;
			if ( newItem == null ) throw new System.Exception( "Failed to create floor decal" );

			newItem.TexturePath = CurrentTexturePath;

			World.SpawnPersistentNode( newItem, pos, playerRotation, World.ItemPlacement.FloorDecal, false );
		}
	}

}
