using System;
using Godot;
using Godot.Collections;
using vcrossing.Code.Data;
using vcrossing.Code.Player;

namespace vcrossing.Code.WorldBuilder;

public partial class FishSpot : Node3D, IWorldLoaded
{

	[Export] public Array<FishData> SpecialFish { get; set; }
	[Export] public FishData.FishLocation Location { get; set; } = FishData.FishLocation.River;
	[Export] public float SpawnRadius { get; set; } = 5f;

	private float WaterCheckHeight { get; set; } = 2f;

	private bool CheckForWater( Vector3 position )
	{

		var spaceState = GetWorld3D().DirectSpaceState;

		var traceWater =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( position + (Vector3.Up * WaterCheckHeight), position + (Vector3.Down * WaterCheckHeight), World.WaterLayer ) );

		if ( traceWater == null )
		{
			// Logger.Warn( "FishSpot", $"No water found at {position}." );
			return false;
		}

		var traceTerrain =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( traceWater.Position, traceWater.Position + (Vector3.Down * 1f), World.TerrainLayer ) );

		if ( traceTerrain != null )
		{
			// Logger.Warn( "FishSpot", $"Terrain found at {position}." );
			return false;
		}

		return true;

	}

	public override void _Ready()
	{
		// if ( GD.RandRange( 0, 100 ) > 50 ) SpawnFish();
		// SpawnFish();
	}

	private void SpawnFish()
	{
		var findPositionTry = 0;
		var basePosition = GlobalTransform.Origin;

		Logger.Debug( "FishSpot", $"Trying to spawn fish at {basePosition}." );

		FishData fishData;

		if ( SpecialFish != null && SpecialFish.Count > 0 )
		{
			fishData = SpecialFish.PickRandom();
		}
		else
		{
			var resources = Resources.LoadAllResources( "res://items/fish/" );
			var fish = resources.Where( r => r is FishData ).Select( r => r as FishData ).ToList();

			fishData = fish.PickRandom();
		}

		if ( fishData == null )
		{
			Logger.Warn( "FishSpot", $"No fish data found." );
			return;
		}

		while ( findPositionTry < 10 )
		{
			/* var randomPosition = new Vector3(
                basePosition.X + (GD.Randf() * SpawnRadius * 2) - SpawnRadius,
                basePosition.Y,
                basePosition.Z + (GD.Randf() * SpawnRadius * 2) - SpawnRadius
            ); */
			var randomPosition = basePosition;

			if ( CheckForWater( randomPosition ) )
			{
				// var fishData = Fish[GD.RandRange( 0, Fish.Count )];

				var worldManager = GetNode<WorldManager>( "/root/Main/WorldManager" );
				if ( !IsInstanceValid( worldManager ) ) throw new NullReferenceException( "WorldManager not found." );
				var activeWorld = worldManager.ActiveWorld;
				if ( !IsInstanceValid( activeWorld ) ) throw new NullReferenceException( "ActiveWorld not found." );

				var fish = Loader.LoadResource<PackedScene>( "res://world/fishing/fish_shadow.tscn" ).Instantiate<CatchableFish>();

				activeWorld.AddChild( fish );

				if ( !fish.IsInsideTree() ) throw new Exception( "Fish not added to world." );

				fish.Data = fishData;
				fish.GlobalPosition = randomPosition;
				fish.SetSize( fishData.Size );
				fish.Weight = fishData.GetRandomWeight();

				Logger.Debug( "FishSpot", $"Spawned fish {fishData.Name} at {randomPosition}." );

				return;
			}

			findPositionTry++;
		}

		Logger.Warn( "FishSpot", $"Failed to find a valid position to spawn fish on {Name}." );

	}

	public void WorldLoaded()
	{
		if ( GD.RandRange( 0, 100 ) > 50 ) SpawnFish();
	}


	private float _lastSpawnCheck = 0f;
	private float _nextSpawnCheck = 60f + GD.Randf() * 60f;
	public override void _Process( double delta )
	{

		_lastSpawnCheck += (float)delta;

		if ( _lastSpawnCheck > _nextSpawnCheck )
		{
			_lastSpawnCheck = 0f;
			_nextSpawnCheck = 60f + GD.Randf() * 60f;
			CheckSpawn();
		}

	}

	private void CheckSpawn()
	{
		var allFish = GetTree().GetNodesInGroup( "fish" ).Cast<CatchableFish>().ToList();
		var fishNearby = allFish.Where( f => f.GlobalPosition.DistanceTo( GlobalTransform.Origin ) < SpawnRadius * 1.5f ).ToList();
		if ( fishNearby.Count > 0 ) return;
		if ( PlayerNearby() ) return;
		if ( GD.RandRange( 0, 100 ) > 50 ) return;
		SpawnFish();
	}

	private bool PlayerNearby()
	{
		var player = GetTree().GetNodesInGroup( "player" ).FirstOrDefault() as PlayerController;
		if ( player == null ) return false;
		return player.GlobalPosition.DistanceTo( GlobalTransform.Origin ) < SpawnRadius * 1.5f;
	}
}
