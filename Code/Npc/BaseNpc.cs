using System;
using Godot.Collections;
using vcrossing.Code.Carriable;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Items;
using vcrossing.Code.Player;
using vcrossing.Code.Save;
using YarnSpinnerGodot;

namespace vcrossing.Code.Npc;

public partial class BaseNpc : CharacterBody3D, IUsable, IPushable, INettable
{
	// [Export] public virtual string NpcName { get; set; }
	[Export] public string NpcData { get; set; }
	[Export, Require] public Node3D Model { get; set; }
	[Export, Require] public NavigationAgent3D NavigationAgent { get; set; }
	// public virtual string Description { get; set; }

	[Export] public float WalkSpeed { get; set; } = 2.0f;
	[Export] public float RunSpeed { get; set; } = 5.0f;
	[Export] public float RotationSpeed { get; set; } = 2.0f;
	[Export] public float Acceleration { get; set; } = 2f;
	[Export] public float Deceleration { get; set; } = 5f;
	private Vector3 TargetPosition { get; set; }
	public Node3D FollowTarget { get; set; }

	public float PushForce { get; set; } = 1f;
	public bool PushOnce { get; set; } = false;

	[Export] public Node3D CurrentInteractionTarget { get; set; }

	// [Export] public Array<Resource> Dialogue { get; set; }

