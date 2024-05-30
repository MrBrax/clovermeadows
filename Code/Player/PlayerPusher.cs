﻿using vcrossing2.Code.Helpers;
using vcrossing2.Code.Npc;

namespace vcrossing2.Code.Player;

/// <summary>
///  Pushes other players and npc's away from the player.
/// </summary>
public partial class PlayerPusher : Area3D
{
	[Export] public float PushForce = 0.1f;

	private List<Node3D> _pushedNodes = new();

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

		foreach ( var node in _pushedNodes )
		{
			Push( node );
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

		if ( node.GlobalPosition.DistanceTo( GlobalPosition ) > 2 )
		{
			// _pushedNodes.Remove( node );
			Logger.Warn( "PlayerPusher", $"Node {node.Name} is too far away." );
			return;
		}
		
		if ( node is not CharacterBody3D body ) return;

		if ( node == GetParent() )
		{
			// Logger.Warn( "PlayerPusher", "Node is self." );
			return;
		}
		
		var direction = (body.GlobalTransform.Origin - GlobalTransform.Origin).Normalized();
		
		// only push when moving
		direction *= MoveVelocity.Length();

		// counteract the opposite velocity
		// direction += body.Velocity * 2f;
		
		direction.Y = 0;
		
		body.Velocity += direction * PushForce;
		
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