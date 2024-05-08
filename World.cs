using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using vcrossing.DTO;

namespace vcrossing;

public partial class World : Node3D
{
	[Flags]
	public enum ItemPlacement
	{
		Wall = 1,
		OnTop = 2,
		Floor = 4
	}

	public enum ItemPlacementType
	{
		Placed,
		Dropped
	}

	public enum ItemRotation
	{
		North = 1,
		East = 2,
		South = 3,
		West = 4
	}

	public enum Direction
	{
		North = 1,
		South = 2,
		West = 3,
		East = 4,
		NorthWest = 5,
		NorthEast = 6,
		SouthWest = 7,
		SouthEast = 8
	}

	public const int GridSize = 1;
	public const float GridSizeCenter = GridSize / 2f;
	public const int GridWidth = 16;
	public const int GridHeight = 16;

	public Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<ItemPlacement, WorldItem>> Items = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/*SpawnPlacedItem( GD.Load<ItemData>( "res://items/furniture/single_bed/single_bed.tres" ), new Vector2I( 0, 0 ),
			ItemPlacement.Floor, ItemRotation.North );
		SpawnPlacedItem( GD.Load<ItemData>( "res://items/furniture/single_bed/single_bed.tres" ), new Vector2I( 0, 2 ),
			ItemPlacement.Floor, ItemRotation.West );
		SpawnPlacedItem( GD.Load<ItemData>( "res://items/furniture/single_bed/single_bed.tres" ), new Vector2I( 0, 4 ),
			ItemPlacement.Floor, ItemRotation.South );
		SpawnPlacedItem( GD.Load<ItemData>( "res://items/furniture/single_bed/single_bed.tres" ), new Vector2I( 0, 6 ),
			ItemPlacement.Floor, ItemRotation.East );

		SpawnPlacedItem( GD.Load<ItemData>( "res://items/furniture/armchair/armchair.tres" ), new Vector2I( 3, 0 ),
			ItemPlacement.Floor, ItemRotation.North );
		SpawnPlacedItem( GD.Load<ItemData>( "res://items/furniture/armchair/armchair.tres" ), new Vector2I( 4, 0 ),
			ItemPlacement.Floor, ItemRotation.West );
		SpawnPlacedItem( GD.Load<ItemData>( "res://items/furniture/armchair/armchair.tres" ), new Vector2I( 5, 0 ),
			ItemPlacement.Floor, ItemRotation.South );
		SpawnPlacedItem( GD.Load<ItemData>( "res://items/furniture/armchair/armchair.tres" ), new Vector2I( 6, 0 ),
			ItemPlacement.Floor, ItemRotation.East );
		SpawnPlacedItem( GD.Load<ItemData>( "res://items/furniture/armchair/armchair.tres" ), new Vector2I( 7, 0 ),
			ItemPlacement.Floor, ItemRotation.North );
		Save();*/
		Load();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process( double delta )
	{
	}

	public void Save()
	{
		var save = new SaveData();
		save.AddWorldItems( this );
		save.SaveFile( "user://save.json" );
	}

	public void Load()
	{
		var save = new SaveData();
		save.LoadFile( "user://save.json" );
		save.LoadWorldItems( this );
	}

	public bool IsOutsideGrid( Vector2I position )
	{
		return position.X < 0 || position.X >= GridWidth || position.Y < 0 || position.Y >= GridHeight;
	}

	public Quaternion GetRotation( ItemRotation rotation )
	{
		return rotation switch
		{
			ItemRotation.North => new Quaternion( 0, 0, 0, 1 ),
			ItemRotation.East => new Quaternion( 0, 0.7071068f, 0, 0.7071068f ),
			ItemRotation.South => new Quaternion( 0, 1, 0, 0 ),
			ItemRotation.West => new Quaternion( 0, -0.7071068f, 0, 0.7071068f ),
			_ => new Quaternion( 0, 0, 0, 1 )
		};
	}

	/*public void AddItem( Vector2I position, ItemRotation rotation, WorldItem item, ItemPlacement placement, bool isMainTile )
	{
		if ( Items.TryGetValue( position, out var dict ) )
		{
			dict[placement] = item;
		}
		else
		{
			Items[position] = new Dictionary<ItemPlacement, WorldItem> { { placement, item } };
		}

		item.GridPosition = position;
		item.Rotation = GetRotation( rotation );
		item.Placement = placement;
		// item.IsMainTile = isMainTile;
	}*/

