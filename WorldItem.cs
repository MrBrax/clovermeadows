﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;
using Godot.Collections;
using vcrossing.DTO;
using vcrossing.Player;

namespace vcrossing;

public partial class WorldItem : Node3D
{
	// [Export] public string Name { get; set; }
	public Vector2I GridPosition { get; set; }

	public World.ItemRotation GridRotation { get; set; } = World.ItemRotation.North;

	// [Export] public ItemData ItemData { get; set; }
	public string ItemDataPath { get; set; }
	[Export] public NodePath Model { get; set; }
	public World.ItemPlacement Placement { get; set; } = World.ItemPlacement.Floor;
	public World.ItemPlacementType PlacementType { get; set; } = World.ItemPlacementType.Placed;
	
	[JsonIgnore] protected World World => GetNode<World>( "/root/Main/World" );

	public BaseDTO DTO = new();

	// public Vector2I Size => new( GetItemData().Width, GetItemData().Height );
	public Vector2I GridSize
	{
		get
		{
			var positions = GetGridPositions();
			if ( positions.Count == 0 )
			{
				throw new Exception( "No grid positions found" );
				// return new Vector2I( 1, 1 );
			}

			var minX = positions.Min( p => p.X );
			var minY = positions.Min( p => p.Y );
			var maxX = positions.Max( p => p.X );
			var maxY = positions.Max( p => p.Y );
			return new Vector2I( maxX - minX + 1, maxY - minY + 1 );
		}
	}

	public ItemData GetItemData()
	{
		return GD.Load<ItemData>( ItemDataPath );
	}

	public List<Vector2I> GetGridPositions( bool global = false )
	{
		var positions = new List<Vector2I>();

		var itemData = GetItemData();

		// rotate the item based on the rotation
		var width = itemData.Width;
		var height = itemData.Height;
		var rotation = GridRotation;

		// GD.Print( $"Getting size of {itemData.Name}" );
		// GD.Print( $"Width: {width}, Height: {height}, Rotation: {rotation}" );

		if ( width == 0 || height == 0 ) throw new Exception( "Item has no size" );

		if ( width == 1 && height == 1 )
		{
			return new List<Vector2I> { global ? GridPosition : Vector2I.Zero };
		}

		if ( rotation == World.ItemRotation.North )
		{
			for ( var x = 0; x < width; x++ )
			{
				for ( var y = 0; y < height; y++ )
				{
					positions.Add( new Vector2I( x, y ) );
				}
			}
		}
		else if ( rotation == World.ItemRotation.South )
		{
			for ( var x = 0; x < width; x++ )
			{
				for ( var y = 0; y < height; y++ )
				{
					positions.Add( new Vector2I( x, y * -1 ) );
				}
			}
		}
		else if ( rotation == World.ItemRotation.East )
		{
			for ( var x = 0; x < height; x++ )
			{
				for ( var y = 0; y < width; y++ )
				{
					positions.Add( new Vector2I( x, y ) );
				}
			}
		}
		else if ( rotation == World.ItemRotation.West )
		{
			for ( var x = 0; x < height; x++ )
			{
				for ( var y = 0; y < width; y++ )
				{
					positions.Add( new Vector2I( x * -1, y ) );
				}
			}
		}
		
		if ( global )
		{
			positions = positions.Select( p => p + GridPosition ).ToList();
		}

		return positions;
	}

	public virtual void OnPlayerUse( PlayerInteract playerInteract )
	{
		GD.Print( "Player used " + GetItemData().Name );
	}

	public virtual void OnPlayerPickUp( PlayerInteract playerInteract )
	{
		
		// QueueFree();
		World.RemoveItem( this );

	}
	
	public void UpdateDTO()
	{
		// DTO.GridPosition = GridPosition;
		// DTO.GridRotation = GridRotation;
		// DTO.Placement = Placement;
		DTO.ItemDataPath = ItemDataPath;
		DTO.PlacementType = PlacementType;
		DTO.GridRotation = GridRotation;
	}
	
	public void UpdateFromDTO()
	{
		// GridPosition = DTO.GridPosition;
		// GridRotation = DTO.GridRotation;
		// Placement = DTO.Placement;
		ItemDataPath = DTO.ItemDataPath;
		PlacementType = DTO.PlacementType;
		GridRotation = DTO.GridRotation;
		GD.Print( $"Updated {this} from DTO (rot: {GridRotation})" );
	}
}
