using System;
using vcrossing.Code.Data;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Objects;

public partial class GiftCarrier : Node3D, IShootable
{

	[Export] public float Speed = 5f;

	[Export] public Node3D GiftVisual;
	[Export] public PackedScene GiftModel;
	[Export] public Node3D GiftModelSpawn;

	[Export] public AnimationPlayer AnimationPlayer;

	public List<PersistentItem> Items { get; set; } = new();

	private bool _hasDroppedGift;

	public override void _Ready()
	{
		base._Ready();

		if ( Items.Count == 0 )
		{
			Items.Add( ResourceManager.Instance.LoadItemFromId<ItemData>( "shovel" ).CreateItem() );
		}

		AnimationPlayer.Play( "stork_armatureAction" );
	}

	// public float LookAtWhenShotTimeout => 2f;
	// public Node3D LookAtWhenShotTarget { get; set; }
	// public bool LookAtWhenShot => true;

	public void OnShot( Node3D pellet )
	{

		if ( _hasDroppedGift ) return;

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

		// LookAtWhenShotTarget = giftModel;

		// QueueFree();
		GiftVisual.Hide();
		_hasDroppedGift = true;
		Speed *= 2f;
		AnimationPlayer.SpeedScale = 2f;
	}

	public void SpawnGift( Vector3 position )
	{

		var world = NodeManager.WorldManager.ActiveWorld;

		var gridPos = world.WorldToItemGrid( position );

		var gift = ResourceManager.Instance.LoadItemFromId<ItemData>( "gift" );

		var nodeLink = world.SpawnNode( gift, gridPos, World.ItemRotation.North, World.ItemPlacement.Floor, false );

		var giftNode = nodeLink.Node as Items.Gift;

		foreach ( var item in Items )
		{
			giftNode.AddItem( item );
		}

	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		GlobalPosition += -Transform.Basis.Z * Speed * (float)delta;

		var x = GlobalPosition.X;
		var z = GlobalPosition.Z;

		// out of bounds, destroy
		if ( x < -45 || x > 170 || z < -45 || z > 170 )
		{
			Logger.Info( "GiftCarrier", "Out of bounds" );
			QueueFree();
		}

	}

	public static void SpawnRandom()
	{

		Logger.Info( "GiftCarrier", "Spawning random gift carrier" );

		var world = NodeManager.WorldManager.ActiveWorld;

		if ( world == null )
		{
			Logger.LogError( "GiftCarrier", "No active world" );
			return;
		}

		var giftCarrier = Loader.LoadResource<PackedScene>( "res://objects/giftcarrier/giftcarrier.tscn" ).Instantiate<GiftCarrier>();
		world.AddChild( giftCarrier );

		var height = 11f;

		var westOrEast = GD.Randi() % 2 == 0 ? -40 : 160;
		var NorthOrSouth = GD.Randi() % 2 == 0 ? -40 : 160;

		giftCarrier.GlobalPosition = new Vector3( westOrEast, height, NorthOrSouth );

		Logger.Info( "GiftCarrier", $"Spawned at {giftCarrier.GlobalPosition}" );

		var midpoint = new Vector3( 64, height, 64 );

		// face the midpoint
		giftCarrier.LookAt( midpoint, Vector3.Up );
		// giftCarrier.RotateObjectLocal( Vector3.Up, Mathf.Pi );

		Logger.Info( "GiftCarrier", $"Facing {midpoint} ({giftCarrier.RotationDegrees}) ({giftCarrier.Transform.Basis.Z}) ({giftCarrier.RotationDegrees.Y})" );

		giftCarrier.Items = GenerateRandomItems();

	}

	private static List<PersistentItem> GenerateRandomItems()
	{
		var category = Loader.LoadResource<ItemCategoryData>( "res://collections/gifts.tres" );
		var itemData = category.Items.PickRandom();
		return [itemData.CreateItem()]; // TODO: give maybe multiple items?
	}

}
