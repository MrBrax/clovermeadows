using System.Linq;
using Godot;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Carriable;

public partial class Shovel : BaseCarriable
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

		base.OnUse( player );
		_timeUntilUse = UseTime;

		var pos = player.Interact.GetAimingGridPosition();

		var worldItems = player.World.GetItems( pos ).ToList();

		if ( worldItems.Count == 0 )
		{
			DigHole( pos );
			return;
		}
		else
		{
			var undergroundItem = worldItems.FirstOrDefault( x => x.GridPlacement == World.ItemPlacement.Underground );
			if ( undergroundItem != null )
			{
				DigUpItem( pos, undergroundItem );
				return;
			}

			var floorItem = worldItems.FirstOrDefault( x => x.GridPlacement == World.ItemPlacement.Floor );
			if ( floorItem != null )
			{
				if ( floorItem.Node is Hole hole )
				{
					FillHole( pos );
					return;
				}
				else
				{
					HitItem( pos, floorItem );
					return;
				}
			}
		}

		GD.PushWarning( "No action taken." );
	}

	private void HitItem( Vector2I pos, WorldNodeLink floorItem )
	{
		Logger.Info( $"Hit {floorItem.GetItemData().Name} at {pos}" );
		GetNode<AudioStreamPlayer3D>( "HitSound" ).Play();
	}

	private void SnapPlayerToGrid()
	{
		// var player = Inventory.Player;
		// player.GlobalPosition = World.ItemGridToWorld( World.WorldToItemGrid( player.GlobalPosition ) );
		// player.Model.Quaternion = World.GetRotation( World.Get8Direction( player.Model.RotationDegrees.Y ) );
	}

	private void DigHole( Vector2I pos )
	{
		Logger.Info( $"Dug hole at {pos}" );

		SnapPlayerToGrid();

		var holeData = GD.Load<ItemData>( "res://items/misc/hole/hole.tres" );
		/*var hole = Inventory.World.SpawnPlacedItem<Hole>( holeData, pos, World.ItemPlacement.Floor,
			World.RandomItemRotation() );*/
		var hole = Inventory.World.SpawnNode( holeData, pos, World.RandomItemRotation(), World.ItemPlacement.Floor, false );

		GetNode<AudioStreamPlayer3D>( "DigSound" ).Play();

		Durability--;
		Inventory.Player.Save();

		Inventory.World.Save();
	}

	private void FillHole( Vector2I pos )
	{
		Logger.Info( $"Filled hole at {pos}" );

		var hole = Inventory.World.GetItem( pos, World.ItemPlacement.Floor );
		if ( hole == null )
		{
			Logger.Info( "No hole found." );
			return;
		}

		if ( hole.Node is Hole holeItem )
		{
			Inventory.World.RemoveItem( holeItem );
			Inventory.World.Save();

			SnapPlayerToGrid();

			Durability--;
			Inventory.Player.Save();

			GetNode<AudioStreamPlayer3D>( "FillSound" ).Play();
		}
		else
		{
			GD.PushWarning( "Not a hole." );
		}

		// TODO: check if hole has item in it
	}

	private void DigUpItem( Vector2I pos, WorldNodeLink item )
	{
		Logger.Info( $"Dug up {item.GetItemData().Name} at {pos}" );
		// Inventory.World.RemoveItem( item );

		// TODO: check if there are items in ground
	}
}
