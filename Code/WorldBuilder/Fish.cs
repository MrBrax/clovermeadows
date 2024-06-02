using System;

namespace vcrossing2.Code.WorldBuilder;

public partial class Fish : Node3D
{

	public enum FishState
	{
		Idle = 0,
		Swimming = 1,
		FoundBobber = 2,
		TryingToEat = 3,
		Caught = 4
	}

	// private float _speed = 1.0f;
	private float _bobberMaxDistance = 2f;
	private float _bobberDiscoverAngle = 45f;

	private Vector3 _velocity = Vector3.Zero;
	private float _maxSwimSpeed = 0.8f;
	private float _swimAcceleration = 0.1f;
	private float _swimDeceleration = 0.5f;

	public FishState State { get; set; } = FishState.Idle;

	public override void _Ready()
	{
		AddToGroup( "fish" );
	}

	public void SetState( FishState state )
	{
		Logger.Info( "Fish", $"State changed to {state}." );
		State = state;
	}

	public override void _Process( double delta )
	{

		switch ( State )
		{
			case FishState.Idle:
				Idle( delta );
				break;
			case FishState.Swimming:
				Swim( delta );
				break;
			case FishState.FoundBobber:
				FoundBobber( delta );
				break;
			case FishState.TryingToEat:
				TryToEat( delta );
				break;
		}

		// Rotate the fish
		// RotateY( (float)delta );

		/* var bobber = GetTree().GetNodesInGroup( "fishing_bobber" ).Cast<FishingBobber>().FirstOrDefault();

		if ( !IsInstanceValid( bobber ) )
		{
			return;
		}

		var bobberPosition = bobber.GlobalTransform.Origin.WithY( 0 );
		var fishPosition = GlobalTransform.Origin.WithY( 0 );

		// check if the bobber is near the fish
		var distance = fishPosition.DistanceTo( bobberPosition );
		if ( distance > _bobberMaxDistance )
		{
			// Logger.Info( "Fish", $"Bobber is too far away ({distance})." );
			return;
		}

		// check if the bobber is within the fish's view
		var direction = (bobberPosition - GlobalTransform.Origin).Normalized();
		var angle = Mathf.RadToDeg( Mathf.Acos( direction.Dot( GlobalTransform.Basis.Z ) ) );

		/* if ( angle > _bobberDiscoverAngle )
		{
			Logger.Info( "Fish", $"Bobber is not in view ({angle})." );
			return;
		} *

		// move towards the bobber
		var target = bobber.GlobalTransform.Origin;
		var moveDirection = (target - GlobalTransform.Origin).Normalized();
		GlobalPosition += moveDirection * _speed * (float)delta;

		// rotate towards the bobber
		var targetRotation = Mathf.Atan2( moveDirection.X, moveDirection.Z );
		var currentRotation = Rotation.Y;
		var newRotation = Mathf.LerpAngle( currentRotation, targetRotation, (float)delta * 2 );

		Rotation = new Vector3( 0, newRotation, 0 );

		if ( distance < 0.1f )
		{
			Logger.Info( "Fish", "Reached bobber." );
			bobber.Rod.CatchFish( this );
		} */
	}

	private void TryToEat( double delta )
	{
		throw new NotImplementedException();
	}

	private void FoundBobber( double delta )
	{
		throw new NotImplementedException();
	}

	private int _swimRandomRadius = 6;

	private Vector3 _swimStartPos;
	private Vector3 _swimTarget;
	private const int _swimTargetTries = 10;
	private float _swimProgress;

