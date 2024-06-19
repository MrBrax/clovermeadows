using System;
using DialogueManagerRuntime;
using Godot;
using Godot.Collections;
using vcrossing.Code.Carriable;
using vcrossing.Code.Data;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Dialogue;
using vcrossing.Code.Helpers;
using vcrossing.Code.Items;
using vcrossing.Code.Player;
using vcrossing.Code.Save;

namespace vcrossing.Code.Npc.Fauna;

public partial class BaseFauna : CharacterBody3D, IDataPath
{

	[Export] public string ItemDataPath { get; set; }

	[Export, Require] public Node3D Model { get; set; }

	[Export, Require] public NavigationAgent3D NavigationAgent { get; set; }

	[Export] public AnimationPlayer AnimationPlayer { get; set; }

	[Export] public Area3D SightArea { get; set; }

	[Export] public float MoveSpeed { get; set; } = 2.0f;

	[Export] public float Acceleration { get; set; } = 2f;
	[Export] public float Deceleration { get; set; } = 5f;
	[Export] public float RotationSpeed { get; set; } = 2.0f;

	private float WaitingTime { get; set; }
	private float WalkTimeout { get; set; }

	private Vector3 TargetPosition { get; set; }
	public Node3D FollowTarget { get; set; }

	public BaseNpc.CurrentState State { get; set; }

	private Vector3 WishVelocity { get; set; }

	public Vector3 MovementTarget
	{
		get => NavigationAgent.TargetPosition;
		set => NavigationAgent.TargetPosition = value;
	}

	// [Export] public string AnimalDataPath { get; set; }
	private AnimalData _animalData;
	public AnimalData AnimalData
	{
		get
		{
			if ( _animalData == null )
			{
				_animalData = Loader.LoadResource<AnimalData>( ItemDataPath );
				if ( _animalData == null )
				{
					throw new Exception( $"Animal data not found at path {ItemDataPath}" );
				}
			}
			return _animalData;
		}
	}

	private List<Node3D> _nodesInSight = new();

	public override void _Ready()
	{
		base._Ready();

		AddToGroup( "usables" );

		if ( SightArea != null )
		{
			SightArea.BodyEntered += OnSightAreaBodyEntered;
			SightArea.BodyExited += OnSightAreaBodyExited;
		}

		Callable.From( SelectRandomActivity ).CallDeferred();
	}

	private void OnSightAreaBodyEntered( Node body )
	{
		if ( body is Node3D node )
		{
			_nodesInSight.Add( node );
		}
	}

	private void OnSightAreaBodyExited( Node body )
	{
		if ( body is Node3D node )
		{
			_nodesInSight.Remove( node );
		}
	}

	public void SetState( BaseNpc.CurrentState state )
	{
		Logger.Info( "BaseFauna", $"Setting state to {state}" );
		State = state;
	}

	private void SelectRandomActivity()
	{
		Logger.Info( "BaseFauna", "Selecting random activity" );
		var random = GD.Randf();
		if ( random < 0.5f )
		{
			// GD.Print( "Going to random position" );
			Logger.Info( "BaseFauna", "Going to random position" );
			GoToRandomPosition();
		}
		else
		{
			// GD.Print( "Waiting" );
			Logger.Info( "BaseFauna", "Waiting" );
			WaitingTime = GD.RandRange( 1, 5 );
			SetState( BaseNpc.CurrentState.Waiting );
		}
	}

