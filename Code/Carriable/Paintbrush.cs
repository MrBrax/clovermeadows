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

		var pos = player.Interact.GetAimingGridPosition();

		var item = World.GetItem( pos, World.ItemPlacement.FloorDecal );

		if ( item != null && item.Node is Items.FloorDecal decal )
		{
			if ( string.IsNullOrWhiteSpace( CurrentTexturePath ) || CurrentTexturePath == decal.TexturePath )
			{
				item.Remove();
			}
			else
			{
				decal.TexturePath = CurrentTexturePath;
				decal.UpdateDecal();
			}
		}
		else
		{

			if ( string.IsNullOrWhiteSpace( CurrentTexturePath ) )
			{
				return;
			}

			var playerRotation = World.GetItemRotationFromDirection(
				World.Get4Direction( player.Model.RotationDegrees.Y ) );

			var newItem = PersistentItem.Create( ResourceManager.Instance.LoadItemFromId<ItemData>( "floor_decal" ) ) as Persistence.FloorDecal;
			if ( newItem == null ) throw new System.Exception( "Failed to create floor decal" );

			newItem.TexturePath = CurrentTexturePath;

			var node = World.SpawnPersistentNode( newItem, pos, playerRotation, World.ItemPlacement.FloorDecal, false );

			// fade in the decal
			if ( node is Items.FloorDecal decal2 )
			{
				decal2.Decal.Modulate = new Godot.Color( 1, 1, 1, 0f );
				var tween = GetTree().CreateTween();
				tween.TweenProperty( decal2.Decal, "modulate:a", 1f, 0.1f );
			}
		}

		GetNode<AudioStreamPlayer3D>( "UseSound" )?.Play();
	}

}