	public WorldItem SpawnPlacedItem( ItemData item, Vector2I position, ItemPlacement placement,
		ItemRotation rotation )
	{
		if ( !item.Placements.HasFlag( placement ) )
		{
			throw new Exception( $"Item {item} does not support placement {placement}" );
		}

		if ( IsOutsideGrid( position ) )
		{
			throw new Exception( $"Position {position} is outside the grid" );
		}

		var itemInstance = item.PlaceScene.Instantiate<WorldItem>();
		if ( itemInstance == null )
		{
			// GD.PrintErr( $"Failed to instantiate item {item}" );
			throw new Exception( $"Failed to instantiate item {item}" );
		}

		itemInstance.ItemDataPath = item.ResourcePath;
		itemInstance.GridPosition = position;
		itemInstance.GridRotation = rotation;
		itemInstance.Placement = placement;
		itemInstance.PlacementType = ItemPlacementType.Placed;
		AddItem( position, placement, itemInstance );
		AddChild( itemInstance );
		GD.Print( $"Spawned item {itemInstance} at {position} with placement {placement} and rotation {rotation}" );
		return itemInstance;
	}

	public DroppedItem SpawnDroppedItem( ItemData item, Vector2I position, ItemPlacement placement,
		ItemRotation rotation )
	{
		if ( !item.Placements.HasFlag( placement ) )
		{
			throw new Exception( $"Item {item} does not support placement {placement}" );
		}

		if ( IsOutsideGrid( position ) )
		{
			throw new Exception( $"Position {position} is outside the grid" );
		}

		var itemInstance = item.DropScene.Instantiate<DroppedItem>();
		if ( itemInstance == null )
		{
			// GD.PrintErr( $"Failed to instantiate item {item}" );
			throw new Exception( $"Failed to instantiate item {item}" );
		}

		itemInstance.ItemDataPath = item.ResourcePath;
		itemInstance.GridPosition = position;
		itemInstance.GridRotation = rotation;
		itemInstance.Placement = placement;
		itemInstance.PlacementType = ItemPlacementType.Dropped;
		AddItem( position, placement, itemInstance );
		AddChild( itemInstance );
		// GD.Print( $"Spawned item {itemInstance} at {position} with placement {placement} and rotation {rotation}" );
		return itemInstance;
	}

	public WorldItem SpawnDto( BaseDTO dto, Vector2I position, ItemPlacement placement )
	{
		var item = GD.Load<ItemData>( dto.ItemDataPath );
		if ( item == null )
		{
			throw new Exception( $"Failed to load item data {dto.ItemDataPath}" );
		}

		WorldItem worldItem;
		if ( dto.PlacementType == ItemPlacementType.Dropped )
		{
			worldItem = SpawnDroppedItem( item, position, placement, dto.GridRotation );
		}
		else
		{
			worldItem = SpawnPlacedItem( item, position, placement, dto.GridRotation );
		}

		worldItem.DTO = dto;
		worldItem.UpdateFromDTO();
		
		UpdateTransform( position, placement );
		
		return worldItem;

		/*if ( item.Placements.HasFlag( placement ) )
		{
			return SpawnPlacedItem( item, position, placement, dto.GridRotation );
		}
		else
		{
			return SpawnDroppedItem( item, position, placement, dto.GridRotation );
		}*/
	}
	
	public string Vector2IToString( Vector2I vector )
	{
		return $"{vector.X},{vector.Y}";
	}

	public void AddItem( Vector2I position, ItemPlacement placement, WorldItem item )
	{
		if ( IsOutsideGrid( position ) )
		{
			throw new Exception( $"Position {position} is outside the grid" );
		}

		if ( !item.GetItemData().Placements.HasFlag( placement ) )
		{
			throw new Exception( $"Item {item} does not support placement {placement}" );
		}

		var positionString = Vector2IToString( position );

		if ( Items.TryGetValue( positionString, out var dict ) )
		{
			dict[placement] = item;
		}
		else
		{
			Items[positionString] = new Godot.Collections.Dictionary<ItemPlacement, WorldItem> { { placement, item } };
		}

		item.GridPosition = position;
		item.Placement = placement;
		// GD.Print( $"Added item {item} at {position} with placement {placement}" );
		UpdateTransform( position, placement );

		// Save();
	}

