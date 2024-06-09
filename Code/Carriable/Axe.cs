using System;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Carriable;

public partial class Axe : BaseCarriable
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

		var floorItem = worldItems.FirstOrDefault( x => x.GridPlacement == World.ItemPlacement.Floor );
		if ( floorItem != null )
		{
			if ( floorItem.Node is Items.Tree tree )
			{
				ChopTree( pos, floorItem, tree );
				return;
			}
			else
			{
				HitItem( pos, floorItem );
				return;
			}
		}


	}

	private void HitItem( Vector2I pos, WorldNodeLink floorItem )
	{
		Logger.Info( "Hitting item." );
	}

	private async void ChopTree( Vector2I pos, WorldNodeLink nodeLink, Items.Tree tree )
	{
		// Logger.Info( "Chopping tree." );
		GetNode<AudioStreamPlayer3D>( "TreeHit" ).Play();
		await tree.DropFruit();
		// nodeLink.Remove();

		var tween = GetTree().CreateTween();
		var treePositionTween = tween.Parallel().TweenProperty( tree, "global_position", tree.GlobalPosition + Vector3.Down * 5f, 2f );
		var treeOpacityTween = tween.Parallel().TweenProperty( tree, "modulate:a", 0, 2f );

		tree.GetNode<AudioStreamPlayer3D>( "Fall" ).Play();

		await ToSignal( tween, Tween.SignalName.Finished );

		nodeLink.Remove();

		var stump = Loader.LoadResource<ItemData>( "res://items/trees/stump/tree_stump.tres" );
		var stumpNode = Inventory.World.SpawnNode( stump, pos, World.ItemRotation.North, World.ItemPlacement.Floor );
	}
}
