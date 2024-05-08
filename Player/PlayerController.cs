using Godot;
using System;
using vcrossing.Save;

namespace vcrossing.Player;

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

	public bool ShouldDisableMovement()
	{
		// if ( DisableControlsToggle ) return true;
		// if ( CurrentGrabbedItem != null ) return true;
		return false;
	}

	public override void _Ready()
	{
		base._Ready();
		Load();
	}

	public override void _PhysicsProcess( double delta )
	{
		Vector3 velocity = Velocity;
		var playerModel = GetNode<Node3D>( "PlayerModel" );

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
			var currentRotation = playerModel.Rotation.Y;
			var newRotation = Mathf.LerpAngle( currentRotation, targetRotation, (float)delta * RotationSpeed );
			newRotation = Mathf.Wrap( newRotation, -Mathf.Pi, Mathf.Pi );
			playerModel.Rotation = new Vector3( 0, newRotation, 0 );

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
		playerSave.LoadFile( "user://player.json" );
		playerSave.LoadPlayer( this );
	}
}
