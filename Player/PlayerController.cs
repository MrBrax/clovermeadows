using Godot;
using System;
using vcrossing.Save;

namespace vcrossing.Player;

public partial class PlayerController : CharacterBody3D
{
	public const float WalkSpeed = 3.0f;
	public const float RunSpeed = 5.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting( "physics/3d/default_gravity" ).AsSingle();

	public Vector3 WishVelocity;

	public bool ShouldDisableMovement()
	{
		// if ( DisableControlsToggle ) return true;
		// if ( CurrentGrabbedItem != null ) return true;
		return false;
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

		// smoothly rotate the player model towards the direction
		if ( direction.Length() > 0 )
			playerModel.LookAt( Transform.Origin - direction, Vector3.Up );
		
		var speed = Input.IsActionPressed( "Run" ) ? RunSpeed : WalkSpeed;
		
		if ( direction != Vector3.Zero )
		{
			velocity.X = direction.X * speed;
			velocity.Z = direction.Z * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward( Velocity.X, 0, speed );
			velocity.Z = Mathf.MoveToward( Velocity.Z, 0, speed );
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
}
