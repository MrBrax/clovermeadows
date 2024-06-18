using vcrossing.Code.Data;
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
				if ( tree.IsFalling || tree.IsDroppingFruit ) return;
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

		tree.Stump.Show();

		var model = tree.GetNode<Node3D>( tree.Model );

		var fallenRotation = new Vector3( 0, 0, 90 );

		var tween = GetTree().CreateTween();
		var treePositionTween = tween.TweenProperty( model, "rotation_degrees", fallenRotation, 1f );
		treePositionTween.SetTrans( Tween.TransitionType.Expo );
		treePositionTween.SetEase( Tween.EaseType.In );

		// var treeSizeTween = tween.Parallel().TweenProperty( model, "scale", Vector3.Zero, 0.1f ).SetDelay( 0.9f );
		// var treeOpacityTween = tween.Parallel().TweenProperty( model, "modulate:a", 0, 2f );

		tree.GetNode<AudioStreamPlayer3D>( "Fall" ).Play();

		await ToSignal( tween, Tween.SignalName.Finished );

		tree.GetNode<AudioStreamPlayer3D>( "FallGround" ).Play();

		var particle = Loader.LoadResource<PackedScene>( "res://particles/poof.tscn" ).Instantiate<Node3D>();
		GetTree().Root.AddChild( particle );
		particle.GlobalPosition = tree.GlobalPosition + Vector3.Left * 1f;

		model.Hide();

		await ToSignal( GetTree().CreateTimer( 1f ), Timer.SignalName.Timeout );

		nodeLink.Remove();

		var stump = Loader.LoadResource<ItemData>( "res://items/trees/stump/tree_stump.tres" );
		var stumpNode = World.SpawnNode( stump, pos, World.ItemRotation.North, World.ItemPlacement.Floor );
	}
}