	protected WorldManager WorldManager => GetNode<WorldManager>( "/root/Main/WorldManager" );
	protected NpcManager NpcManager => GetNode<NpcManager>( "/root/Main/NpcManager" );



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
		Pose,
		SittingOrLying
	}

	public CurrentState State { get; set; }

	protected float WaitingTime { get; set; }
	protected float WalkTimeout { get; set; }

	public bool IsDisabled { get; set; }

	protected Vector3 LastPosition { get; set; }

	public virtual bool ShouldDisableMovement()
	{
		if ( IsDisabled ) return true;
		if ( WorldManager.IsLoading ) return true;
		return false;
	}

	public NpcData GetData()
	{
		if ( NpcData == null ) throw new NullReferenceException( "NpcData is null" );
		return Loader.LoadResource<NpcData>( NpcData );
	}

	public override void _Ready()
	{
		base._Ready();

		AddToGroup( "usables" );

		// WorldManager.WorldUnload += OnWorldUnloaded;
		// WorldManager.WorldLoaded += OnWorldLoaded;

		SetState( CurrentState.Idle );

		// NavigationAgent = GetNode<NavigationAgent3D>( "NavigationAgent" );
		NavigationAgent.PathDesiredDistance = 0.5f;
		NavigationAgent.TargetDesiredDistance = 0.5f;

		// SelectRandomActivity();
		// Callable.From( ActorSetup ).CallDeferred();

		if ( NpcData == null || GetData() == null ) throw new NullReferenceException( "NpcData is null" );
		if ( string.IsNullOrEmpty( GetData().NpcId ) ) throw new NullReferenceException( "NpcId is null" );

		Callable.From( SelectRandomActivity ).CallDeferred();
	}

	public void OnWorldUnloaded( World world )
	{
		/* if ( !IsInstanceValid( this ) ) return;
		/* IsDisabled = true;
		if ( FollowTarget == null )
		{
			QueueFree();
		} *
		var npcId = GetData().NpcId;
		if ( FollowTarget != null )
		{
			NpcManager.NpcInstanceData[npcId].FollowTarget = FollowTarget;
			if ( FollowTarget is PlayerController player )
			{
				NpcManager.NpcInstanceData[npcId].WorldPath = player.ExitWorld;
				Logger.Info( "Npc", $"Saved follow target {FollowTarget.Name} for {npcId}" );
			}
		} else {
			NpcManager.NpcInstanceData[npcId].FollowTarget = null;
		} */
		IsDisabled = true;
	}

	public void OnWorldLoaded( World world )
	{
		IsDisabled = false;
		/*

		if ( FollowTarget is not PlayerController player )
		{
			Logger.LogError( "Follow target is not a player" );
			return;
		}

		var playerExitName = player.ExitName;
		if ( string.IsNullOrEmpty( playerExitName ) )
		{
			Logger.LogError( "Player exit name is null" );
			return;
		}

		var node = world.FindChild( playerExitName );
		if ( node == null )
		{
			throw new Exception( $"Exit node {playerExitName} not found." );
			return;
		}

		if ( node is not Node3D exit )
		{
			throw new Exception( $"Exit node {playerExitName} is not a Node3D." );
			return;
		}

		GD.Print( $"Player entered area {playerExitName}, moving to {exit.Name} @ {exit.Position}" );
		Position = exit.GlobalPosition; */

		var npcId = GetData().NpcId;

		if ( NpcManager.NpcInstanceData.ContainsKey( npcId ) )
		{
			var data = NpcManager.NpcInstanceData[npcId];
			if ( data.FollowTarget != null )
			{
				FollowTarget = data.FollowTarget;
				if ( FollowTarget is PlayerController player )
				{
					player.PlayerEnterArea += FollowPlayerIntoNewArea;
				}
				Logger.Info( "Npc", $"Loaded follow target {FollowTarget.Name} for {npcId}" );
			}

			if ( !string.IsNullOrEmpty( data.FollowTargetExit ) )
			{
				var node = world.FindChild( data.FollowTargetExit );
				if ( !IsInstanceValid( node ) )
				{
					throw new Exception( $"Exit node {data.FollowTargetExit} not found." );
				}

				if ( node is not Node3D exit )
				{
					throw new Exception( $"Exit node {data.FollowTargetExit} is not a Node3D." );
				}

				Logger.Info( "Npc", $"Player entered area, following to exit {exit.Name} @ {exit.GlobalPosition}" );

				GlobalPosition = exit.GlobalPosition;

				NpcManager.NpcInstanceData[npcId].FollowTargetExit = null;
			}
		}
		else
		{
			Logger.Warn( "Npc", $"No saved data for {npcId}" );
		}
	}

	public void SetTargetPosition( Vector3 position )
	{
		// GD.Print( $"Setting target position to {position}" );
		MovementTarget = position;
		WalkTimeout = 10f;
		SetState( CurrentState.Walking );
	}

	/// <summary>
	///  Set target position 1 unit behind the node, rotated
	/// </summary>
	public void SetTargetPositionBehind( Vector3 position, Basis basis )
	{
		var direction = basis.Z.Normalized();
		var targetPosition = position - direction;
		SetTargetPosition( targetPosition );
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
		if ( !IsInstanceValid( Model ) ) throw new NullReferenceException( "Model is null" );
		Model.GlobalTransform = Model.GlobalTransform.LookingAt( position - direction, Vector3.Up );
	}


	private Vector3 WishVelocity { get; set; }

	public override void _PhysicsProcess( double delta )
	{
		base._PhysicsProcess( delta );

		if ( !ShouldDisableMovement() )
		{
			Velocity = Velocity.Lerp( WishVelocity, (float)delta * Acceleration );
			MoveAndSlide();
		}
		else
		{
			Velocity = Vector3.Zero;
		}

		if ( HasFollowTarget )
		{
			// TODO: move to villager
			/* CheckForBed();

			if ( !IsLyingOrSitting )
			{
				// SetTargetPositionBehind( FollowTarget );
				// SetTargetPosition( FollowTarget.GlobalPosition );

				if ( FollowTarget is PlayerController player )
				{
					SetTargetPositionBehind( player.GlobalPosition, player.Model.Basis );
				}
			} */
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
			if ( !IsInstanceValid( CurrentInteractionTarget ) /* ||
				 CurrentInteractionTarget.GlobalPosition.DistanceTo( GlobalPosition ) > 1.5f */ )
			{
				Logger.Info( "Npc", "Interaction target is gone" );
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
		/* if ( IsLyingOrSitting ) return; */ // TODO: move to villager

		if ( NavigationAgent.IsNavigationFinished() )
		{
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
		Logger.Info( "Npc", $"Adding {points} reputation points" );
		ReputationPoints += points;
	}

	public bool CanUse( PlayerController player )
	{
		return true;
	}


	public virtual void OnUse( PlayerController player )
	{
		TalkTo( player, "Start" );
	}

	protected virtual void SetupDialogueRunner( PlayerController player, DialogueRunner runner )
	{
		runner.VariableStorage.SetValue( "$NpcName", GetData().NpcName );
		runner.VariableStorage.SetValue( "$PlayerName", player.PlayerName );

		runner.AddCommandHandler( "AddRepPoints", ( int points ) =>
		{
			AddRepPoints( points );
		} );

		runner.AddCommandHandler( "SetFollowTarget", ( Node3D node ) =>
		{
			SetFollowTarget( node );
		} );

		runner.AddCommandHandler( "FollowPlayer", () =>
		{
			if ( player is PlayerController p )
			{
				SetFollowTarget( p );
			}
		} );
	}

	protected virtual void TalkTo( PlayerController player, string title )
	{

		var runner = GetNode<DialogueRunner>( "/root/Main/UserInterface/YarnSpinnerCanvasLayer/DialogueRunner" );

		runner.onDialogueComplete += OnDialogueComplete;

		/* runner.VariableStorage.SetValue( "$NpcName", GetData().NpcName );
		runner.VariableStorage.SetValue( "$PlayerName", player.PlayerName );

		runner.AddCommandHandler( "AddRepPoints", ( int points ) =>
		{
			AddRepPoints( points );
		} );

		runner.AddCommandHandler( "SetFollowTarget", ( Node3D node ) =>
		{
			SetFollowTarget( node );
		} );

		runner.AddCommandHandler( "FollowPlayer", () =>
		{
			if ( player is PlayerController p )
			{
				SetFollowTarget( p );
			}
		} ); */

		SetupDialogueRunner( player, runner );

		runner.StartDialogue( title );

		WishVelocity = Vector3.Zero;
		SetState( CurrentState.Interacting );
		LookAtNode( player );
		CurrentInteractionTarget = player;

	}

	protected virtual void OnDialogueComplete()
	{
		// GD.Print( "Dialogue complete" );
		CurrentInteractionTarget = null;
		SelectRandomActivity();

		var runner = GetNode<DialogueRunner>( "/root/Main/UserInterface/YarnSpinnerCanvasLayer/DialogueRunner" );
		runner.onDialogueComplete -= OnDialogueComplete;

		runner.RemoveCommandHandler( "AddRepPoints" );
		runner.RemoveCommandHandler( "SetFollowTarget" );
		runner.RemoveCommandHandler( "FollowPlayer" );
	}


	public void SetFollowTarget( Node3D node )
	{

		if ( node == null )
		{
			if ( FollowTarget is PlayerController oldPlayer )
			{
				oldPlayer.PlayerEnterArea -= FollowPlayerIntoNewArea;
			}

			FollowTarget = null;
			NpcManager.NpcInstanceData[GetData().NpcId].FollowTarget = null;
			Logger.Info( "Npc", $"Cleared follow target for {GetData().NpcId}" );
			return;
		}

		if ( node is not PlayerController player )
		{
			Logger.LogError( "Follow target is not a player" );
			return;
		}

		FollowTarget = player;

		NpcManager.NpcInstanceData[GetData().NpcId].FollowTarget = player;

		player.PlayerEnterArea += FollowPlayerIntoNewArea;

		Logger.Info( "Npc", $"Set follow target {player.Name} for {GetData().NpcId}" );

	}

	private void FollowPlayerIntoNewArea( string exit, string world, float pause )
	{
		var npcId = GetData().NpcId;
		NpcManager.NpcInstanceData[npcId].FollowTargetExit = exit;
		NpcManager.NpcInstanceData[npcId].WorldPath = world;
		NpcManager.NpcInstanceData[npcId].IsFollowing = true;
		Logger.Info( "Npc", $"Saved follow target {FollowTarget.Name} for {npcId} into {world} ({exit})" );
	}

	public void OnNetted( Net net )
	{
		Logger.Info( "Npc", "Netted" );
	}
}
