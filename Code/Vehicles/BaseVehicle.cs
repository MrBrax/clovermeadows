using System;
using Godot;
using Godot.Collections;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Vehicles;

public partial class BaseVehicle : CharacterBody3D, IUsable
{

	[Export] public float MaxSpeed { get; set; } = 5;
	[Export] public float Acceleration { get; set; } = 0.5f;
	[Export] public float Deceleration { get; set; } = 0.5f;
	[Export] public float Steering { get; set; } = 0.5f;
	[Export] public Array<Node3D> Seats { get; set; } = [];

	private float Gravity = 9.8f;

	private float CarRotation = 0;

	public Godot.Collections.Dictionary<Node3D, Node3D> Occupants { get; set; } = [];

	public Node3D HasDriver => Occupants.Keys.Contains( Seats[0] ) ? Occupants[Seats[0]] : null;

	public override void _Ready()
	{
		AddToGroup( "usables" );
	}

	public void AddOccupant( int seatIndex, Node3D occupant )
	{
		if ( Occupants.Keys.Contains( Seats[seatIndex] ) )
		{
			throw new System.Exception( "Seat already occupied" );
		}

		Occupants[Seats[seatIndex]] = occupant;
		Logger.Info( "BaseVehicle", $"Added occupant to seat {seatIndex}" );

		// Set the occupant's position to the seat's position
		occupant.GlobalPosition = Seats[seatIndex].GlobalPosition;
		if ( occupant is PlayerController player )
		{
			player.Vehicle = this;
			player.Model.RotationDegrees = RotationDegrees;
			player.SetCollisionEnabled( false );
		}
	}

	public void RemoveOccupant( Node3D occupant )
	{
		if ( !Occupants.Values.Contains( occupant ) )
		{
			throw new System.Exception( "Occupant not found" );
		}

		var seat = Occupants.First( x => x.Value == occupant ).Key;
		Occupants.Remove( seat );
		Logger.Info( "BaseVehicle", $"Removed occupant from seat" );

		if ( occupant is PlayerController player )
		{
			player.Vehicle = null;
			player.SetCollisionEnabled( true );
		}
	}

	public bool CanUse( PlayerController player )
	{
		return true;
	}

	public void OnUse( PlayerController player )
	{
		if ( !CanUse( player ) )
		{
			return;
		}

		if ( !Occupants.Keys.Contains( Seats[0] ) )
		{
			AddOccupant( 0, player );
		}
	}

	public override void _Process( double delta )
	{
		HandleOccupants();
		Handling( delta );
	}

	public Vector2 InputDirection => Input.GetVector( "Left", "Right", "Up", "Down" );

	private Vector3 ApplyGravity( double delta, Vector3 velocity )
	{
		// Add the gravity.
		if ( !IsOnFloor() )
		{
			velocity.Y -= Gravity * (float)delta;
		}
		return velocity;
	}

	private void Handling( double delta )
	{

		var velocity = Velocity;

		// gravity
		velocity = ApplyGravity( delta, velocity );

		// no driver
		if ( !Occupants.Keys.Contains( Seats[0] ) )
		{
			velocity = velocity.Lerp( Vector3.Zero, Deceleration * (float)delta ).WithY( velocity.Y );
			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		// check if local player is driving
		var driver = Occupants[Seats[0]];
		if ( driver is not PlayerController player )
		{
			velocity = velocity.Lerp( Vector3.Zero, Deceleration * (float)delta ).WithY( velocity.Y );
			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		// apply deceleration
		velocity = velocity.Lerp( Vector3.Zero, Deceleration * (float)delta ).WithY( velocity.Y );

		var rotationDegrees = CarRotation;
		var carForward = new Vector3( Mathf.Sin( Mathf.DegToRad( rotationDegrees ) ), 0, Mathf.Cos( Mathf.DegToRad( rotationDegrees ) ) );

		var gas = InputDirection.Y;
		var steer = InputDirection.X;

		// apply acceleration
		if ( gas > 0 )
		{
			velocity += carForward * Acceleration * (float)delta;
		}
		else if ( gas < 0 )
		{
			velocity -= carForward * Acceleration * (float)delta;
		}

		// apply steering, rotating velocity
		if ( steer != 0 )
		{
			var angle = Mathf.DegToRad( -steer * Steering * (float)delta );
			// Rotation += new Vector3( 0, angle, 0 );
			velocity = velocity.Rotated( Vector3.Up, angle );
		}

		// rotate the model
		var targetRotation = Mathf.Atan2( velocity.X, velocity.Z );
		var currentRotation = Rotation.Y;
		var newRotation = Mathf.LerpAngle( currentRotation, targetRotation, (float)delta * 10 );
		newRotation = Mathf.Wrap( newRotation, -Mathf.Pi, Mathf.Pi );
		Rotation = new Vector3( 0, newRotation, 0 );

		// clamp speed
		velocity = velocity.Clamp( -MaxSpeed, MaxSpeed );




		// move
		Velocity = velocity;
		MoveAndSlide();

	}

	private void HandleOccupants()
	{
		foreach ( var seat in Seats )
		{
			if ( Occupants.Keys.Contains( seat ) )
			{
				var occupant = Occupants[seat];
				if ( !IsInstanceValid( occupant ) )
				{
					Occupants.Remove( seat );
					continue;
				}

				occupant.GlobalPosition = seat.GlobalPosition;
				if ( occupant is PlayerController player )
				{
					player.Model.RotationDegrees = RotationDegrees;
				}
			}
		}
	}

	/* public override void _Process( double delta )
	{
		base._Process( delta );
		
		// Move the vehicle forward
		var forward = GlobalTransform.Basis.Z.Normalized();
		GlobalPosition += forward * -Speed * (float)delta;
		
		if ( GlobalPosition.X > 128 )
		{
			GlobalPosition = new Vector3( -10f, GlobalPosition.Y, GlobalPosition.Z );
		} else if ( GlobalPosition.X < -10 )
		{
			GlobalPosition = new Vector3( 128, GlobalPosition.Y, GlobalPosition.Z );
		}
	} */
}
