﻿using System;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public partial class WorldItem : BaseItem, IWorldItem, IPersistence
{
	// [Export] public string Name { get; set; }
	public Vector2I GridPosition { get; set; }

	public World.ItemRotation GridRotation { get; set; }

	// [Export] public ItemData ItemData { get; set; }

	[Export] public bool IsPlacedInEditor { get; set; }
	[Export] public World.ItemPlacement Placement { get; set; }
	[Export] public World.ItemPlacementType PlacementType { get; set; }

	public virtual Type PersistentType => typeof( Persistence.PersistentItem );

	public DateTime Placed { get; set; }

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

	[Obsolete]
	public ItemData GetItemData()
	{
		if ( string.IsNullOrEmpty( ItemDataPath ) ) throw new Exception( "ItemDataPath is null" );
		return Loader.LoadResource<ItemData>( ItemDataPath );
	}

	public string GetName()
	{
		return ItemData.Name;
	}

	public virtual bool ShouldBeSaved()
	{
		return true;
	}

	public virtual bool CanBePickedUp()
	{
		return !ItemData.DisablePickup;
	}

	public List<Vector2I> GetGridPositions( bool global = false )
	{
		var positions = new List<Vector2I>();

		var itemData = ItemData;

		// rotate the item based on the rotation
		var width = itemData.Width;
		var height = itemData.Height;
		var rotation = GridRotation;

		// Logger.Info( $"Getting size of {itemData.Name}" );
		// Logger.Info( $"Width: {width}, Height: {height}, Rotation: {rotation}" );

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
		Logger.Info( $"Player used {ItemData.Name}" );
	}

	public virtual void OnPlayerPickUp( PlayerInteract playerInteract )
	{
		// QueueFree();
		// World.RemoveItem( this );

		if ( !CanBePickedUp() )
		{
			Logger.Info( $"Cannot pick up {GetName()}" );
			return;
		}

		var playerInventory = playerInteract.GetNode<Components.Inventory>( "../PlayerInventory" );
		playerInventory.PickUpItem( World.GetNodeLink( this ) );
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
		// Logger.Info( $"Updated {this} from DTO (rot: {GridRotation})" );
	}*/

	public override string ToString()
	{
		if ( !IsInsideTree() ) return $"[WorldItem:{ItemData?.Name} (not in tree)]";
		return $"[WorldItem:{ItemData?.Name} @ {GridPosition}]";
	}

	public void DisableCollisions()
	{
		var colliders = this.GetNodesOfType<CollisionShape3D>();
		foreach ( var collider in colliders )
		{
			collider.Disabled = true;
		}
	}

	public virtual Dictionary<string, object> GetNodeData()
	{
		return default;
	}

	public virtual void SetNodeData( Dictionary<string, object> data )
	{

	}
}
