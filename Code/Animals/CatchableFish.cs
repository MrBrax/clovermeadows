using System;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Objects;

namespace vcrossing.Code.WorldBuilder;

public partial class CatchableFish : Node3D
{

	public enum FishState
	{
		Idle = 0,
		Swimming = 1,
		FoundBobber = 2,
		TryingToEat = 3,
		Fighting = 4
	}

	[Export] public FishData Data { get; set; }

	// private float _speed = 1.0f;
	private const float _bobberMaxDistance = 2f;
	private const float _bobberDiscoverAngle = 45f;

	private Vector3 _velocity = Vector3.Zero;
	private const float _maxSwimSpeed = 0.8f;
	private const float _swimAcceleration = 0.1f;
	private const float _swimDeceleration = 0.5f;

	private const float _catchMsecWindow = 1500f;
	private float _lastNibble;
	private bool _isNibbleDeep;
	private const float _maxNibbles = 6;

	public FishState State { get; set; } = FishState.Idle;

	public FishingBobber Bobber { get; set; }

	public Vector3 WishedRotation { get; set; }

	private AnimationPlayer AnimationPlayer => GetNode<AnimationPlayer>( "fish_shadow2/AnimationPlayer" );

	public override void _Ready()
	{
		AddToGroup( "fish" );
	}

	public void SetState( FishState state )
	{
		Logger.Debug( "Fish", $"State changed to {state}." );
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
			// case FishState.TryingToEat:
			// 	TryToEat( delta );
			// 	break;
			case FishState.Fighting:
				Fight( delta );
				break;
		}

		Animate();

		if ( !WishedRotation.IsFinite() ) throw new Exception( "WishedRotation is not finite." );

