using System;
using Godot;
using vcrossing2.Code.Carriable;
using vcrossing2.Code.Dependencies;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Save;
using vcrossing2.Code.Ui;
using vcrossing2.Code.WorldBuilder;

namespace vcrossing2.Code.Player;

public partial class PlayerController : CharacterBody3D
{
	public const float WalkSpeed = 3.0f;
	public const float RunSpeed = 5.0f;
	public const float Friction = 5.0f;
	public const float Acceleration = 5f;
	public const float RotationSpeed = 7f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting( "physics/3d/default_gravity" ).AsSingle();

	public Vector3 WishVelocity;

	public PlayerSaveData SaveData { get; set; } = new();
	public string PlayerName => SaveData.PlayerName;
	public string PlayerId => SaveData.PlayerId;

	public string ExitName { get; set; }
	public string ExitWorld { get; set; }

	[Signal]
	public delegate void PlayerEnterAreaEventHandler( string exit, string world, float pause = 0f );


	public PlayerInteract Interact => GetNode<PlayerInteract>( "PlayerInteract" );
	public Inventory Inventory => GetNode<Inventory>( "PlayerInventory" );
	public Node3D Model => GetNode<Node3D>( "PlayerModel" );
	public WorldManager WorldManager => GetNode<WorldManager>( "/root/Main/WorldContainer" );
	public World World => WorldManager.ActiveWorld;

	[Export, Require] public Node3D Equip { get; set; }
	public BaseCarriable CurrentCarriable { get; set; }

	public bool InCutscene { get; set; }

	public bool ShouldDisableMovement()
	{
		// if ( DisableControlsToggle ) return true;
		// if ( CurrentGrabbedItem != null ) return true;
		if ( Interact.SittingNode != null ) return true;
		if ( Interact.LyingNode != null ) return true;
		if ( WorldManager.IsLoading ) return true;
		return false;
	}

	public override void _Ready()
	{
		base._Ready();
		Load();

		WorldManager.WorldLoaded += OnWorldLoaded;

		PlayerEnterArea += OnPlayerEnterArea;

		/*WorldManager.WorldChanged += () =>
		{
		};*/
	}

	private async void OnPlayerEnterArea( string exit, string world, float pause = 0f )
	{
		Logger.Info( "PlayerController", $"Player entered area {world} ({exit}), saving exit data" );
		ExitName = exit;
		ExitWorld = world;

		// start cutscene, player automatically walks forward
		InCutscene = true;

		if ( pause > 0 )
		{
			await ToSignal( GetTree().CreateTimer( pause ), SceneTreeTimer.SignalName.Timeout );
		}

		var fader = GetNode<Fader>( "/root/Main/UserInterface/Fade" );
		fader.FadeIn();

		// wait for the fade to complete
		await ToSignal( GetTree().CreateTimer( fader.FadeTime ), SceneTreeTimer.SignalName.Timeout );

		// delay loading the world to allow the player to walk for a second
		// await ToSignal( GetTree().CreateTimer( 1 ), SceneTreeTimer.SignalName.Timeout );

		// load the world
		var manager = GetNode<WorldManager>( "/root/Main/WorldContainer" );
		await manager.LoadWorld( world );

		Logger.Info( "AreaTrigger", "World loaded sync." );

		// wait for the physics frame to complete
		await ToSignal( GetTree(), SceneTree.SignalName.PhysicsFrame );

		// stop cutscene
		InCutscene = false;
		fader.FadeOut();
	}

	public void OnWorldLoaded( World world )
	{
		if ( string.IsNullOrEmpty( ExitName ) ) return;

		var node = world.FindChild( ExitName );
		if ( node == null )
		{
			throw new Exception( $"Exit node {ExitName} not found." );
		}

		if ( node is not AreaExit exit )
		{
			throw new Exception( $"Exit node {ExitName} is not a Node3D." );
		}

		Logger.Info( "Player", $"Entered area {ExitName}, moving to {exit.Name} @ {exit.Position}" );
		Position = exit.GlobalPosition;
		exit.OnExited();
	}

	public Vector2 InputDirection => Input.GetVector( "Left", "Right", "Up", "Down" );
	public Vector3 InputVector => (Transform.Basis * new Vector3( InputDirection.X, 0, InputDirection.Y )).Normalized();

	public override void _PhysicsProcess( double delta )
	{

		if ( InCutscene )
		{
			MoveAndSlide();
			return;
		}

		if ( ShouldDisableMovement() )
		{
			Velocity = Vector3.Zero;
			return;
		}

		Vector3 velocity = Velocity;

		// Add the gravity.
		if ( !IsOnFloor() )
			velocity.Y -= gravity * (float)delta;

		// player inputs
		// Vector2 inputDir = Input.GetVector( "Left", "Right", "Up", "Down" );
		// Vector3 direction = (Transform.Basis * new Vector3( inputDir.X, 0, inputDir.Y )).Normalized();

		var speed = Input.IsActionPressed( "Run" ) ? RunSpeed : WalkSpeed;

		if ( InputVector != Vector3.Zero )
		{
			// smoothly rotate the player model towards the direction
			var targetRotation = Mathf.Atan2( InputVector.X, InputVector.Z );
			var currentRotation = Model.Rotation.Y;
			var newRotation = Mathf.LerpAngle( currentRotation, targetRotation, (float)delta * RotationSpeed );
			newRotation = Mathf.Wrap( newRotation, -Mathf.Pi, Mathf.Pi );
			Model.Rotation = new Vector3( 0, newRotation, 0 );

			// velocity.X = direction.X * speed;
			// velocity.Z = direction.Z * speed;
			velocity.X = Mathf.MoveToward( Velocity.X, InputVector.X * speed, (float)delta * Acceleration );
			velocity.Z = Mathf.MoveToward( Velocity.Z, InputVector.Z * speed, (float)delta * Acceleration );
		}
		else
		{
			velocity.X = Mathf.MoveToward( Velocity.X, 0, (float)delta * Friction );
			velocity.Z = Mathf.MoveToward( Velocity.Z, 0, (float)delta * Friction );
		}

		// never allow velocity height to be positive, since player can't jump
		velocity.Y = Mathf.Min( 0, velocity.Y );

		Velocity = velocity;
		MoveAndSlide();
	}

	public void Save()
	{
		SaveData.AddPlayer( this );
		SaveData.SaveFile( "user://player.json" );
	}

	public void Load()
	{
		SaveData = new PlayerSaveData();
		if ( SaveData.LoadFile( "user://player.json" ) )
		{
			SaveData.LoadPlayer( this );
		}
	}
}
