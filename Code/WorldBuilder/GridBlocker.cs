﻿using Godot;
using vcrossing2.Code.Helpers;

namespace vcrossing2.Code.WorldBuilder;

public partial class GridBlocker : Node3D
{
	
	public override void _Ready()
	{
		Logger.Debug( "GridBlocker ready." );
		var world = GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;
		// world.AddPlacementBlocker( PlacementBlocker );
		foreach ( var child in GetChildren() )
		{
			if ( child is Area3D area )
			{
				world.AddPlacementBlocker( area );
			}
		}
	}
	
}
