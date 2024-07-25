using System;
using System.Text.Json;
using System.Threading.Tasks;
using Godot.Collections;
using vcrossing.Code.Data;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public sealed partial class Tree : WorldItem, IUsable, IPersistence
{

	// public TreeData TreeData { get; private set; }

	[Export] public Array<Node3D> GrowSpawnPoints = new();
	[Export] public Array<Node3D> ShakeSpawnPoints = new();

	[Export] public Node3D Stump;

	[Export] public FruitData FruitData;

	public bool IsDroppingFruit;
	public bool IsFalling;
	public bool IsShaking;

	public DateTime LastFruitDrop = DateTime.UnixEpoch;
	public const float FruitGrowTime = 10f;

	private bool _hasFruit;

	public override void _Ready()
	{
		base._Ready();
		// AddToGroup( "usables" );
		Stump?.Hide();
	}

	private void SpawnFruit()
	{
		if ( _hasFruit ) return;
		foreach ( var spawnPoint in GrowSpawnPoints )
		{
			var scene = FruitData.InTreeScene;
			if ( scene == null )
			{
				throw new Exception( "FruitData.InTreeScene is null" );
			}

			var fruit = scene.Instantiate<Node3D>();
			spawnPoint.AddChild( fruit );
			Logger.Debug( "Tree", "Added fruit to tree" );
		}
		_hasFruit = true;
	}

	public override void _Process( double delta )
	{
		base._Process( delta );
		CheckGrowth();
	}

	private void CheckGrowth()
	{
		if ( IsFalling ) return;
		if ( GrowSpawnPoints == null || GrowSpawnPoints.Count == 0 ) return;
		if ( ShakeSpawnPoints == null || ShakeSpawnPoints.Count == 0 ) return;

		if ( !_hasFruit && TimeNow - LastFruitDrop > TimeSpan.FromSeconds( FruitGrowTime ) )
		{
			SpawnFruit();
			// LastFruitDrop = DateTime.Now;
		}

	}

	public override System.Collections.Generic.Dictionary<string, object> GetNodeData()
	{
		return new() { { "LastFruitDrop", LastFruitDrop } };
	}

	public override void SetNodeData( PersistentItem item, System.Collections.Generic.Dictionary<string, object> data )
	{
		if ( item.TryGetCustomProperty<DateTime>( "LastFruitDrop", out var lastFruitDrop ) )
		{
			Logger.Info( "Tree", $"Setting LastFruitDrop CUSTOM PROPERTY: {lastFruitDrop}" );
			LastFruitDrop = lastFruitDrop;
		}
	}


	public override bool CanBePickedUp()
	{
		return false;
	}

	public bool CanUse( PlayerController player )
	{
		return !IsDroppingFruit && !IsFalling && !IsShaking;
	}

	public void OnUse( PlayerController player )
	{
		Shake();
	}

	private async void Shake()
	{
		/* // remove fruit from tree
		foreach ( var spawnPoint in GrowSpawnPoints )
		{
			foreach ( var child in spawnPoint.GetChildren() )
			{
				child.QueueFree();
			}
		}

		// add fruit to spawn points
		foreach ( var spawnPoint in ShakeSpawnPoints )
		{
			var scene = FruitData.PlaceScene;
			if ( scene == null )
			{
				throw new Exception( "FruitData.PlaceScene is null" );
			}

			// var fruit = Persistence.PersistentItem.Create( FruitData );

			var startPos = spawnPoint.GlobalTransform.Origin;
			var endPos = World.WorldToItemGrid( spawnPoint.GlobalTransform.Origin );

			// World.SpawnNode( FruitData, pos, World.ItemRotation.North, World.ItemPlacement.Floor );

			var tween = GetTree().CreateTween();
			tween.TweenProperty( spawnPoint, "position", spawnPoint.Scale, Vector3.Zero, 0.5f, Tween.TransitionType.Quad, Tween.EaseType.Out );

		} */

		if ( IsShaking ) return;

		IsShaking = true;

		var tween = GetTree().CreateTween();
		tween.TweenProperty( Model, "rotation_degrees", new Vector3( 0, 0, 5 ), 0.2f ).SetTrans( Tween.TransitionType.Quad ).SetEase( Tween.EaseType.In );
		tween.TweenProperty( Model, "rotation_degrees", new Vector3( 0, 0, -5 ), 0.2f ).SetTrans( Tween.TransitionType.Quad ).SetEase( Tween.EaseType.InOut );
		tween.TweenProperty( Model, "rotation_degrees", new Vector3( 0, 0, 5 ), 0.2f ).SetTrans( Tween.TransitionType.Quad ).SetEase( Tween.EaseType.InOut );
		tween.TweenProperty( Model, "rotation_degrees", new Vector3( 0, 0, 0 ), 0.2f ).SetTrans( Tween.TransitionType.Quad ).SetEase( Tween.EaseType.Out );
		await ToSignal( tween, Tween.SignalName.Finished );

		await DropFruitAsync();

		IsShaking = false;
	}

	public async Task DropFruitAsync()
	{
		if ( IsDroppingFruit ) return;
		IsDroppingFruit = true;
		for ( var i = 0; i < GrowSpawnPoints.Count; i++ )
		{
			var growPoint = GrowSpawnPoints[i];
			var shakePoint = ShakeSpawnPoints[i];

			/* foreach ( var child in growPoint.GetChildren() )
			{ */

			Logger.Info( "Tree", $"Checking growPoint: {growPoint.Name}" );

			var growNodeRaw = growPoint.GetChildren().FirstOrDefault();
			var shakeNodeRaw = shakePoint;

			Logger.Info( "Tree", $"growNodeRaw: {growNodeRaw}" );
			Logger.Info( "Tree", $"shakeNodeRaw: {shakeNodeRaw}" );

			/* if ( !IsInstanceValid( growNode ) || !IsInstanceValid( spawnNode ) )
			{
				throw new Exception( "growNode or spawnNode is null" );
			} */

			if ( growNodeRaw is not Node3D growNode || shakeNodeRaw is not Node3D spawnNode )
			{
				throw new Exception( "growNode or spawnNode is null" );
			}

			var endPos = spawnNode.GlobalTransform.Origin + Vector3.Up * 0.25f;

			// if ( child is not Node3D growNode ) continue;
			var tween = GetTree().CreateTween();
			var p = tween.TweenProperty( growNode, "global_position", endPos, 0.7f + GD.Randf() * 0.5f );
			p.SetTrans( Tween.TransitionType.Bounce );
			p.SetEase( Tween.EaseType.Out );
			p.SetDelay( GD.Randf() * 0.5f );

			tween.TweenCallback( Callable.From( () =>
			{
				growNode.QueueFree();
				var pos = World.WorldToItemGrid( shakePoint.GlobalTransform.Origin );
				World.SpawnNode( FruitData, pos, World.ItemRotation.North, World.ItemPlacement.Floor, true );

				GetNode<AudioStreamPlayer3D>( "Drop" ).Play();
			} ) );

			/* await ToSignal( tween, Tween.SignalName.Finished );

			growNode.QueueFree();
			var pos = World.WorldToItemGrid( shakePoint.GlobalTransform.Origin );
			World.SpawnNode( FruitData, pos, World.ItemRotation.North, World.ItemPlacement.Floor );

			GetNode<AudioStreamPlayer3D>( "Drop" ).Play(); */

		}

		await ToSignal( GetTree().CreateTimer( 1.5f ), Timer.SignalName.Timeout );
		IsDroppingFruit = false;
		LastFruitDrop = TimeNow;
		_hasFruit = false;
	}

	/* public override bool ShouldBeSaved()
	{
		return false;
	} */

}
