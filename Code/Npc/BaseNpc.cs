using System;
using DialogueManagerRuntime;
using Godot;
using Godot.Collections;
using vcrossing2.Code.Dependencies;
using vcrossing2.Code.Dialogue;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;
using vcrossing2.Code.Save;

namespace vcrossing2.Code.Npc;

public partial class BaseNpc : CharacterBody3D, IUsable
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

	[Export] public Node3D CurrentInteractionTarget { get; set; }

	[Export] public Array<Resource> Dialogue { get; set; }

	protected WorldManager WorldManager => GetNode<WorldManager>( "/root/Main/WorldContainer" );
	protected NpcManager NpcManager => GetNode<NpcManager>( "/root/Main/NpcManager" );

	private NpcSaveData _saveData;

	public NpcSaveData SaveData
	{
		get
		{
			if ( NpcData == null ) throw new NullReferenceException( "NpcData is null" );
			if ( string.IsNullOrEmpty( GetData().NpcId ) ) throw new NullReferenceException( "NpcId is null" );
			return _saveData ??= NpcSaveData.Load( GetData().NpcId );
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
		Pose,
		SittingOrLying
	}

	public CurrentState State { get; set; }

	private float WaitingTime { get; set; }
	private float WalkTimeout { get; set; }

	public bool IsDisabled { get; set; }

	public SittableNode SittingNode { get; set; }
	public LyingNode LyingNode { get; set; }
	private Vector3 LastPosition { get; set; }

	public bool IsLyingOrSitting => SittingNode != null || LyingNode != null;

	public bool ShouldDisableMovement()
	{
		if ( IsDisabled ) return true;
		if ( IsLyingOrSitting ) return true;
		if ( WorldManager.IsLoading ) return true;
		return false;
	}

	public NpcData GetData()
	{
		if ( NpcData == null ) throw new NullReferenceException( "NpcData is null" );
		return GD.Load<NpcData>( NpcData );
	}

	public override async void _Ready()
	{
		base._Ready();

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

		SaveData = NpcSaveData.Load( GetData().NpcId );

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
				if ( node == null )
				{
					throw new Exception( $"Exit node {data.FollowTargetExit} not found." );
				}

				if ( node is not Node3D exit )
				{
					throw new Exception( $"Exit node {data.FollowTargetExit} is not a Node3D." );
				}

				Logger.Info( "Npc", $"Player entered area {data.FollowTargetExit}, following to {exit.Name} @ {exit.Position}" );

				GlobalPosition = exit.GlobalPosition;

				NpcManager.NpcInstanceData[npcId].FollowTargetExit = null;
			}
		} else {
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

	public void SetState( CurrentState state )
	{
		State = state;
	}

	public void LookAtNode( Node3D node )
	{
		var target = node.GlobalTransform.Origin;
		var position = GlobalTransform.Origin;
		var direction = target - position;
		if ( Model == null ) throw new NullReferenceException( "Model is null" );
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

		if ( IsDisabled ) return;

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
			CheckForBed();

			if ( !IsLyingOrSitting )
			{
				SetTargetPosition( FollowTarget.GlobalPosition );
			}
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

	public void LieInBed( PlacedItem bed )
	{
		if ( LyingNode != null )
		{
			Logger.LogError( "Already lying" );
			return;
		}

		var lyingNodes = bed.GetChildren().Where( c => c is LyingNode ).Cast<LyingNode>().ToList();

		var freeNode = lyingNodes.FirstOrDefault( n => n.Occupant == null );

		if ( freeNode != null )
		{
			Logger.Info( "Npc", "Lying node is free" );
			freeNode.Occupant = this;
			LyingNode = freeNode;

			LastPosition = GlobalPosition;

			SetState( CurrentState.SittingOrLying );
			GlobalPosition = freeNode.GlobalPosition;
			Model.Rotation = freeNode.GlobalRotation;
		}
		else
		{
			Logger.Warn( "Npc", "No free lying node" );
		}
	}

	public void GetUpFromBedOrSittable()
	{
		if ( LyingNode != null )
		{
			Logger.Info( "Npc", "Getting up" );
			LyingNode.Occupant = null;
			LyingNode = null;
			SetState( CurrentState.Idle );
			GlobalPosition = LastPosition;
		}
		else if ( SittingNode != null )
		{
			Logger.Info( "Npc", "Getting up" );
			SittingNode.Occupant = null;
			SittingNode = null;
			SetState( CurrentState.Idle );
			GlobalPosition = LastPosition;
		}
	}

	private void CheckForBed()
	{
		if ( FollowTarget is not PlayerController player )
		{
			Logger.LogError( "Follow target is not a player" );
			return;
		}

		var playerInteract = player.Interact;

		if ( playerInteract.LyingNode == null )
		{
			if ( LyingNode != null )
			{
				GetUpFromBedOrSittable();
			}

			return;
		}

		if ( LyingNode != null )
		{
			// GD.Print( "Lying node is free" );
			return;
		}

		var bed = playerInteract.LyingNode.GetParent();
		while ( bed != null )
		{
			if ( bed is PlacedItem b )
			{
				/*var lyingNodes = b.GetChildren().Where( c => c is LyingNode ).Cast<LyingNode>().ToList();

				var freeNode = lyingNodes.FirstOrDefault( n => n.Occupant == null );

				if ( freeNode != null )
				{
					Logger.Info( "Lying node is free" );
					// playerInteract.LyingNode.Occupant = null;
					// playerInteract.LyingNode = null;

					freeNode.Occupant = this;
					LyingNode = freeNode;

					LastPosition = GlobalPosition;

					SetState( CurrentState.SittingOrLying );
					GlobalPosition = freeNode.GlobalPosition;
					Model.Rotation = freeNode.GlobalRotation;
				}
				else
				{
					Logger.Info( "No free lying node" );
				}

				return;*/

				LieInBed( b );
				return;
			}

			bed = bed.GetParent();
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
		if ( IsLyingOrSitting ) return;

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

	private void FollowPlayerIntoNewArea( string exit, string world )
	{
		var npcId = GetData().NpcId;
		NpcManager.NpcInstanceData[npcId].FollowTargetExit = exit;
		NpcManager.NpcInstanceData[npcId].WorldPath = world;
		NpcManager.NpcInstanceData[npcId].IsFollowing = true;
		Logger.Info( "Npc", $"Saved follow target {FollowTarget.Name} for {npcId} into {world} ({exit})" );
	}
}
