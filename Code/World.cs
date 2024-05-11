using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using vcrossing2.Code.DTO;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Items;
using vcrossing2.Code.Save;

namespace vcrossing2.Code;

public partial class World : Node3D
{
	[Export] public string WorldName { get; set; }

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

	// public const int GridWidth = 16;
	// public const int GridHeight = 16;
	[Export] public int GridWidth { get; set; } = 16;
	[Export] public int GridHeight { get; set; } = 16;
	[Export] public bool UseAcres { get; set; } = false;
	[Export] public int AcreWidth { get; set; } = 16;
	[Export] public int AcreHeight { get; set; } = 16;

	public Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<ItemPlacement, WorldItem>> Items = new();

	public HashSet<Vector2I> BlockedGridPositions = new();
	public Dictionary<Vector2I, float> GridPositionHeights = new();

	public float CurrentTime => (float)(Time.GetUnixTimeFromSystem() % 86400);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print( $"World spawned" );
		try
		{
			SpawnPlacedItem<PlacedItem>( GD.Load<ItemData>( "res://items/furniture/polka_chair/polka_chair.tres" ),
				new Vector2I( 0, 5 ),
				ItemPlacement.Floor, ItemRotation.North );
		}
		catch ( Exception e )
		{
			GD.Print( e );
		}

		try
		{
			SpawnPlacedItem<PlacedItem>( GD.Load<ItemData>( "res://items/furniture/single_bed/single_bed.tres" ),
				new Vector2I( 0, 7 ),
				ItemPlacement.Floor, ItemRotation.North );
		}
		catch ( Exception e )
		{
			GD.Print( e );
		}

		try
		{
			SpawnPlacedItem<PlacedItem>( GD.Load<ItemData>( "res://items/furniture/armchair/armchair.tres" ),
				new Vector2I( 0, 9 ),
				ItemPlacement.Floor, ItemRotation.North );
		}
		catch ( Exception e )
		{
			GD.Print( e );
		}

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
		// Load();

