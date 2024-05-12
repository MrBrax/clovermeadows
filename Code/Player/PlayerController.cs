using System;
using Godot;
using vcrossing2.Code.Carriable;
using vcrossing2.Code.Save;

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

	public string ExitName { get; set; }
	
	public PlayerInteract Interact => GetNode<PlayerInteract>( "PlayerInteract" );
	public Inventory Inventory => GetNode<Inventory>( "PlayerInventory" );
	public Node3D Model => GetNode<Node3D>( "PlayerModel" );
	public World World => GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;
	
	[Export] public Node3D Equip { get; set; }
	public BaseCarriable CurrentCarriable { get; set; }

	public bool ShouldDisableMovement()
	{
		// if ( DisableControlsToggle ) return true;
		// if ( CurrentGrabbedItem != null ) return true;
		if ( Interact.SittingNode != null ) return true;
		if ( Interact.LyingNode != null ) return true;
		return false;
	}

	public override void _Ready()
	{
		base._Ready();
		Load();
		
		GetNode<WorldManager>( "/root/Main/WorldContainer" ).WorldLoaded += world =>
		{
			OnAreaEntered();
		};
	}

	public void OnAreaEntered()
	{
		if ( string.IsNullOrEmpty( ExitName ) ) return;
		
		var node = World.FindChild( ExitName );
		if ( node == null )
		{
			throw new Exception( $"Exit node {ExitName} not found." );
			return;
		}
		
		if ( node is not Node3D exit )
		{
			throw new Exception( $"Exit node {ExitName} is not a Node3D." );
			return;
		}
		
		GD.Print( $"Player entered area {ExitName}, moving to {exit.Name} @ {exit.Position}" );
		Position = exit.GlobalPosition;
	}

	public override void _PhysicsProcess( double delta )
	{

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
		Vector2 inputDir = Input.GetVector( "Left", "Right", "Up", "Down" );
		Vector3 direction = (Transform.Basis * new Vector3( inputDir.X, 0, inputDir.Y )).Normalized();

		var speed = Input.IsActionPressed( "Run" ) ? RunSpeed : WalkSpeed;

		if ( direction != Vector3.Zero )
		{
			// smoothly rotate the player model towards the direction
			var targetRotation = Mathf.Atan2( direction.X, direction.Z );
			var currentRotation = Model.Rotation.Y;
			var newRotation = Mathf.LerpAngle( currentRotation, targetRotation, (float)delta * RotationSpeed );
			newRotation = Mathf.Wrap( newRotation, -Mathf.Pi, Mathf.Pi );
			Model.Rotation = new Vector3( 0, newRotation, 0 );

			// velocity.X = direction.X * speed;
			// velocity.Z = direction.Z * speed;
			velocity.X = Mathf.MoveToward( Velocity.X, direction.X * speed, (float)delta * Acceleration );
			velocity.Z = Mathf.MoveToward( Velocity.Z, direction.Z * speed, (float)delta * Acceleration );
		}
		else
		{
			velocity.X = Mathf.MoveToward( Velocity.X, 0, (float)delta * Friction );
			velocity.Z = Mathf.MoveToward( Velocity.Z, 0, (float)delta * Friction );
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void Save()
	{
		var playerSave = new PlayerSaveData();
		playerSave.AddPlayer( this );
		playerSave.SaveFile( "user://player.json" );
	}

	public void Load()
	{
		var playerSave = new PlayerSaveData();
		if ( playerSave.LoadFile( "user://player.json" ) )
		{
			playerSave.LoadPlayer( this );
		}
	}
}
