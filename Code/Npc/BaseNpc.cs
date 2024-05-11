using Godot;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Npc;

public partial class BaseNpc : CharacterBody3D
{
	[Export] public virtual string Name { get; set; }
	[Export] public Node3D Model { get; set; }
	[Export] public NavigationAgent3D NavigationAgent { get; set; }
	// public virtual string Description { get; set; }

	[Export] public float WalkSpeed { get; set; } = 2.0f;
	[Export] public float RotationSpeed { get; set; } = 2.0f;
	[Export] public float Acceleration { get; set; } = 2f;
	[Export] public float Deceleration { get; set; } = 5f;
	private Vector3 TargetPosition { get; set; }

	[Export] public Node3D CurrentInteractionTarget { get; set; }

	public Vector3 MovementTarget
	{
		get => NavigationAgent.TargetPosition;
		set => NavigationAgent.TargetPosition = value;
	}

	public enum CurrentState
	{
		Idle,
		Waiting,
		Walking,
		Talking,
		Interacting,
		Pose
	}

	public CurrentState State { get; set; }

	private float WaitingTime { get; set; }
	private float WalkTimeout { get; set; }


	public override void _Ready()
	{
		base._Ready();
		SetState( CurrentState.Idle );

		// NavigationAgent = GetNode<NavigationAgent3D>( "NavigationAgent" );
		NavigationAgent.PathDesiredDistance = 0.5f;
		NavigationAgent.TargetDesiredDistance = 0.5f;

		SelectRandomActivity();
		// Callable.From( ActorSetup ).CallDeferred();
	}

	public void SetTargetPosition( Vector3 position )
	{
		// GD.Print( $"Setting target position to {position}" );
		MovementTarget = position;
		WalkTimeout = 10f;
		SetState( CurrentState.Walking );
	}

	public void SetState( CurrentState state )
	{
		State = state;
	}

	public void LookAtNode( Node3D node )
	{
		var target = node.GlobalTransform.Origin;
		var position = GlobalTransform.Origin;
		var direction = target - position;
		Model.GlobalTransform = Model.GlobalTransform.LookingAt( position - direction, Vector3.Up );
	}

	public virtual void OnInteract( PlayerInteract player )
	{
		// GD.Print( $"Interacting with {Name}" );
		WishVelocity = Vector3.Zero;
		SetState( CurrentState.Interacting );
		LookAtNode( player );
		CurrentInteractionTarget = player;
		// SetTargetPosition( new Vector3( 4, 0, 4 ) );
	}

	private Vector3 WishVelocity { get; set; }

	public override void _PhysicsProcess( double delta )
	{
		base._PhysicsProcess( delta );

		/*if ( State == CurrentState.Idle )
		{
			GoToRandomPosition();
		}*/

		Velocity = Velocity.Lerp( WishVelocity, (float)delta * Acceleration );
		MoveAndSlide();

		if ( State == CurrentState.Waiting )
		{
			WaitingTime -= (float)delta;
			if ( WaitingTime <= 0 )
			{
				// GD.Print( "Waiting time is over" );
				SelectRandomActivity();
			}

			return;
		}

		if ( State == CurrentState.Walking )
		{
			WalkTimeout -= (float)delta;

			if ( WalkTimeout <= 0 )
			{
				// GD.Print( "Walk timeout, panic" );
				SelectRandomActivity();
				return;
			}

			WalkToTarget( delta );
		}

		if ( State == CurrentState.Interacting )
		{
			if ( CurrentInteractionTarget.GlobalPosition.DistanceTo( GlobalPosition ) > 1.5f )
			{
				CurrentInteractionTarget = null;
				SelectRandomActivity();
			}
		}
	}

	private void SelectRandomActivity()
	{
		var random = GD.Randf();
		if ( random < 0.5f )
		{
			// GD.Print( "Going to random position" );
			GoToRandomPosition();
		}
		else
		{
			// GD.Print( "Waiting" );
			WaitingTime = GD.RandRange( 1, 5 );
			SetState( CurrentState.Waiting );
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

			SelectRandomActivity();
			return;
		}

		var currentAgentPosition = GlobalTransform.Origin;
		var nextPathPosition = NavigationAgent.GetNextPathPosition();

		// Velocity = ( nextPathPosition - currentAgentPosition ).Normalized() * WalkSpeed;
		WishVelocity = currentAgentPosition.DirectionTo( nextPathPosition ) * WalkSpeed;

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

	/*private async void ActorSetup()
	{
		await ToSignal( GetTree(), SceneTree.SignalName.PhysicsFrame );


	}*/
}