		CheckTerrain();
	}

	private void CheckTerrain()
	{
		for ( var x = 0; x < GridWidth; x++ )
		{
			for ( var y = 0; y < GridHeight; y++ )
			{
				var gridPos = new Vector2I( x, y );
				var check = CheckGridPositionEligibility( gridPos, out var worldPos );

				if ( worldPos.Y != 0 )
				{
					GridPositionHeights[gridPos] = worldPos.Y;
					// GD.Print( $"Adding grid position height {gridPos} = {worldPos.Y}" );
				}

				if ( !check )
				{
					BlockedGridPositions.Add( gridPos );
					// GD.Print( $"Blocking grid position from terrain check: {gridPos}" );
					// GetTree().CallGroup( "debugdraw", "add_line", ItemGridToWorld( gridPos ), ItemGridToWorld( gridPos ) + new Vector3( 0, 10, 0 ), new Color( 1, 0, 0 ), 15 );
				}
				else
				{
					// GetTree().CallGroup( "debugdraw", "add_line", ItemGridToWorld( gridPos ), ItemGridToWorld( gridPos ) + new Vector3( 0, 10, 0 ), new Color( 0, 1, 0 ), 15 );
				}
			}
		}

		using var cacheFile = FileAccess.Open( "user://grid_height.bin", FileAccess.ModeFlags.Write );
		/*cacheFile.StoreString(
			JsonSerializer.Serialize(
				GridPositionHeights.Select( x => new { Key = $"{x.Key.X},{x.Key.Y}", Value = x.Value } )
					.ToDictionary( x => x.Key, x => x.Value ) ) );
		cacheFile.Close();*/
		cacheFile.StorePascalString( WorldName );
		cacheFile.Store32( (uint)GridPositionHeights.Count() );
		foreach ( var kvp in GridPositionHeights )
		{
			cacheFile.Store16( (ushort)kvp.Key.X );
			cacheFile.Store16( (ushort)kvp.Key.Y );
			cacheFile.StoreFloat( kvp.Value );
		}
		cacheFile.Close();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process( double delta )
	{
	}

	public void Save()
	{
		var worldSave = new WorldSaveData();
		worldSave.LoadFile( "user://world.json" );
		worldSave.AddWorldItems( this );
		worldSave.SaveFile( "user://world.json" );
	}

	public void Load()
	{
		GD.Print( $"Loading world {WorldName}" );
		var save = new WorldSaveData();
		if ( save.LoadFile( "user://world.json" ) )
		{
			save.LoadWorldItems( this );
		}
	}

	public void LoadEditorPlacedItems()
	{
		var items = GetChildren().OfType<WorldItem>().Where( x => x.IsPlacedInEditor );
		foreach ( var item in items )
		{
			var gridPosition = WorldToItemGrid( item.GlobalTransform.Origin );
			AddItem( gridPosition, item.Placement, item );
			GD.Print( $"Loaded editor placed item {item} at {gridPosition}" );
		}
	}

	// TODO: is this actually needed
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

	public bool CanPlaceItem( ItemData item, Vector2I position, ItemRotation rotation, ItemPlacement placement )
	{
		if ( IsOutsideGrid( position ) )
		{
			// throw new Exception( $"Position {position} is outside the grid" );
			GD.Print( $"Position {position} is outside the grid" );
			return false;
		}

		var positions = new List<Vector2I>();
		var width = item.Width;
		var height = item.Height;

		if ( width == 0 || height == 0 ) throw new Exception( "Item has no size" );

		if ( rotation == ItemRotation.North )
		{
			for ( var x = 0; x < width; x++ )
			{
				for ( var y = 0; y < height; y++ )
				{
					positions.Add( new Vector2I( position.X + x, position.Y + y ) );
				}
			}
		}
		else if ( rotation == ItemRotation.South )
		{
			for ( var x = 0; x < width; x++ )
			{
				for ( var y = 0; y < height; y++ )
				{
					positions.Add( new Vector2I( position.X + x, position.Y - y ) );
				}
			}
		}
		else if ( rotation == ItemRotation.East )
		{
			for ( var x = 0; x < height; x++ )
			{
				for ( var y = 0; y < width; y++ )
				{
					positions.Add( new Vector2I( position.X + x, position.Y + y ) );
				}
			}
		}
		else if ( rotation == ItemRotation.West )
		{
			for ( var x = 0; x < height; x++ )
			{
				for ( var y = 0; y < width; y++ )
				{
					positions.Add( new Vector2I( position.X - x, position.Y + y ) );
				}
			}
		}

		foreach ( var pos in positions )
		{
			/*if ( IsOutsideGrid( pos ) ) return true;

			if ( Items.TryGetValue( Vector2IToString( pos ), out var dict ) )
			{
				if ( dict.ContainsKey( placement ) ) return true;
			}*/
			if ( GetItems( pos ).Any() )
			{
				GD.Print( $"Found item at {pos}" );
				return false;
			}

			if ( BlockedGridPositions.Contains( pos ) )
			{
				GD.Print( $"Found blocked grid position at {pos}" );
				return false;
			}
		}

		return true;
	}

	public T SpawnPlacedItem<T>( ItemData item, Vector2I position, ItemPlacement placement,
		ItemRotation rotation ) where T : WorldItem
	{
		if ( !item.Placements.HasFlag( placement ) )
		{
			throw new Exception( $"Item {item} does not support placement {placement}" );
		}

		if ( IsOutsideGrid( position ) )
		{
			throw new Exception( $"Position {position} is outside the grid" );
		}

		if ( !CanPlaceItem( item, position, rotation, placement ) )
		{
			throw new Exception( $"Cannot place item {item} at {position} with placement {placement}" );
		}

		var itemInstance = item.PlaceScene.Instantiate<T>();
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
		// AddChild( itemInstance );
		CallDeferred( Node.MethodName.AddChild, itemInstance );
		// GD.Print( $"Spawned item {itemInstance} at {position} with placement {placement} and rotation {rotation}" );
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

		if ( !CanPlaceItem( item, position, rotation, placement ) )
		{
			throw new Exception( $"Cannot place item {item} at {position} with placement {placement}" );
		}

		var scene = item.DropScene;

		if ( item.DropScene == null )
		{
			// throw new Exception( $"Item {item} does not have a drop scene" );
			scene = GD.Load<PackedScene>( "res://items/misc/dropped_item.tscn" );
		}

		var itemInstance = scene.Instantiate<DroppedItem>();
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
			worldItem = SpawnPlacedItem<PlacedItem>( item, position, placement, dto.GridRotation );
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
		DebugPrint();
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
				if ( dict.Count == 0 )
				{
					Items.Remove( positionString );
				}

				GD.Print( $"Removed item {item} at {position} with placement {placement}" );
				DebugPrint();
			}
		}
	}

	public void DebugPrint()
	{
		return;
		foreach ( var item in Items )
		{
			GD.Print( $"Item at {item.Key}" );
			foreach ( var placement in item.Value )
			{
				GD.Print( $"  {placement.Key}: {placement.Value}" );
			}
		}
	}

	private void UpdateTransform( Vector2I position, ItemPlacement placement )
	{
		var positionString = Vector2IToString( position );
		var item = Items.TryGetValue( positionString, out var dict ) ? dict[placement] : null;
		if ( item == null ) throw new Exception( $"Failed to find item at {position} with placement {placement}" );

		var newPosition = ItemGridToWorld( position );
		var newRotation = GetRotation( item.GridRotation );

		item.Transform = new Transform3D( new Basis( newRotation ), newPosition );

		GD.Print(
			$"Updated transform of {item.Name} to {item.Transform} (position: {newPosition} (grid: {position}), rotation: {newRotation} (grid: {item.GridRotation}))" );
	}

	public bool CheckGridPositionEligibility( Vector2I position, out Vector3 worldPosition )
	{
		if ( IsOutsideGrid( position ) )
		{
			worldPosition = Vector3.Zero;
			return false;
		}

		if ( BlockedGridPositions.Contains( position ) )
		{
			worldPosition = Vector3.Zero;
			return false;
		}

		// trace a ray from the sky straight down in each corner, if height is the same on all corners then it's a valid position

		var basePosition = ItemGridToWorld( position );

		var margin = GridSizeCenter * 0.8f;

		var topLeft = new Vector3( basePosition.X - margin, 50, basePosition.Z - margin );
		var topRight = new Vector3( basePosition.X + margin, 50, basePosition.Z - margin );
		var bottomLeft = new Vector3( basePosition.X - margin, 50, basePosition.Z + margin );
		var bottomRight = new Vector3( basePosition.X + margin, 50, basePosition.Z + margin );

		var spaceState = GetWorld3D().DirectSpaceState;

		var traceTopLeft =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( topLeft, new Vector3( topLeft.X, -50, topLeft.Z ) ) );
		var traceTopRight =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( topRight, new Vector3( topRight.X, -50, topRight.Z ) ) );
		var traceBottomLeft = new Trace( spaceState ).CastRay(
			PhysicsRayQueryParameters3D.Create( bottomLeft, new Vector3( bottomLeft.X, -50, bottomLeft.Z ) ) );
		var traceBottomRight = new Trace( spaceState ).CastRay(
			PhysicsRayQueryParameters3D.Create( bottomRight, new Vector3( bottomRight.X, -50, bottomRight.Z ) ) );

		if ( traceTopLeft == null || traceTopRight == null || traceBottomLeft == null || traceBottomRight == null )
		{
			worldPosition = Vector3.Zero;
			return false;
		}

		var heightTopLeft = traceTopLeft.Position.Y;
		var heightTopRight = traceTopRight.Position.Y;
		var heightBottomLeft = traceBottomLeft.Position.Y;
		var heightBottomRight = traceBottomRight.Position.Y;

		if ( heightTopLeft != heightTopRight || heightTopLeft != heightBottomLeft ||
		     heightTopLeft != heightBottomRight )
		{
			worldPosition = Vector3.Zero;
			return false;
		}

		worldPosition = new Vector3( basePosition.X, heightTopLeft, basePosition.Z );

		return true;


		/*var rayStart = new Vector3( ItemGridToWorld( position ).X, 50, ItemGridToWorld( position ).Z );
		var rayEnd = new Vector3( ItemGridToWorld( position ).X, -50, ItemGridToWorld( position ).Z );
		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create( rayStart, rayEnd );
		var traceResult = new Trace( spaceState ).CastRay( query );

		if ( traceResult == null )
		{
			worldPosition = Vector3.Zero;
			return false;
		}

		worldPosition = traceResult.Position;

		return traceResult.Normal.Dot( Vector3.Up ) > 0.9f;*/
	}

	public Vector3 ItemGridToWorld( Vector2I gridPosition )
	{
		// return new Vector3( gridPosition.X + GridSizeCenter, 0, gridPosition.Y + GridSizeCenter );
		return new Vector3(
			(gridPosition.X * GridSize) + GridSizeCenter + Position.X,
			GridPositionHeights.GetValueOrDefault( gridPosition, Position.Y ),
			(gridPosition.Y * GridSize) + GridSizeCenter + Position.Z
		);
	}

	public Vector2I WorldToItemGrid( Vector3 worldPosition )
	{
		// return new Vector2I( (int)(worldPosition.X / GridSize), (int)(worldPosition.Z / GridSize) );
		return new Vector2I(
			(int)((worldPosition.X - Position.X) / GridSize),
			(int)((worldPosition.Z - Position.Z) / GridSize)
		);
	}

	public Vector2I GetAcreFromGridPosition( Vector2I gridPosition )
	{
		if ( !UseAcres ) return new Vector2I( 0, 0 );

		return new Vector2I( (int)Math.Floor( gridPosition.X / (float)AcreWidth ),
			(int)Math.Floor( gridPosition.Y / (float)AcreHeight ) );
	}

	public Vector2I GetAcreFromWorldPosition( Vector3 worldPosition )
	{
		return GetAcreFromGridPosition( WorldToItemGrid( worldPosition ) );
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

	public Direction Get4Direction( float angle )
	{
		var snapAngle = Mathf.Round( angle / 90 ) * 90;
		// Log.Info( $"Snap angle: {snapAngle}" );
		switch ( snapAngle )
		{
			case 0:
				return Direction.South;
			case 90:
				return Direction.East;
			case 180:
			case -180:
				return Direction.North;
			case -90:
				return Direction.West;
		}

		return Direction.North;
	}

	public ItemRotation GetItemRotationFromDirection( Direction direction )
	{
		return direction switch
		{
			Direction.North => ItemRotation.North,
			Direction.South => ItemRotation.South,
			Direction.West => ItemRotation.West,
			Direction.East => ItemRotation.East,
			Direction.NorthWest => ItemRotation.North,
			Direction.NorthEast => ItemRotation.East,
			Direction.SouthWest => ItemRotation.West,
			Direction.SouthEast => ItemRotation.South,
			_ => ItemRotation.North
		};
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

		if ( Items == null )
		{
			throw new Exception( "Items is null" );
		}

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

	public void AddPlacementBlocker( Area3D placementBlocker )
	{
		if ( placementBlocker == null ) throw new Exception( "Placement blocker is null" );

		var positions = new List<Vector2I>();

		// var basePosition = WorldToItemGrid( placementBlocker.GlobalTransform.Origin );
		// var shapeNode = placementBlocker.GetNode<CollisionShape3D>( "CollisionShape3D" );

		// if ( shapeNode == null ) throw new Exception( "Shape node is null" );

		foreach ( var child in placementBlocker.GetChildren() )
		{
			if ( child is CollisionShape3D shapeNode )
			{
				var shapeNodePosition = shapeNode.GlobalPosition;
				var shape = shapeNode.Shape;
				if ( shape is BoxShape3D box )
				{
					// var size = box.Size;
					var bbox = new BBox( box );
					bbox.Rotate( placementBlocker.GlobalTransform.Basis.GetRotationQuaternion() );
					bbox.Translate( shapeNodePosition );

					// 
					// bbox.Draw( GetTree() );

					GD.Print( $"Adding placement blocker at {bbox.Min} to {bbox.Max}" );

					for ( var x = 0; x < GridWidth; x++ )
					{
						for ( var y = 0; y < GridHeight; y++ )
						{
							var gridPos = new Vector2I( x, y );
							var worldPos = ItemGridToWorld( gridPos ) /*+
										   new Vector3( GridSize / 2f, 0, GridSize / 2f )*/;

							if ( bbox.Contains( worldPos ) )
							{
								positions.Add( gridPos );
								GD.Print( $"Blocked grid position {gridPos} ({worldPos})" );
							}
							else
							{
								// GD.Print( $"No blocker at {gridPos} ({worldPos})" );
							}
						}
					}
				}
				else
				{
					throw new Exception( $"Shape type {shape} is not supported" );
				}
			}
		}

		if ( positions.Count == 0 )
		{
			GD.Print( "No positions found" );
			return;
		}

		// BlockedGridPositions.AddRange( positions );
		foreach ( var pos in positions )
		{
			BlockedGridPositions.Add( pos );
		}
	}
}
