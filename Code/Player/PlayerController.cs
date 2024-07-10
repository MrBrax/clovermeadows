using System;
using Godot;
using vcrossing.Code.Carriable;
using vcrossing.Code.Components;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Helpers;
using vcrossing.Code.Inventory;
using vcrossing.Code.Save;
using vcrossing.Code.Ui;
using vcrossing.Code.Vehicles;
using vcrossing.Code.WorldBuilder;
using YarnSpinnerGodot;

namespace vcrossing.Code.Player;

public partial class PlayerController : CharacterBody3D
{
	public const float WalkSpeed = 3.0f;
	public const float RunSpeed = 5.0f;
	public const float SneakSpeed = 1.0f;
	public const float Friction = 5.0f;
	public const float Acceleration = 8f;
	public const float RotationSpeed = 7f;

	[Export] public Node3D DefaultCamera { get; set; }
	[Export] public Node3D LookUpCamera { get; set; }

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

	[Signal]
	public delegate void PlayerCloversChangedEventHandler( int oldAmount, int newAmount );


	public PlayerInteract Interact => GetNode<PlayerInteract>( "PlayerInteract" );
	public Components.Inventory Inventory => GetNode<Components.Inventory>( "PlayerInventory" );
	public Components.Equips Equips => GetNode<Components.Equips>( "PlayerEquips" );

	public Node3D Model => GetNode<Node3D>( "Smoothing/PlayerModel" );
	public WorldManager WorldManager => GetNode<WorldManager>( "/root/Main/WorldManager" );
	public World World => WorldManager.ActiveWorld;

	// [Export, Require] public Node3D Equip { get; set; }
	// public BaseCarriable CurrentCarriable { get; set; }

	public int Clovers { get; private set; }

	[Export, Require] public Node3D ToolEquip { get; set; }

	[Export] public AnimationPlayer AnimationPlayer { get; set; }

	public BaseVehicle Vehicle { get; set; }
	public bool IsInVehicle => Vehicle != null && IsInstanceValid( Vehicle );

	public bool InCutscene { get; set; }

	public Vector3 CutsceneTarget { get; set; }

	public Vector3 AimDirection => Model.GlobalTransform.Basis.Z;

