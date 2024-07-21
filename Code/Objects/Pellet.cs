using Godot;
using vcrossing.Code.Carriable;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Objects;

public partial class Pellet : Node3D
{

	[Export] Area3D Hitbox;

	public Vector3 Direction { get; set; }

	public float Speed { get; set; }

	private float _timer;

	public override void _Ready()
	{
		AddToGroup( "pellet" );

		Hitbox.BodyEntered += ( body ) =>
		{
			OnHit( body );
		};
	}

	public override void _Process( double delta )
	{
		GlobalPosition += Direction * Speed * (float)delta;

		_timer += (float)delta;

		// as a failsafe, destroy the pellet after 10 seconds
		if ( _timer > 10 )
		{
			Logger.Info( "Pellet", "Pellet timed out" );
			QueueFree();
		}
	}

	public void OnHit( Node3D hitNode )
	{
		/* if ( hitNode is BaseCarriable carriable )
		{
			carriable.OnHitByPellet( this );
		} */
		Logger.Info( "Pellet", $"Pellet hit {hitNode.Name}" );
		QueueFree();
	}


}
