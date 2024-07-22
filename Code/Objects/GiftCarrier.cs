using System;
using vcrossing.Code.Data;

namespace vcrossing.Code.Objects;

public partial class GiftCarrier : Node3D, IShootable
{

	[Export] public float Speed = 5f;

	[Export] public PackedScene GiftModel;
	[Export] public Node3D GiftModelSpawn;

	public void OnShot( Node3D pellet )
	{
		Logger.Info( "GiftCarrier", "Shot" );

		var world = NodeManager.WorldManager.ActiveWorld;

		var endPosGrid = world.WorldToItemGrid( GlobalPosition );
		var endPosWorld = world.ItemGridToWorld( endPosGrid );

		var giftModel = GiftModel.Instantiate<Node3D>();
		GetTree().CurrentScene.AddChild( giftModel );
		giftModel.GlobalPosition = GiftModelSpawn.GlobalPosition;
		giftModel.RotationDegrees = GiftModelSpawn.RotationDegrees;

		var tween = GetTree().CreateTween();
		var p = tween.TweenProperty( giftModel, "global_position", endPosWorld, 2f );
		p.SetTrans( Tween.TransitionType.Bounce );
		p.SetEase( Tween.EaseType.Out );

		tween.TweenCallback( Callable.From( () =>
		{
			giftModel.QueueFree();
			SpawnGift( endPosWorld );
		} ) );

		QueueFree();
	}

	public void SpawnGift( Vector3 position )
	{

		var world = NodeManager.WorldManager.ActiveWorld;

		var gridPos = world.WorldToItemGrid( position );

		var gift = ResourceManager.Instance.LoadItemFromId<ItemData>( "gift" );

		world.SpawnNode( gift, gridPos, World.ItemRotation.North, World.ItemPlacement.Floor, false );

	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		GlobalPosition += -Transform.Basis.Z * Speed * (float)delta;

	}
}