		Rotation = Rotation.Lerp( WishedRotation, (float)delta * 2f );

	}


	private float _stamina;
	private float _nextSplashSound = 0;
	private void Fight( double delta )
	{

		if ( Time.GetTicksMsec() > _nextSplashSound )
		{
			GetNode<AudioStreamPlayer3D>( "Splash" ).Play();
			_nextSplashSound = Time.GetTicksMsec() + (GD.Randf() * 800);
		}

		if ( !ActionDone ) return;

		Logger.Info( "Fish", "Time ran out, fish got away." );
		FailCatch();

	}

	private void Animate()
	{
		if ( State == FishState.Swimming )
		{
			AnimationPlayer.Play( "swimming" );
			// AnimationPlayer.SpeedScale =
		}
		else
		{
			AnimationPlayer.Play( "idle" );
		}
	}

	/* private void TryToEat( double delta )
	{
		// Logger.Info( "Fish", "Trying to eat the bobber." );
	} */

	public void TryHook()
	{
		// if last nibble is within the catch window, catch the fish
		if ( _isNibbleDeep )
		{
			if ( Time.GetTicksMsec() - _lastNibble < _catchMsecWindow )
			{
				HookFish();
				return;
			}
			else
			{
				Logger.Info( "Fish", "Catch window missed." );
				FailCatch();
				return;
			}

			// TODO: remove fish?
			_isNibbleDeep = false;
			_lastNibble = 0;
		}
		else
		{
			Logger.Info( "Fish", "Nibble was not deep enough." );
			FailCatch();
		}
	}

	private int _nibbles = 0;

	private void FoundBobber( double delta )
	{

		if ( !ActionDone ) return;

		if ( !IsInstanceValid( Bobber ) )
		{
			Logger.Warn( "Fish", "Bobber is not valid." );
			SetState( FishState.Idle );
			Bobber = null;
			return;
		}

		if ( _isNibbleDeep )
		{
			FailCatch();
			return;
		}

		var bobberPosition = Bobber.GlobalTransform.Origin.WithY( 0 );
		var fishPosition = GlobalTransform.Origin.WithY( 0 );

		// rotate towards the bobber
		var moveDirection = (bobberPosition - fishPosition).Normalized();
		var targetRotation = Mathf.Atan2( moveDirection.X, moveDirection.Z );
		// var currentRotation = Rotation.Y;
		// var newRotation = Mathf.LerpAngle( currentRotation, targetRotation, (float)delta * 2 );

		WishedRotation = new Vector3( 0, float.IsNaN( targetRotation ) ? 0 : targetRotation, 0 );

		var distance = fishPosition.DistanceTo( bobberPosition );

		if ( distance > 0.3f )
		{
			// move towards the bobber
			GlobalPosition += moveDirection * _maxSwimSpeed * (float)delta;
			return;
		}

		_actionDuration = (float)GD.RandRange( 2000, 5000 );
		_lastAction = Time.GetTicksMsec();

		// random chance to try to eat the bobber
		/* if ( GD.RandRange( 0, 100 ) < 10 )
		{
			Logger.Info( "Fish", "Trying to eat the bobber." );
			_lastNibble = Time.GetTicksMsec();
			GetNode<AudioStreamPlayer3D>( "Chomp" ).Play();
			return;
		} */

		_isNibbleDeep = GD.RandRange( 0, 100 ) < 30 || _nibbles >= _maxNibbles;
		_lastNibble = Time.GetTicksMsec();

		if ( _isNibbleDeep )
		{
			GetNode<AudioStreamPlayer3D>( "Chomp" ).Play();
		}
		else
		{
			GetNode<AudioStreamPlayer3D>( "Nibble" ).Play();
		}

		_nibbles++;

		// Logger.Info( "Fish", $"Found bobber, waiting for {_actionDuration} msec." );

	}

	private void FailCatch()
	{
		Logger.Info( "Fish", "Failed to catch the fish." );
		Bobber.Rod.FishGotAway();
		QueueFree();
	}

	private void HookFish()
	{
		Logger.Info( "Fish", "Hooked the fish." );
		SetState( FishState.Fighting );
		Bobber.Fish = this;

		switch ( Data?.Size )
		{
			case FishData.FishSize.Tiny:
				_stamina = GD.RandRange( 2, 7 );
				break;
			case FishData.FishSize.Small:
				_stamina = GD.RandRange( 5, 10 );
				break;
			case FishData.FishSize.Medium:
				_stamina = GD.RandRange( 8, 15 );
				break;
			case FishData.FishSize.Large:
				_stamina = GD.RandRange( 10, 20 );
				break;
			default:
				_stamina = 10;
				break;
		}

		_lastAction = Time.GetTicksMsec();
		_actionDuration = GD.RandRange( 4000, 8000 );
	}

	private void CatchFish()
	{
		// SetState( FishState.Caught );
		GetNode<AudioStreamPlayer3D>( "Catch" ).Play();
		Bobber.Rod.CatchFish( this );
		// SetState( FishState.Caught );
	}

	private int _swimRandomRadius = 6;

	private Vector3 _swimStartPos;
	private Vector3 _swimTarget;
	private const int _swimTargetTries = 10;
	private float _swimProgress;

	private void Swim( double delta )
	{

		if ( !ActionDone ) return;

		// find a new swim target
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

			Logger.Debug( "Fish", $"New swim target: {_swimTarget}." );

		}


		// move towards the target smoothly
		var moveDirection = (_swimTarget - GlobalTransform.Origin).Normalized();
		var swimDistance = _swimTarget.DistanceTo( _swimStartPos );
		_swimProgress += (float)delta * (_maxSwimSpeed / swimDistance);

		Vector3 preA = _swimStartPos;
		Vector3 postB = _swimTarget;

		if ( preA.IsEqualApprox( postB ) )
		{
			Logger.Info( "Fish", "preA is equal to postB." );
			SetState( FishState.Idle );
			return;
		}

		if ( !_swimTarget.IsFinite() ) throw new Exception( "Swim target is not finite." );
		if ( !_swimStartPos.IsFinite() ) throw new Exception( "Swim start pos is not finite." );
		if ( !preA.IsFinite() ) throw new Exception( "preA is not finite." );
		if ( !postB.IsFinite() ) throw new Exception( "postB is not finite." );
		if ( float.IsNaN( _swimProgress ) ) throw new Exception( "swimProgress is NaN." );
		if ( float.IsNaN( swimDistance ) ) throw new Exception( "swimDistance is NaN." );

		var interp = _swimStartPos.CubicInterpolate( _swimTarget, preA, postB, _swimProgress );
		if ( !interp.IsFinite() ) throw new Exception( $"interp is not finite (_swimTarget: {_swimTarget}, preA: {preA}, postB: {postB}, _swimProgress: {_swimProgress})." );

		GlobalPosition = interp;

		// rotate towards the target
		var targetRotation = Mathf.Atan2( moveDirection.X, moveDirection.Z );
		if ( float.IsNaN( targetRotation ) ) throw new Exception( "targetRotation is NaN." );

		var newRotation = new Vector3( 0, float.IsNaN( targetRotation ) ? 0 : targetRotation, 0 );
		if ( !newRotation.IsFinite() ) throw new Exception( "newRotation is not finite." );

		WishedRotation = newRotation;

		// check if the fish has reached the target
		var distance = GlobalTransform.Origin.DistanceTo( _swimTarget );
		if ( distance < 0.01f )
		{
			Logger.Debug( "Fish", "Reached swim target." );
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
			Logger.Debug( "Fish", $"No water found at {randomPoint}." );
			// this will just try again
			return;
		}

		// check if the random point is on terrain or at the edge of the water where there's a steep slant
		var traceTerrain =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( randomPoint + Vector3.Up * 1f, randomPoint + Vector3.Down * 1f, World.TerrainLayer ) );

		if ( traceTerrain != null )
		{
			Logger.Debug( "Fish", $"Terrain found at {randomPoint}." );
			// this will just try again
			return;
		}

		// check if there is terrain between the fish and the random point
		var trace = new Trace( spaceState ).CastRay(
			PhysicsRayQueryParameters3D.Create( GlobalTransform.Origin, randomPoint, World.TerrainLayer ) );

		if ( trace != null )
		{
			Logger.Debug( "Fish", $"Terrain found between {GlobalTransform.Origin} and {randomPoint}." );
			// this will just try again
			return;
		}

		_swimTarget = randomPoint;
	}

	private float _lastAction;
	private float _actionDuration;
	private float _panicMaxIdles;

	private bool ActionDone => Time.GetTicksMsec() - _lastAction > _actionDuration;

	public float Weight { get; internal set; }


	private void Idle( double delta )
	{

		CheckForBobber();

		if ( !ActionDone ) return;

		// randomly start swimming, 20% chance
		if ( GD.RandRange( 0, 100 ) < 20 || _panicMaxIdles > 5 )
		{
			Logger.Debug( "Fish", "Starting to swim after being idle." );
			SetState( FishState.Swimming );
			_panicMaxIdles = 0;
			return;
		}

		// else, stay idle
		_actionDuration = (float)GD.RandRange( 1000, 5000 );
		_lastAction = Time.GetTicksMsec();
		_panicMaxIdles++;

		Logger.Debug( "Fish", $"Idle for {_actionDuration} msec." );

	}

	private void CheckForBobber()
	{

		if ( IsInstanceValid( Bobber ) )
		{
			return;
		}

		var bobber = GetTree().GetNodesInGroup( "fishing_bobber" ).Cast<FishingBobber>().FirstOrDefault();

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
		} */

		Bobber = bobber;
		Bobber.Fish = this;

		SetState( FishState.FoundBobber );

	}

	public void Pull()
	{
		if ( State != FishState.Fighting ) return;
		_stamina -= 1;
		Logger.Info( "Fish", $"Pulled the fish, stamina left: {_stamina}." );
		if ( _stamina <= 0 )
		{
			CatchFish();
		}
	}

	internal void SetSize( FishData.FishSize size )
	{
		var scale = 1f;
		switch ( size )
		{
			case FishData.FishSize.Tiny:
				scale = 0.5f;
				break;
			case FishData.FishSize.Small:
				scale = 0.75f;
				break;
			case FishData.FishSize.Medium:
				scale = 1f;
				break;
			case FishData.FishSize.Large:
				scale = 1.25f;
				break;
		}

		Scale = new Vector3( scale, scale, scale );
	}

}
