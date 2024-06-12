using System;
using Godot;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Helpers;
using vcrossing.Code.Player;

namespace vcrossing.Code.WorldBuilder;

public partial class Bridge : Building
{

	[Export, Require] public Area3D CollisionTrigger { get; set; }

	[Export, Require] public Node3D WorldMesh { get; set; }

	public override void _Ready()
	{
		base._Ready();
		CollisionTrigger.BodyEntered += OnBodyEntered;
		CollisionTrigger.BodyExited += OnBodyExited;
	}

	private uint _playerLayer;

	private void OnBodyExited( Node3D body )
	{
		if ( body is not PlayerController player )
		{
			return;
		}
		player.CollisionMask = _playerLayer;
		/* var collisions = NodeExtensions.GetNodesOfType<CollisionShape3D>( WorldMesh );
		foreach ( var collision in collisions )
		{
			// TODO: set layers instead of disabling
			// collision.Disabled = false;
			collision.SetDeferred( "disabled", false );
		} */
	}

	private void OnBodyEntered( Node3D body )
	{
		if ( body is not PlayerController player )
		{
			return;
		}
		_playerLayer = player.CollisionMask;
		player.CollisionMask = 1 << 12;
		/* var collisions = NodeExtensions.GetNodesOfType<CollisionShape3D>( WorldMesh );
		foreach ( var collision in collisions )
		{
			// collision.Disabled = true;
			collision.SetDeferred( "disabled", true );
		} */
	}
}
