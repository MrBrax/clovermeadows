using System;
using Godot;
using Godot.Collections;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Items;
using vcrossing.Code.Player;

namespace vcrossing.Code.Vehicles;

public partial class BaseVehicle : CharacterBody3D, IUsable
{

	[Export, Require] public Node3D Model { get; set; }
	[Export] public Array<Node3D> Headlights { get; set; } = [];
	[Export] public Array<Node3D> Seats { get; set; } = [];
	[Export] public float MaxSpeed { get; set; } = 5;
	[Export] public float Acceleration { get; set; } = 0.5f;
	[Export] public float Deceleration { get; set; } = 0.5f;
	[Export] public float Steering { get; set; } = 0.5f;

	private float Gravity = 9.8f;

	private float Momentum = 0f;

	private bool _isOn;

	public Godot.Collections.Dictionary<Node3D, Node3D> Occupants { get; set; } = [];

	public bool HasDriver => Occupants.Keys.Contains( Seats[0] );

	private float _lastAction;

	public override void _Ready()
	{
		AddToGroup( "usables" );

		foreach ( var light in Headlights )
		{
			light.Hide();
		}
	}

	private void Start()
	{
		if ( _isOn ) return;

		var engine = GetNode<AudioStreamPlayer3D>( "Engine" );
		// engine.VolumeDb = 0f;
		engine.PitchScale = 0.01f;
		engine.Play();

		// Fade in the engine sound
		Tween tween = GetTree().CreateTween();
		// tween.TweenProperty( engine, "volume_db", -10, 1f );
		tween.TweenProperty( engine, "pitch_scale", 0.5f, 1f );

		GetNode<AudioStreamPlayer3D>( "Kick" ).Play();
		_isOn = true;
		_lastAction = Time.GetTicksMsec();

		foreach ( var light in Headlights )
		{
			light.Show();
		}
	}

	private void Stop()
	{
		if ( !_isOn ) return;

		// GetNode<AudioStreamPlayer3D>( "Engine" ).Stop();
		var engine = GetNode<AudioStreamPlayer3D>( "Engine" );
		_isOn = false;

		// Fade out the engine sound
		Tween tween = GetTree().CreateTween();
		// tween.TweenProperty( engine, "volume_db", 0, 1f );
		tween.TweenProperty( engine, "pitch_scale", 0.01f, 1f );
		tween.TweenCallback( Callable.From( () => engine.Stop() ) );

		foreach ( var light in Headlights )
		{
			light.Hide();
		}
	}

	private void HandleSound()
	{
		if ( !_isOn ) return;
		if ( Time.GetTicksMsec() - _lastAction > 1000f )
		{
			GetNode<AudioStreamPlayer3D>( "Engine" ).PitchScale = Mathf.Lerp( 0.5f, 1.5f, Mathf.Abs( Momentum / MaxSpeed ) );
		}
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
			player.SetCarriableVisibility( false );
		}

		if ( seatIndex == 0 && !_isOn )
		{
			Start();
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
			player.SetCarriableVisibility( true );
		}

		if ( seat == Seats[0] && _isOn )
		{
			Stop();
		}
	}

	public void TryToEjectOccupant( Node3D occupant )
	{
		if ( !Occupants.Values.Contains( occupant ) )
		{
			return;
		}

		var exitPosition = FindExitPosition( occupant );
		if ( exitPosition == Vector3.Zero )
		{
			Logger.Warn( "BaseVehicle", "Exit position not found" );
			return;
		}

		RemoveOccupant( occupant );

		occupant.GlobalPosition = exitPosition;

	}

	public Vector3 FindExitPosition( Node3D occupant )
	{

		var exitPos = GlobalTransform.Origin + GlobalTransform.Basis.X * 2;

		var spaceState = GetWorld3D().DirectSpaceState;
		var query = new PhysicsShapeQueryParameters3D();
		var shape = new SphereShape3D();
		shape.Radius = 1;
		query.Shape = shape;
		query.Exclude.Add( this.GetRid() );
		// query.Transform = GlobalTransform;
		// query.Transform.Origin = GlobalTransform.Origin + GlobalTransform.Basis.Y * 2;
		var transform = new Transform3D();
		transform.Origin = exitPos + Vector3.Up * 1;
		query.Transform = transform;

		// GetTree().CallGroup( "debugdraw", "add_sphere", exitPos + Vector3.Up * 1, 1, new Color( 1, 0, 0 ) );

		var trace = new Trace( spaceState ).CastShape( query );

		if ( trace.Count() > 0 )
		{
			Logger.Info( "BaseVehicle", $"Results ({trace.Count()})" );
			foreach ( var t in trace )
			{
				t.DebugPrint();
			}
			// Logger.Warn( "BaseVehicle", "Exit position blocked" );
			return Vector3.Zero;
		}

		return exitPos;
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
		HandleSound();

		/* if ( _isOn && HasDriver && Input.IsActionJustPressed( "Interact" ) )
		{
			TryToEjectOccupant( Occupants[Seats[0]] );
		} */
	}

	public override void _Input( InputEvent @event )
	{
		base._Input( @event );

		if ( @event is InputEventKey keyEvent )
		{
			if ( keyEvent.IsActionPressed( "Interact" ) )
			{
				if ( HasDriver )
				{
					TryToEjectOccupant( Occupants[Seats[0]] );
				}

			}
		}
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

		// align the model with the ground
		var spaceState = GetWorld3D().DirectSpaceState;
		var trace =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( GlobalPosition + Vector3.Up * 1, GlobalPosition + Vector3.Down * 1, World.TerrainLayer ) );

		if ( trace != null )
		{
			Model.RotationDegrees = Model.RotationDegrees.Lerp( new Vector3( trace.Normal.X, Model.RotationDegrees.Y, trace.Normal.Z ), 0.1f );
		}


		// steering
		var steering = InputDirection.X;
		if ( steering > 0 )
		{
			Model.RotationDegrees -= new Vector3( 0, Steering, 0 );
		}
		else if ( steering < 0 )
		{
			Model.RotationDegrees += new Vector3( 0, Steering, 0 );
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
		velocity = (Model.GlobalTransform.Basis.Z.Normalized() * Momentum).WithY( velocity.Y );

		// apply drag
		Momentum = Mathf.Lerp( Momentum, 0, (float)delta * Deceleration );

		// clamp speed
		velocity = velocity.Clamp( -MaxSpeed, MaxSpeed );

		// rotate velocity to car's rotation
		// velocity = velocity.Rotated( Vector3.Up, RotationDegrees.Y );

		// don't jump
		velocity.Y = Mathf.Min( 0, velocity.Y );

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
