using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

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
		BuildWishVelocity( delta );

		if ( IsOnFloor() )
		{
			Accelerate( WishVelocity, delta );
			ApplyFriction( 4.0f, delta );
		}
		else
		{
			Velocity -= new Vector3( 0, gravity * (float)delta, 0 );
			Accelerate( WishVelocity.LimitLength( 50 ), delta );
			ApplyFriction( 0.1f, delta );
		}

		MoveAndSlide();

		// why is this duplicated?
		if ( !IsOnFloor() )
		{
			Velocity -= new Vector3( 0, gravity * (float)delta, 0 );
		}
		else
		{
			Velocity = new Vector3( Velocity.X, 0, Velocity.Z );
		}

		/*Vector3 velocity = Velocity;

		// Add the gravity.
		if ( !IsOnFloor() )
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		// if ( Input.IsActionJustPressed( "ui_accept" ) && IsOnFloor() )
		// 	velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector( "Left", "Right", "Up", "Down" );
		Vector3 direction = (Transform.Basis * new Vector3( inputDir.X, 0, inputDir.Y )).Normalized();
		if ( direction != Vector3.Zero )
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward( Velocity.X, 0, Speed );
			velocity.Z = Mathf.MoveToward( Velocity.Z, 0, Speed );
		}

		Velocity = velocity;
		MoveAndSlide();

		// rotate the player model node to the input direction
		var model = GetNode<Node3D>( "PlayerModel" );
		if ( direction != Vector3.Zero )
			model.Rotation = new Vector3( 0, Mathf.Atan2( direction.X, direction.Z ), 0 );*/
	}

	public void BuildWishVelocity( double delta )
	{
		var inputDir = Input.GetVector( "Left", "Right", "Up", "Down" );
		var direction = (Transform.Basis * new Vector3( inputDir.X, 0, inputDir.Y )).Normalized();
		var model = GetNode<Node3D>( "PlayerModel" );

		if ( direction != Vector3.Zero )
		{
			var newTransform = Transform.LookingAt( direction, Vector3.Up );
			model.Quaternion = model.Quaternion.Slerp( newTransform.Basis.GetRotationQuaternion(), 0.1f );

			var forward = model.Quaternion.GetAxis();
			WishVelocity = forward.Normalized();
		}
		else
		{
			WishVelocity = Vector3.Zero;
		}
		
		WishVelocity *= Speed;
		
	}
	
	private void ApplyFriction( float friction, double delta )
	{
		var speed = Velocity.Length();
		if ( speed < 0.1f )
		{
			Velocity = Vector3.Zero;
			return;
		}

		var drop = speed * friction * (float)delta;
		Velocity *= Mathf.Max( speed - drop, 0 ) / speed;
	}
	
	public void Accelerate( Vector3 wishDir, double delta )
	{
		var currentSpeed = Velocity.Dot( wishDir );
		var addSpeed = Speed - currentSpeed;
		if ( addSpeed <= 0 )
			return;

		double accelSpeed = 5.0f * delta;
		if ( accelSpeed > addSpeed )
			accelSpeed = addSpeed;

		Velocity += wishDir * (float)accelSpeed;
	}
}