	private void WalkToTarget( double delta )
	{

		if ( NavigationAgent.IsNavigationFinished() )
		{
			// GD.Print( "Reached target position" );

			// Velocity = Velocity.Lerp( Vector3.Zero, (float)(delta * 0.1f) );
			// MoveAndSlide();
			WishVelocity = Vector3.Zero;

			/* if ( !HasFollowTarget )
			{
				SelectRandomActivity();
			} */

			return;
		}

		var currentAgentPosition = GlobalTransform.Origin;
		var nextPathPosition = NavigationAgent.GetNextPathPosition();

		var moveSpeed = MoveSpeed;
		/* if ( HasFollowTarget && FollowTarget.GlobalPosition.DistanceTo( GlobalPosition ) > 2 )
		{
			moveSpeed = RunSpeed;
		} */

		// Velocity = ( nextPathPosition - currentAgentPosition ).Normalized() * WalkSpeed;
		WishVelocity = currentAgentPosition.DirectionTo( nextPathPosition ) * moveSpeed;

		var direction = nextPathPosition - currentAgentPosition;
		// Model.GlobalTransform = Model.GlobalTransform.LookingAt( currentAgentPosition - direction, Vector3.Up );
		// smoothly rotate the player model towards the direction
		var targetRotation = Mathf.Atan2( direction.X, direction.Z );
		var currentRotation = Model.Rotation.Y;
		var newRotation = Mathf.LerpAngle( currentRotation, targetRotation, (float)delta * RotationSpeed );
		newRotation = Mathf.Wrap( newRotation, -Mathf.Pi, Mathf.Pi );
		Model.Rotation = new Vector3( 0, newRotation, 0 );

		// MoveAndSlide();
	}

	public void GoToRandomPosition()
	{
		// var randomPosition = new Vector3( GD.Randf() * 10, 0, GD.Randf() * 10 ) + new Vector3( 4, 0, 4 );
		var randomPosition = GlobalPosition + new Vector3( GD.RandRange( -1, 1 ) * 5, 0, GD.RandRange( -1, 1 ) * 5 );
		SetTargetPosition( randomPosition );
	}

	public void SetTargetPosition( Vector3 position )
	{
		// GD.Print( $"Setting target position to {position}" );
		Logger.Info( "BaseFauna", $"Setting target position to {position}" );
		MovementTarget = position;
		WalkTimeout = 10f;
		SetState( BaseNpc.CurrentState.Walking );
	}

	protected void Animate()
	{
		/* if ( Velocity.Length() > 0.1f )
		{
			AnimationPlayer.Play( "Walk" );
		}
		else
		{
			AnimationPlayer.Play( "Idle" );
		} */

	}

	private void CheckSightedNodes()
	{
		if ( _nodesInSight.Count == 0 ) return;

		foreach ( var node in _nodesInSight )
		{
			// bodies should automatically be removed from the list when they exit the sight area, but just in case
			if ( node.GlobalPosition.DistanceTo( GlobalPosition ) > 3 )
			{
				_nodesInSight.Remove( node );
				return;
			}

			if ( node is PlayerController player )
			{
				var velocity = player.Velocity;
				if ( velocity.Length() > 1f )
				{
					Logger.Info( "BaseFauna", "scared" );
				}
			}
		}
	}

	public override void _PhysicsProcess( double delta )
	{
		base._PhysicsProcess( delta );

		Animate();

		CheckSightedNodes();

		/*if ( State == CurrentState.Idle )
		{
			GoToRandomPosition();
		}*/

		// if ( IsDisabled ) return;

		Velocity = Velocity.Lerp( WishVelocity, (float)delta * Acceleration );
		MoveAndSlide();

		/* if ( !ShouldDisableMovement() )
		{
			Velocity = Velocity.Lerp( WishVelocity, (float)delta * Acceleration );
			MoveAndSlide();
		}
		else
		{
			Velocity = Vector3.Zero;
		} */

		/* if ( HasFollowTarget )
		{
			CheckForBed();

			if ( !IsLyingOrSitting )
			{
				// SetTargetPositionBehind( FollowTarget );
				// SetTargetPosition( FollowTarget.GlobalPosition );

				if ( FollowTarget is PlayerController player )
				{
					SetTargetPositionBehind( player.GlobalPosition, player.Model.Basis );
				}
			}
		}  */

		if ( State == BaseNpc.CurrentState.Waiting )
		{
			WaitingTime -= (float)delta;
			if ( WaitingTime <= 0 )
			{
				// GD.Print( "Waiting time is over" );
				SelectRandomActivity();
			}

			return;
		}

		if ( State == BaseNpc.CurrentState.Walking )
		{
			WalkTimeout -= (float)delta;

			if ( WalkTimeout <= 0 )
			{
				Logger.Warn( "BaseFauna", "Walk timeout" );
				SelectRandomActivity();
				return;
			}

			WalkToTarget( delta );
			return;
		}

	}

}
