using Godot;
using vcrossing.Code.Carriable;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Objects;

public partial class Pellet : Node3D
{

	[Export] Area3D Hitbox;

	// public Vector3 Direction { get; set; }

	public float Speed { get; set; }

	private float _timer;
	public Vector3 StartPosition;

	[Signal]
	public delegate void OnHitEventHandler( Node3D hitNode, PelletGun pelletGun );

	[Signal]
	public delegate void OnTimeoutEventHandler();

	public PelletGun PelletGun { get; set; }

	public override void _Ready()
	{
		AddToGroup( "pellet" );

		Hitbox.BodyEntered += ( body ) =>
		{
			OnHitNode( body );
		};
	}

	public override void _Process( double delta )
	{
		// GlobalPosition += Direction * Speed * (float)delta;
		GlobalPosition += -Transform.Basis.Z * Speed * (float)delta;

		_timer += (float)delta;

		// as a failsafe, destroy the pellet after 10 seconds
		if ( _timer > 10 )
		{
			Logger.Info( "Pellet", "Pellet timed out" );
			EmitSignal( SignalName.OnTimeout );
			PlayHitSound();
			QueueFree();
		}
		else if ( GlobalPosition.DistanceTo( StartPosition ) > 20 )
		{
			Logger.Info( "Pellet", "Pellet went too far" );
			EmitSignal( SignalName.OnTimeout );
			PlayHitSound();
			QueueFree();
		}
	}

	public void OnHitNode( Node3D hitNode )
	{
		/* if ( hitNode is BaseCarriable carriable )
		{
			carriable.OnHitByPellet( this );
		} */
		Logger.Info( "Pellet", $"Pellet hit {hitNode.Name}" );
		EmitSignal( SignalName.OnHit, hitNode, PelletGun );

		if ( hitNode is IShootable shootable )
		{
			shootable.OnShot( this );
		}

		PlayHitSound();

		QueueFree();
	}

	private void PlayHitSound()
	{
		Sounds.Play( "res://objects/pellet/pellet_hit.wav", GlobalPosition );
	}


}
