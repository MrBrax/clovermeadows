﻿using System;
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

	private float Momentum = 0f;

	public Godot.Collections.Dictionary<Node3D, Node3D> Occupants { get; set; } = [];

	public Node3D HasDriver => Occupants.Keys.Contains( Seats[0] ) ? Occupants[Seats[0]] : null;

	public override void _Ready()
	{
		AddToGroup( "usables" );
	}

	public void AddOccupant( int seatIndex, Node3D occupant )
	{
		var seat = Seats[seatIndex];

		if ( Occupants.Keys.Contains( seat ) )
		{
			throw new System.Exception( "Seat already occupied" );
		}

		Occupants[seat] = occupant;
		Logger.Info( "BaseVehicle", $"Added occupant to seat {seatIndex}" );

		// Set the occupant's position to the seat's position
		occupant.GlobalPosition = seat.GlobalPosition;
		if ( occupant is PlayerController player )
		{
			player.Vehicle = this;
			player.Model.RotationDegrees = seat.GlobalRotationDegrees;
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
		// velocity = velocity.Lerp( Vector3.Zero, Deceleration * (float)delta ).WithY( velocity.Y );


		// steering
		var steering = InputDirection.X;
		if ( steering > 0 )
		{
			RotationDegrees -= new Vector3( 0, Steering, 0 );
		}
		else if ( steering < 0 )
		{
			RotationDegrees += new Vector3( 0, Steering, 0 );
		}

		// acceleration
		var acceleration = InputDirection.Y;
		if ( acceleration > 0 )
		{
			// velocity += GlobalTransform.Basis.Z.Normalized() * Acceleration * (float)delta;
			Momentum += Acceleration * (float)delta;
		}
		else if ( acceleration < 0 )
		{
			// velocity -= GlobalTransform.Basis.Z.Normalized() * Acceleration * (float)delta;
			Momentum -= Acceleration * (float)delta;
		}

		// apply momentum
		velocity = GlobalTransform.Basis.Z.Normalized() * Momentum;

		// apply drag
		Momentum = Mathf.Lerp( Momentum, 0, (float)delta * Deceleration );

		// clamp speed
		velocity = velocity.Clamp( -MaxSpeed, MaxSpeed );

		// rotate velocity to car's rotation
		// velocity = velocity.Rotated( Vector3.Up, RotationDegrees.Y );


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
					player.Model.RotationDegrees = seat.GlobalRotationDegrees;
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
