using System;
using System.Threading.Tasks;
using Godot.Collections;
using vcrossing.Code.Data;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public partial class Tree : WorldItem, IUsable, IPersistence
{

	// public TreeData TreeData { get; private set; }

	[Export] public Array<Node3D> GrowSpawnPoints;
	[Export] public Array<Node3D> ShakeSpawnPoints;

	[Export] public Node3D Stump;

	[Export] public FruitData FruitData;

	public bool IsDroppingFruit;
	public bool IsFalling;

	public DateTime LastFruitDrop = DateTime.UnixEpoch;
	public const float FruitGrowTime = 10f;

	private bool _hasFruit;

	public override void _Ready()
	{
		base._Ready();
		AddToGroup( "usables" );
		Logger.Info( "Tree", "Ready" );
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
			Logger.Info( "Tree", "Added fruit to tree" );
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
		if ( GrowSpawnPoints.Count == 0 ) return;
		if ( ShakeSpawnPoints.Count == 0 ) return;

		if ( !_hasFruit && DateTime.Now - LastFruitDrop > TimeSpan.FromSeconds( FruitGrowTime ) )
		{
			SpawnFruit();
			// LastFruitDrop = DateTime.Now;
		}

	}

	public override System.Collections.Generic.Dictionary<string, object> GetNodeData()
	{
		return new() { { "LastFruitDrop", LastFruitDrop } };
	}

	public override void SetNodeData( System.Collections.Generic.Dictionary<string, object> data )
	{
		if ( data.ContainsKey( "LastFruitDrop" ) )
		{
			LastFruitDrop = (DateTime)data["LastFruitDrop"];
		}
	}


	public override bool CanBePickedUp()
	{
		return false;
	}

	public bool CanUse( PlayerController player )
	{
		return !IsDroppingFruit && !IsFalling;
	}

	public void OnUse( PlayerController player )
	{
		Shake();
	}

	private void Shake()
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

		DropFruit();
	}

	public async Task DropFruit()
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
		LastFruitDrop = DateTime.Now;
		_hasFruit = false;
	}

	/* public override bool ShouldBeSaved()
	{
		return false;
	} */
}