	private void Swim( double delta )
	{

		if ( !ActionDone ) return;

		if ( _swimTarget == Vector3.Zero )
		{
			var randomTries = 0;

			do
			{
				GetNewSwimTarget();
				randomTries++;
			} while ( _swimTarget == Vector3.Zero && randomTries < _swimTargetTries );

			if ( _swimTarget == Vector3.Zero )
			{
				Logger.Warn( "Fish", "Failed to find a swim target." );
				SetState( FishState.Idle );
				return;
			}

			_lastAction = Time.GetTicksMsec();
			_swimStartPos = GlobalPosition;
			_swimProgress = 0;

		}

		/* // move towards the target
		var moveDirection = (_swimTarget - GlobalTransform.Origin).Normalized();
		// GlobalPosition += moveDirection * _speed * (float)delta;
		_velocity = moveDirection * _speed;

		GlobalPosition += _velocity * (float)delta; */

		/* // rotate towards the target
		var targetRotation = Mathf.Atan2( moveDirection.X, moveDirection.Z );
		var currentRotation = Rotation.Y;
		var newRotation = Mathf.LerpAngle( currentRotation, targetRotation, (float)delta * 2 );

		Rotation = new Vector3( 0, newRotation, 0 ); */

		// move towards the target smoothly
		var moveDirection = (_swimTarget - GlobalTransform.Origin).Normalized();
		/* _velocity = _velocity.Lerp( moveDirection * _maxSwimSpeed, _swimAcceleration * (float)delta );

		GlobalPosition += _velocity * (float)delta; */

		// calculate fraction based off the distance between the start and target and the current time
		var swimDistance = _swimTarget.DistanceTo( _swimStartPos );
		// var frac = (float)((Time.GetTicksMsec() - _lastAction) / swimDistance);

		_swimProgress += (float)delta * (_maxSwimSpeed / swimDistance);

		Vector3 preA = _swimStartPos;
		Vector3 postB = _swimTarget;

		GlobalPosition = _swimStartPos.CubicInterpolate( _swimTarget, preA, postB, _swimProgress );

		// rotate towards the target
		var targetRotation = Mathf.Atan2( moveDirection.X, moveDirection.Z );
		var currentRotation = Rotation.Y;
		var newRotation = Mathf.LerpAngle( currentRotation, targetRotation, (float)delta * 2 );

		Rotation = new Vector3( 0, newRotation, 0 );


		// check if the fish has reached the target
		var distance = GlobalTransform.Origin.DistanceTo( _swimTarget );
		if ( distance < 0.01f )
		{
			Logger.Info( "Fish", "Reached swim target." );
			_swimTarget = Vector3.Zero;
			SetState( FishState.Idle );
			return;
		}

		CheckForBobber();

	}

	private void GetNewSwimTarget()
	{
		var randomPoint = GlobalPosition + new Vector3( GD.RandRange( -_swimRandomRadius, _swimRandomRadius ), 0, GD.RandRange( -_swimRandomRadius, _swimRandomRadius ) );

		var spaceState = GetWorld3D().DirectSpaceState;

		// check if the random point is in water
		var traceWater =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( randomPoint + Vector3.Up * 1f, randomPoint + Vector3.Down * 1f, World.WaterLayer ) );

		if ( traceWater == null )
		{
			Logger.Warn( "Fish", $"No water found at {randomPoint}." );
			// this will just try again
			return;
		}

		// check if the random point is on terrain or at the edge of the water where there's a steep slant
		var traceTerrain =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( randomPoint + Vector3.Up * 1f, randomPoint + Vector3.Down * 1f, World.TerrainLayer ) );

		if ( traceTerrain != null )
		{
			Logger.Warn( "Fish", $"Terrain found at {randomPoint}." );
			// this will just try again
			return;
		}

		// check if there is terrain between the fish and the random point
		var trace = new Trace( spaceState ).CastRay(
			PhysicsRayQueryParameters3D.Create( GlobalTransform.Origin, randomPoint, World.TerrainLayer ) );

		if ( trace != null )
		{
			Logger.Warn( "Fish", $"Terrain found between {GlobalTransform.Origin} and {randomPoint}." );
			// this will just try again
			return;
		}

		_swimTarget = randomPoint;
	}

	private float _lastAction;
	private float _actionDuration;
	private float _panicMaxIdles;

	private bool ActionDone => Time.GetTicksMsec() - _lastAction > _actionDuration;

	private void Idle( double delta )
	{
		if ( !ActionDone ) return;

		// randomly start swimming, 20% chance
		if ( GD.RandRange( 0, 100 ) < 20 || _panicMaxIdles > 5 )
		{
			Logger.Info( "Fish", "Starting to swim after being idle." );
			SetState( FishState.Swimming );
			_panicMaxIdles = 0;
			return;
		}

		// else, stay idle
		_actionDuration = (float)GD.RandRange( 1000, 5000 );
		_lastAction = Time.GetTicksMsec();
		_panicMaxIdles++;

		Logger.Info( "Fish", $"Idle for {_actionDuration} msec." );

		CheckForBobber();

	}

	private void CheckForBobber()
	{

	}
}