	public void RemoveItem( Vector2I position, ItemPlacement placement )
	{
		var positionString = Vector2IToString( position );
		if ( Items.TryGetValue( positionString, out var dict ) )
		{
			if ( dict.ContainsKey( placement ) )
			{
				var item = dict[placement];
				item.QueueFree();
				dict.Remove( placement );
				// GD.Print( $"Removed item {item} at {position} with placement {placement}" );
			}
		}
	}

	private void UpdateTransform( Vector2I position, ItemPlacement placement )
	{
		var positionString = Vector2IToString( position );
		var item = Items.TryGetValue( positionString, out var dict ) ? dict[placement] : null;
		if ( item == null ) throw new Exception( $"Failed to find item at {position} with placement {placement}" );

		var newPosition = new Vector3( position.X + GridSizeCenter, 0, position.Y + GridSizeCenter );
		var newRotation = GetRotation( item.GridRotation );

		item.Transform = new Transform3D( new Basis( newRotation ), newPosition );

		// GD.Print(
		// 	$"Updated transform of {item} to {item.Transform} (position: {newPosition} (grid: {position}), rotation: {newRotation} (grid: {item.GridRotation}))" );
	}

	public Vector2I WorldToItemGrid( Vector3 worldPosition )
	{
		return new Vector2I( (int)(worldPosition.X / GridSize), (int)(worldPosition.Z / GridSize) );
	}

	public Direction Get8Direction( float angle )
	{
		var snapAngle = Mathf.Round( angle / 45 ) * 45;
		// Log.Info( $"Snap angle: {snapAngle}" );
		switch ( snapAngle )
		{
			case 0:
				return Direction.South;
			case 45:
				return Direction.SouthEast;
			case 90:
				return Direction.East;
			case 135:
				return Direction.NorthEast;
			case 180:
			case -180:
				return Direction.North;
			case -135:
				return Direction.NorthWest;
			case -90:
				return Direction.West;
			case -45:
				return Direction.SouthWest;
		}

		return Direction.North;
	}

	public Vector2I GetPositionInDirection( Vector2I gridPos, Direction gridDirection )
	{
		return gridDirection switch
		{
			Direction.North => new Vector2I( gridPos.X, gridPos.Y - 1 ),
			Direction.South => new Vector2I( gridPos.X, gridPos.Y + 1 ),
			Direction.West => new Vector2I( gridPos.X - 1, gridPos.Y ),
			Direction.East => new Vector2I( gridPos.X + 1, gridPos.Y ),
			Direction.NorthWest => new Vector2I( gridPos.X - 1, gridPos.Y - 1 ),
			Direction.NorthEast => new Vector2I( gridPos.X + 1, gridPos.Y - 1 ),
			Direction.SouthWest => new Vector2I( gridPos.X - 1, gridPos.Y + 1 ),
			Direction.SouthEast => new Vector2I( gridPos.X + 1, gridPos.Y + 1 ),
			_ => gridPos
		};
	}

	public IEnumerable<WorldItem> GetItems( Vector2I gridPos )
	{
		if ( IsOutsideGrid( gridPos ) )
		{
			throw new Exception( $"Position {gridPos} is outside the grid" );
		}

		if ( Items == null ) throw new Exception( "Items is null" );

		HashSet<WorldItem> foundItems = new();

		var gridPosString = Vector2IToString( gridPos );

		// get items at exact grid position
		if ( Items.TryGetValue( gridPosString, out var dict ) )
		{
			foreach ( var item in dict.Values )
			{
				yield return item;
				foundItems.Add( item );
			}
		}

		// get items that are intersecting this grid position
		foreach ( var item in Items.Values.SelectMany( d => d.Values ) )
		{
			if ( item.GridSize.X == 1 && item.GridSize.Y == 1 ) continue;

			if ( item.GetGridPositions( true ).Contains( gridPos ) )
			{
				if ( foundItems.Contains( item ) ) continue;
				yield return item;
				foundItems.Add( item );
			}

			/*var positions = item.GetGridPositions( true );
			if ( positions.Contains( gridPos ) )
			{
				yield return item;
			}*/
		}
	}

	public void RemoveItem( WorldItem item )
	{
		RemoveItem( item.GridPosition, item.Placement );
	}
}
