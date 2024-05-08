using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
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
		
		GD.Print( "Getting size of " + itemData.Name );
		
		GD.Print( "Width: " + width + ", Height: " + height );
		
		if ( width == 0 || height == 0 ) throw new Exception( "Item has no size" );

		if ( width == 1 && height == 1 )
		{
			GD.Print( "Adding single position" );
			return new List<Vector2I> { global ? GridPosition : Vector2I.Zero };
		}
		
		if ( GridRotation == World.ItemRotation.East || GridRotation == World.ItemRotation.West )
		{
			width = itemData.Height;
			height = itemData.Width;
		}
		else if ( GridRotation == World.ItemRotation.South )
		{
			width = itemData.Width * -1;
			height = itemData.Height * -1;
		}
		
		for ( var y = 0; y < width; y++ )
		{
			for ( var x = 0; x < height; x++ )
			{
				var vector = global ? GridPosition + new Vector2I( x, y ) : new Vector2I( x, y );
				positions.Add( vector );
				GD.Print( "Adding position " + vector );
			}
		}
		
		return positions;
	}
	
	public virtual void OnPlayerUse( PlayerInteract player )
	{
		GD.Print( "Player used " + GetItemData().Name );
	}
	
}