	public bool ShouldDisableMovement()
	{
		// if ( DisableControlsToggle ) return true;
		// if ( CurrentGrabbedItem != null ) return true;
		if ( Interact.SittingNode != null ) return true;
		if ( Interact.LyingNode != null ) return true;
		if ( WorldManager.IsLoading ) return true;
		if ( Equips.HasEquippedItem( Equips.EquipSlot.Tool ) && Equips.GetEquippedItem<BaseCarriable>( Equips.EquipSlot.Tool ).ShouldDisableMovement() ) return true;
		// if ( IsInVehicle ) return true;
		if ( InCutscene ) return true;
		if ( GetNode<UserInterface>( "/root/Main/UserInterface" ).IsPaused ) return true;
		if ( GetNode<UserInterface>( "/root/Main/UserInterface" ).AreWindowsOpen ) return true;

		var runner = GetNode<DialogueRunner>( "/root/Main/UserInterface/YarnSpinnerCanvasLayer/DialogueRunner" );
		if ( runner.IsDialogueRunning ) return true;

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

	private bool _isEnteringArea = false;

	private async void OnPlayerEnterArea( string exit, string world, float pause = 0f )
	{

		if ( _isEnteringArea )
		{
			Logger.Warn( "PlayerController", "Already entering area." );
			return;
		}

		_isEnteringArea = true;

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
		await fader.FadeIn();

		// wait for the fade to complete
		// await ToSignal( GetTree().CreateTimer( fader.FadeTime ), SceneTreeTimer.SignalName.Timeout );

		// delay loading the world to allow the player to walk for a second
		// await ToSignal( GetTree().CreateTimer( 1 ), SceneTreeTimer.SignalName.Timeout );

		// load the world
		var manager = GetNode<WorldManager>( "/root/Main/WorldManager" );
		await manager.LoadWorld( world );

		Logger.Info( "AreaTrigger", "World loaded sync." );

		// wait for the physics frame to complete
		await ToSignal( GetTree(), SceneTree.SignalName.PhysicsFrame );

		await fader.FadeOut();

		// stop cutscene
		InCutscene = false;

		_isEnteringArea = false;
	}

	public void OnWorldLoaded( World world )
	{
		if ( string.IsNullOrEmpty( ExitName ) ) return;

		var rawExitNode = world.FindChild( ExitName );
		if ( !IsInstanceValid( rawExitNode ) )
		{
			throw new Exception( $"Exit node {ExitName} not found." );
		}

		if ( rawExitNode is not AreaExit exitNode )
		{
			throw new Exception( $"Exit node {ExitName} is not a Node3D." );
		}

		Logger.Info( "Player", $"Entered area {ExitName}, moving to {exitNode.Name} @ {exitNode.GlobalPosition} ({exitNode.Basis.Z})" );
		Position = exitNode.GlobalPosition;
		// Velocity = exitNode.Basis.Z * 4;
		Model.GlobalRotation = exitNode.GlobalRotation;
		exitNode.OnExited();

		InCutscene = true;
		CutsceneTarget = exitNode.GlobalPosition + exitNode.Basis.Z * 2;

		ToSignal( GetTree().CreateTimer( 1 ), SceneTreeTimer.SignalName.Timeout ).OnCompleted( () =>
		{
			InCutscene = false;
			CutsceneTarget = Vector3.Zero;
		} );

	}

	/* public Vector2 InputDirection
	{
		get
		{
			// default to keyboard/controller input
			var vec = Input.GetVector( "Left", "Right", "Up", "Down" );
			vec = TouchControl( vec );
			return vec;
		}
	} */

	public Vector2 InputDirection;

	/* private Vector2 TouchControl( Vector2 vec )
	{
		// if no input, check for touch input
		// move player towards touch position if mouse is pressed, but only if the player is not too close to the mouse
		// this is to prevent the player from moving when clicking on nearby objects
		// speed is based on the distance between the player and the mouse
		if (
			vec == Vector2.Zero &&
			Input.IsMouseButtonPressed( MouseButton.Left ) &&
			GetNode<SettingsSaveData>( "/root/SettingsSaveData" ).CurrentSettings.PlayerMouseControl &&
			!GetNode<UserInterface>( "/root/Main/UserInterface" ).IsPaused

		)
		{
			// get mouse position
			var mousePosition = GetViewport().GetMousePosition();

			// trace a ray from the camera to the mouse position
			var spaceState = GetWorld3D().DirectSpaceState;

			var camera = GetTree().GetNodesInGroup( "camera" )[0] as Camera3D;

			var from = camera.ProjectRayOrigin( mousePosition );
			var to = from + camera.ProjectRayNormal( mousePosition ) * 1000;

			var result = new Trace( spaceState ).CastRay( PhysicsRayQueryParameters3D.Create( from, to, World.TerrainLayer ) );

			if ( result == null ) return vec;

			// get the hit position
			var hitPosition = result.Position;

			// get the player position
			var playerPosition = GlobalTransform.Origin;

			// get the distance between the player and the hit position
			var distance = playerPosition.DistanceTo( hitPosition );

			// don't move if the mouse is too close to the player
			if ( distance < 0.5f ) return vec;

			// get the direction from the player to the hit position
			var direction = (hitPosition - playerPosition).Normalized();

			// calculate the speed based on the distance
			var speed = Mathf.Clamp( (distance - 0.5f) * 0.5f, 0f, 2f );

			// move the player towards the hit position
			return new Vector2( direction.X * speed, direction.Z * speed );

			// Logger.Info( "Player", $"Moving player towards mouse position {hitPosition} at speed {speed} ({vec.X}, {vec.Y})" );

		}

		return vec;
	}
 */
	public Vector3 InputVector => new Vector3( InputDirection.X, 0, InputDirection.Y );


	public override void _PhysicsProcess( double delta )
	{

		if ( IsInVehicle )
		{
			return;
		}

		Animate();

		if ( InCutscene )
		{
			Velocity = ApplyGravity( delta, Velocity );

			if ( CutsceneTarget != Vector3.Zero )
			{
				// var y = Velocity.Y;
				var target = CutsceneTarget;
				var direction = (target - GlobalPosition).Normalized();
				// Velocity = direction * 2;
				// direction.Y = y;

				Velocity = new Vector3( direction.X * 2, Velocity.Y, direction.Z * 2 );

				if ( (target - GlobalPosition).Length() < 0.1f )
				{
					CutsceneTarget = Vector3.Zero;
					Velocity = Vector3.Zero;
				}
			}

			MoveAndSlide();
			return;
		}

		if ( ShouldDisableMovement() )
		{
			Velocity = Vector3.Zero;
			return;
		}

		Vector3 velocity = Velocity;
		velocity = ApplyGravity( delta, velocity );

		// player inputs
		// Vector2 inputDir = Input.GetVector( "Left", "Right", "Up", "Down" );
		// Vector3 direction = (Transform.Basis * new Vector3( inputDir.X, 0, inputDir.Y )).Normalized();

		// var speed = Input.IsActionPressed( "Run" ) ? RunSpeed : WalkSpeed;
		float speed = GetSpeedInput();

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

	private float GetSpeedInput()
	{
		var speed = WalkSpeed;
		if ( Input.IsActionPressed( "Run" ) )
		{
			speed = RunSpeed;
		}
		else if ( Input.IsActionPressed( "Sneak" ) ) // only for keyboard
		{
			speed = SneakSpeed;
		}

		if ( Equips.TryGetEquippedItem<BaseCarriable>( Equips.EquipSlot.Tool, out var tool ) )
		{
			speed *= tool.CustomPlayerSpeed();
		}

		return speed;
	}

	private void Animate()
	{
		if ( InputVector != Vector3.Zero )
		{
			AnimationPlayer.Play( "walk" );
			AnimationPlayer.SpeedScale = (Velocity.Length() / RunSpeed) * 2f;
		}
		else
		{
			AnimationPlayer.Play( "RESET" );
		}
	}


	private Vector3 ApplyGravity( double delta, Vector3 velocity )
	{
		// Add the gravity.
		if ( !IsOnFloor() )
			velocity.Y -= gravity * (float)delta;
		return velocity;
	}

	public void Save()
	{
		SaveData.AddPlayer( this );
		SaveData.SaveFile( "user://player.json" );
	}

	public void Load()
	{
		SaveData = PlayerSaveData.LoadFile( "user://player.json" );
		if ( SaveData != null )
		{
			SaveData.LoadPlayer( this );
		}
		else
		{
			Logger.Warn( "Player save data not found, creating new save data" );
			SaveData = new PlayerSaveData();
			Save();
		}
	}

	public void SetCollisionEnabled( bool enabled )
	{
		GetNode<CollisionShape3D>( "CollisionShape3D" ).Disabled = !enabled;
	}

	public void SetCarriableVisibility( bool visible )
	{
		/* if ( CurrentCarriable != null )
		{
			CurrentCarriable.Visible = visible;
		} */

		var carriable = Equips.GetEquippedItem( Equips.EquipSlot.Tool );
		if ( carriable != null )
		{
			carriable.Visible = visible;
		}
	}

	public void ModelLookAt( Vector3 globalPosition )
	{
		// Model.LookAt( globalPosition, Vector3.Up );
		// Model.Rotation = new Vector3( 0, Model.Rotation.Y, 0 );
		var position = GlobalTransform.Origin;
		var direction = globalPosition - position;
		if ( !IsInstanceValid( Model ) ) throw new NullReferenceException( "Model is null" );
		Model.GlobalTransform = Model.GlobalTransform.LookingAt( position - direction, Vector3.Up );
	}

	public bool CanAfford( int cost )
	{
		return Clovers >= cost;
	}

	public void SpendClovers( int cost )
	{
		if ( !CanAfford( cost ) ) throw new Exception( $"Player can't afford {cost} clovers" );
		var oldClovers = Clovers;
		Clovers -= cost;
		EmitSignal( SignalName.PlayerCloversChanged, oldClovers, Clovers );
	}

	public void AddClovers( int amount )
	{
		var oldClovers = Clovers;
		Clovers += amount;
		EmitSignal( SignalName.PlayerCloversChanged, oldClovers, Clovers );
	}

	public void SetClovers( int amount )
	{
		var oldClovers = Clovers;
		Clovers = amount;
		EmitSignal( SignalName.PlayerCloversChanged, oldClovers, Clovers );
	}

	public override void _UnhandledInput( InputEvent @event )
	{
		base._UnhandledInput( @event );

		if ( ShouldDisableMovement() ) return;

		if ( IsInVehicle )
		{
			Vehicle.HandleInput( @event );
			return;
		}

		var vec = Input.GetVector( "Left", "Right", "Up", "Down" );

		if ( vec != Vector2.Zero )
		{
			InputDirection = vec;
		}
		else
		{
			InputDirection = default;
		}

		if ( @event.IsActionPressed( "LookUp" ) )
		{
			if ( LookUpCamera.Get( "priority" ).AsInt32() == 10 )
			{
				DefaultCamera.Set( "priority", 10 );
				LookUpCamera.Set( "priority", 0 );
			}
			else
			{
				DefaultCamera.Set( "priority", 0 );
				LookUpCamera.Set( "priority", 10 );
			}
		}

	}

}
