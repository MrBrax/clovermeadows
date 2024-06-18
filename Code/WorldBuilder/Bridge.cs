using System;
using Godot;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Helpers;
using vcrossing.Code.Player;
using vcrossing.Code.Vehicles;

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

	// private uint _playerLayer;
	private Dictionary<Node3D, uint> _nodeLayers = new();

	private void OnBodyExited( Node3D body )
	{
		if ( body is PlayerController player )
		{
			player.CollisionMask = _nodeLayers.GetValueOrDefault( player, 1u );
			return;
		}

		if ( body is BaseVehicle vehicle )
		{
			vehicle.CollisionMask = _nodeLayers.GetValueOrDefault( vehicle, 1u );
			return;
		}

	}

	private void OnBodyEntered( Node3D body )
	{
		if ( body is PlayerController player )
		{
			_nodeLayers[player] = player.CollisionMask;
			player.CollisionMask = 1 << 12;
			return;
		}

		if ( body is BaseVehicle vehicle )
		{
			_nodeLayers[vehicle] = vehicle.CollisionMask;
			vehicle.CollisionMask = 1 << 12;
			return;
		}

	}
}
