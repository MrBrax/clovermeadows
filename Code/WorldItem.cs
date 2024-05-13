using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using vcrossing2.Code.DTO;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;

namespace vcrossing2.Code;

public partial class WorldItem : Node3D
{
	// [Export] public string Name { get; set; }
	public Vector2I GridPosition { get; set; }

	public World.ItemRotation GridRotation { get; set; }

	// [Export] public ItemData ItemData { get; set; }
	[Export] public string ItemDataPath { get; set; }
	[Export] public NodePath Model { get; set; }
	[Export] public bool IsPlacedInEditor { get; set; }
	public World.ItemPlacement Placement { get; set; }
	[Export] public World.ItemPlacementType PlacementType { get; set; }

	protected World World => GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;

	// public BaseItemDTO DTO = new();

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

	public string GetName()
	{
		return GetItemData().Name;
	}

	public virtual bool ShouldBeSaved()
	{
		return true;
	}

	public virtual bool CanBePickedUp()
	{
		return true;
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

	public virtual void OnPlayerUse( PlayerInteract playerInteract, Vector2I pos )
	{
		GD.Print( "Player used " + GetItemData().Name );
	}

	public virtual void OnPlayerPickUp( PlayerInteract playerInteract )
	{
		// QueueFree();
		// World.RemoveItem( this );

		var playerInventory = playerInteract.GetNode<Player.Inventory>( "../PlayerInventory" );
		playerInventory.PickUpItem( this );
	}

	/*public void UpdateDTO()
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
		// GD.Print( $"Updated {this} from DTO (rot: {GridRotation})" );
	}*/

	public override string ToString()
	{
		if ( !IsInsideTree() ) return $"[WorldItem:{GetItemData()?.Name} (not in tree)]";
		return $"[WorldItem:{GetItemData()?.Name} @ {GridPosition}]";
	}
}
