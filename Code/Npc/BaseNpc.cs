using System;
using DialogueManagerRuntime;
using Godot;
using Godot.Collections;
using vcrossing2.Code.Dialogue;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;
using vcrossing2.Code.Save;

namespace vcrossing2.Code.Npc;

public partial class BaseNpc : CharacterBody3D, IUsable
{
	// [Export] public virtual string NpcName { get; set; }
	[Export] public NpcData NpcData { get; set; }
	[Export] public Node3D Model { get; set; }
	[Export] public NavigationAgent3D NavigationAgent { get; set; }
	// public virtual string Description { get; set; }

	[Export] public float WalkSpeed { get; set; } = 2.0f;
	[Export] public float RunSpeed { get; set; } = 5.0f;
	[Export] public float RotationSpeed { get; set; } = 2.0f;
	[Export] public float Acceleration { get; set; } = 2f;
	[Export] public float Deceleration { get; set; } = 5f;
	private Vector3 TargetPosition { get; set; }
	public Node3D FollowTarget { get; set; }

	[Export] public Node3D CurrentInteractionTarget { get; set; }

	[Export] public Array<Resource> Dialogue { get; set; }

	private NpcSaveData _saveData;

	public NpcSaveData SaveData
	{
		get
		{
			if ( NpcData == null ) throw new NullReferenceException( "NpcData is null" );
			if ( string.IsNullOrEmpty( NpcData.NpcId ) ) throw new NullReferenceException( "NpcId is null" );
			return _saveData ??= NpcSaveData.Load( NpcData.NpcId );
		}
		set => _saveData = value;
	}

	private bool HasFollowTarget => FollowTarget != null;

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


	public override async void _Ready()
	{
		base._Ready();
		SetState( CurrentState.Idle );

		// NavigationAgent = GetNode<NavigationAgent3D>( "NavigationAgent" );
		NavigationAgent.PathDesiredDistance = 0.5f;
		NavigationAgent.TargetDesiredDistance = 0.5f;

		// SelectRandomActivity();
		// Callable.From( ActorSetup ).CallDeferred();

		if ( NpcData == null ) throw new NullReferenceException( "NpcData is null" );
		if ( string.IsNullOrEmpty( NpcData.NpcId ) ) throw new NullReferenceException( "NpcId is null" );

		SaveData = NpcSaveData.Load( NpcData.NpcId );

		Callable.From( SelectRandomActivity ).CallDeferred();
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

		if ( HasFollowTarget )
		{
			SetTargetPosition( FollowTarget.GlobalPosition );
		}

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

			if ( WalkTimeout <= 0 && !HasFollowTarget )
			{
				// GD.Print( "Walk timeout, panic" );
				SelectRandomActivity();
				return;
			}

			WalkToTarget( delta );
			return;
		}

		if ( State == CurrentState.Interacting )
		{
			if ( CurrentInteractionTarget == null ||
			     CurrentInteractionTarget.GlobalPosition.DistanceTo( GlobalPosition ) > 1.5f )
			{
				CurrentInteractionTarget = null;
				SelectRandomActivity();
			}

			return;
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

			if ( !HasFollowTarget )
			{
				SelectRandomActivity();
			}

			return;
		}

		var currentAgentPosition = GlobalTransform.Origin;
		var nextPathPosition = NavigationAgent.GetNextPathPosition();

		var moveSpeed = WalkSpeed;
		if ( HasFollowTarget && FollowTarget.GlobalPosition.DistanceTo( GlobalPosition ) > 2 )
		{
			moveSpeed = RunSpeed;
		}

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

	/*public virtual void OnInteract( PlayerInteract player )
	{
		// GD.Print( $"Interacting with {Name}" );
		WishVelocity = Vector3.Zero;
		SetState( CurrentState.Interacting );
		LookAtNode( player );
		CurrentInteractionTarget = player;
		// SetTargetPosition( new Vector3( 4, 0, 4 ) );
	}*/

	public int ReputationPoints { get; set; }

	public void AddRepPoints( int points )
	{
		ReputationPoints += points;
	}

	public bool CanUse( PlayerController player )
	{
		return true;
	}

	/*protected Godot.Collections.Dictionary<string, Variant> DialogueStates()
	{
		return new Godot.Collections.Dictionary<string, Variant>
		{
			{ "NpcName", NpcName },
			{ "AddRepPoints", new Godot.Collections.Array() { this, "AddRepPoints" } },
			{ "ReputationPoints", ReputationPoints },
		};
	}*/

	public void OnUse( PlayerController player )
	{
		Resource dialogue = null;

		if ( HasFollowTarget )
		{
			dialogue = ResourceLoader.Load<Resource>( "res://dialogue/stop_following.dialogue" );
		}
		else
		{
			WishVelocity = Vector3.Zero;
			SetState( CurrentState.Interacting );
			LookAtNode( player );
			CurrentInteractionTarget = player;

			dialogue = Dialogue.PickRandom();

			if ( dialogue == null )
			{
				Logger.LogError( "No dialogue found" );
				return;
			}
		}

		if ( dialogue == null )
		{
			Logger.LogError( "No dialogue found" );
			return;
		}

		/*if ( randomDialogue is not Dialogue dialogue )
		{
			Logger.LogError( "Dialogue is not a Dialogue" );
			return;
		}*/

		var node = DialogueManager.ShowDialogueBalloon(
			dialogue,
			// ResourceLoader.Load( "res://dialogue/test.dialogue" ),
			"",
			new Array<Variant>()
			{
				/*new Godot.Collections.Dictionary<string, Variant>
				{
					{ "NpcName", NpcName },
					{ "Text", "Lorem ipsum dolor sit amet, consectetur adipiscing elit." },
				},*/
				new DialogueState( player, this ),
			}
		);
	}
}
