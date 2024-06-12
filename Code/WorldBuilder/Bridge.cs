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

	private void OnBodyExited( Node3D body )
	{
		if ( body is not PlayerController player )
		{
			return;
		}
		var collisions = NodeExtensions.GetNodesOfType<CollisionShape3D>( WorldMesh );
		foreach ( var collision in collisions )
		{
			// TODO: set layers instead of disabling
			// collision.Disabled = false;
			collision.SetDeferred( "disabled", false );
		}
	}

	private void OnBodyEntered( Node3D body )
	{
		if ( body is not PlayerController player )
		{
			return;
		}
		var collisions = NodeExtensions.GetNodesOfType<CollisionShape3D>( WorldMesh );
		foreach ( var collision in collisions )
		{
			// collision.Disabled = true;
			collision.SetDeferred( "disabled", true );
		}
	}
}
