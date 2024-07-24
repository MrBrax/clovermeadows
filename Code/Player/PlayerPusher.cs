using vcrossing.Code.Interfaces;
using vcrossing.Code.Npc;

namespace vcrossing.Code.Player;

/// <summary>
///  Pushes other players and npc's away from the player.
/// </summary>
public partial class PlayerPusher : Area3D
{
	[Export] public float PushForce = 0.1f;

	private IList<Node3D> _pushedNodes = [];

	private Vector3 MoveVelocity
	{
		get
		{
			if ( GetParent() is CharacterBody3D body )
			{
				return body.Velocity;
			}

			return Vector3.Zero;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	private void OnBodyEntered( Node body )
	{
		if ( body is Node3D node and IPushable )
		{
			// Push( node );
			_pushedNodes.Add( node );
		}
	}

	private void OnBodyExited( Node body )
	{
		if ( body is Node3D node )
		{
			_pushedNodes.Remove( node );
		}
	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		/*foreach ( var node in _pushedNodes )
		{
			Push( node );
		}*/

		// prevent concurrent modification
		for ( var i = 0; i < _pushedNodes.Count; i++ )
		{
			Push( _pushedNodes[i] );
		}
	}

	public void Push( Node3D node )
	{
		/*if ( node is PlayerController player )
		{
			var direction = (player.GlobalTransform.Origin - GlobalTransform.Origin).Normalized();
			direction.Y = 0;
			player.Velocity += direction * PushForce;
			// Logger.Info( "PlayerPusher", $"Pushing player." );
		}
		else if ( node is BaseNpc npc )
		{
			var direction = (npc.GlobalTransform.Origin - GlobalTransform.Origin).Normalized();
			direction.Y = 0;
			npc.Velocity += direction * PushForce;
			// Logger.Info( "PlayerPusher", $"Pushing npc." );
		}*/

		if ( node is not IPushable pushable )
		{
			Logger.Warn( "PlayerPusher", $"Node {node.Name} is not IPushable." );
			return;
		}

		if ( MoveVelocity.Length() < 0.1f )
		{
			// Logger.Warn( "PlayerPusher", "Player is not moving." );
			return;
		}

		if ( node.GlobalPosition.DistanceTo( GlobalPosition ) > 2 )
		{
			_pushedNodes.Remove( node );
			// Logger.Warn( "PlayerPusher", $"Node {node.Name} is too far away." );
			return;
		}

		if ( node == GetParent() )
		{
			// Logger.Warn( "PlayerPusher", "Node is self." );
			return;
		}

		var direction = (node.GlobalTransform.Origin - GlobalTransform.Origin).Normalized();

		// only push when moving
		direction *= MoveVelocity.Length();

		direction *= pushable.PushForce;

		// counteract the opposite velocity
		// direction += body.Velocity * 2f;

		direction.Y = 0;

		if ( node is CharacterBody3D body )
		{
			Logger.Info( "PlayerPusher", $"Pushing body {body.Name}." );
			body.Velocity += direction * PushForce;
		}
		else if ( node is RigidBody3D rigidBody )
		{
			Logger.Info( "PlayerPusher", $"Pushing rigid body {rigidBody.Name}." );
			// rigidBody.LinearVelocity += direction * PushForce;
			rigidBody.ApplyForce( direction * PushForce, Vector3.Zero );
		}
		else
		{
			Logger.Warn( "PlayerPusher", $"Node {node.Name} is not a CharacterBody3D or RigidBody3D." );
		}

		pushable.OnPushed( this );

		if ( pushable.PushOnce ) _pushedNodes.Remove( node );
	}

	/* public void Push( Node3D node, float force )
	{
		if ( node is Player player )
		{
			var direction = (player.GlobalTransform.origin - GlobalTransform.origin).Normalized();
			player.Velocity += direction * force;
		}
		else if ( node is Npc npc )
		{
			var direction = (npc.GlobalTransform.origin - GlobalTransform.origin).Normalized();
			npc.Velocity += direction * force;
		}
	} */
}
