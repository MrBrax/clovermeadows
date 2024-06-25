using System;
using System.Threading.Tasks;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;
using vcrossing.Code.Save;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code;

/// <summary>
/// The base world for the <see cref="WorldManager"/>. Any item placed in the game world has to be a child of this node, this includes items that are dropped on the ground and items that are placed in the world.
/// <br/><strong>Note that NPCs and the player are not children of the world, they are separate nodes.</strong>
/// </summary>
public partial class World : Node3D
{

	/// <summary>
	/// Terrain layer is 10 in the editor
	/// </summary>
	public static uint TerrainLayer = 512;

	/// <summary>
	/// Water layer is 11 in the editor
	/// </summary>
	public static uint WaterLayer = 1024;

	/// <summary>
	/// Default scene to spawn when dropping an item. Will only work if the item has dropping enabled.
	/// </summary>
	public static string DefaultDropScene = "res://items/misc/dropped_item.tscn";

	/// <summary>
	/// Will be used to save the world to a file, should be unique. Do not use the same id for multiple worlds, this includes stories inside a building.
	/// </summary>
	[Export] public string WorldId { get; set; }
	[Export] public string WorldName { get; set; }
	[Export( PropertyHint.File, "*.tres" )]
	public string WorldPath { get; set; }

	[Export] public bool IsInside { get; set; }

	[Flags]
	public enum ItemPlacement
	{
		Wall = 1 << 0,
		OnTop = 1 << 1,
		Floor = 1 << 2,
		Underground = 1 << 3
	}

	public enum ItemPlacementType
	{
		Placed = 1,
		Dropped = 2,
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

	/// <summary>
	/// Since the Godot unit system is 1 unit = 1 meter, it's just easier to use 1 unit = 1 grid size.
	/// </summary>
	public const int GridSize = 1;

	public const float GridSizeCenter = GridSize / 2f;

	// public const int GridWidth = 16;
	// public const int GridHeight = 16;
	[Export] public int GridWidth { get; set; } = 16;
	[Export] public int GridHeight { get; set; } = 16;
	[Export] public bool UseAcres { get; set; } = false;
	[Export] public int AcreWidth { get; set; } = 16;
	[Export] public int AcreHeight { get; set; } = 16;

	/// <summary>
	/// The items in the world, the key is the grid position and the value is a dictionary of the placement and the item.
	/// Do NOT add nodes directly to the world, use <see cref="AddItem"/> instead so it can be properly tracked.
	/// </summary>
	public Dictionary<string, Dictionary<ItemPlacement, WorldNodeLink>> Items = new();

	/// <summary>
	/// The player cannot place items on these grid positions. This is used to block off areas that are not accessible.
	/// </summary>
	private HashSet<Vector2I> BlockedGridPositions = new();

	/// <summary>
	/// Since we only use one flat grid, we need to check the terrain at each grid position to properly place items at the ground level.
	/// </summary>
	private Dictionary<Vector2I, float> GridPositionHeights = new();

	public float CurrentTime => (float)(Time.GetUnixTimeFromSystem() % 86400);

	// public event Action OnTerrainChecked;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		// SpawnNode( GD.Load<WallpaperData>( "res://wallpaper/test.tres" ), new Vector2I( 3, 42 ), ItemRotation.North, ItemPlacement.Floor, true );
		// SpawnNode( GD.Load<WallpaperData>( "res://wallpaper/test2.tres" ), new Vector2I( 4, 42 ), ItemRotation.North, ItemPlacement.Floor, true );

		// node.GetNode<Wallpaper>().WallpaperDataPath = "res://wallpaper/test.tres";
		/*Logger.Info( $"World ready" );
		try
		{
			SpawnPlacedItem<PlacedItem>( GD.Load<ItemData>( "res://items/furniture/polka_chair/polka_chair.tres" ),
				new Vector2I( 0, 5 ),
				ItemPlacement.Floor, ItemRotation.North );
		}
		catch ( Exception e )
		{
			Logger.Info( e );
		}

		try
		{
			SpawnPlacedItem<PlacedItem>( GD.Load<ItemData>( "res://items/furniture/single_bed/single_bed.tres" ),
				new Vector2I( 0, 7 ),
				ItemPlacement.Floor, ItemRotation.North );
		}
		catch ( Exception e )
		{
			Logger.Info( e );
		}

		try
		{
			SpawnPlacedItem<PlacedItem>( GD.Load<ItemData>( "res://items/furniture/armchair/armchair.tres" ),
				new Vector2I( 0, 9 ),
				ItemPlacement.Floor, ItemRotation.North );
		}
		catch ( Exception e )
		{
			Logger.Info( e );
		}
		*/

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

		// CheckTerrain();
		// CallDeferred( nameof ( CheckTerrain ) );
	}

	public bool IsBlockedGridPosition( Vector2I position )
	{
		if ( BlockedGridPositions.Contains( position ) ) return true;

		CheckTerrainAt( position );

		return BlockedGridPositions.Contains( position );
	}

	public float GetHeightAt( Vector2I position )
	{
		if ( GridPositionHeights.TryGetValue( position, out var height ) )
		{
			return height;
		}

		CheckTerrainAt( position );

		if ( GridPositionHeights.TryGetValue( position, out height ) )
		{
			return height;
		}

		return 0;
	}

	/*public void CheckTerrain()
	{
		Logger.Info( "CheckTerrain", "Checking terrain" );
		for ( var x = 0; x < GridWidth; x++ )
		{
			for ( var y = 0; y < GridHeight; y++ )
			{
				var gridPos = new Vector2I( x, y );
				var check = CheckGridPositionEligibility( gridPos, out var worldPos );

				if ( worldPos.Y != 0 )
				{
					GridPositionHeights[gridPos] = worldPos.Y;
					// Logger.Info( $"Adding grid position height {gridPos} = {worldPos.Y}" );
				}

				if ( !check )
				{
					BlockedGridPositions.Add( gridPos );
					// Logger.Info( $"Blocking grid position from terrain check: {gridPos} (height: {worldPos.Y})" );
					// GetTree().CallGroup( "debugdraw", "add_line", ItemGridToWorld( gridPos ), ItemGridToWorld( gridPos ) + new Vector3( 0, 10, 0 ), new Color( 1, 0, 0 ), 15 );
				}
				else
				{
					// GetTree().CallGroup( "debugdraw", "add_line", ItemGridToWorld( gridPos ), ItemGridToWorld( gridPos ) + new Vector3( 0, 10, 0 ), new Color( 0, 1, 0 ), 15 );
				}
			}
		}

		/*using var cacheFile = FileAccess.Open( "user://grid_height.bin", FileAccess.ModeFlags.Write );
		/*cacheFile.StoreString(
			JsonSerializer.Serialize(
				GridPositionHeights.Select( x => new { Key = $"{x.Key.X},{x.Key.Y}", Value = x.Value } )
					.ToDictionary( x => x.Key, x => x.Value ) ) );
		cacheFile.Close();#2#
		cacheFile.StorePascalString( WorldName );
		cacheFile.Store32( (uint)GridPositionHeights.Count() );
		foreach ( var kvp in GridPositionHeights )
		{
			cacheFile.Store16( (ushort)kvp.Key.X );
			cacheFile.Store16( (ushort)kvp.Key.Y );
			cacheFile.StoreFloat( kvp.Value );
		}

		cacheFile.Close();#1#

		Logger.Info( "CheckTerrain", $"Terrain checked, {BlockedGridPositions.Count} blocked positions and {GridPositionHeights.Count} heights" );

		OnTerrainChecked?.Invoke();
	}*/

	public void CheckTerrainAt( Vector2I position )
	{
		var check = CheckGridPositionEligibility( position, out var worldPos );

		if ( worldPos.Y != 0 )
		{
			GridPositionHeights[position] = worldPos.Y;
			// Logger.Info( $"Adding grid position height {gridPos} = {worldPos.Y}" );
		}

		if ( !check )
		{
			BlockedGridPositions.Add( position );
			// Logger.Info( $"Blocking grid position from terrain check: {gridPos} (height: {worldPos.Y})" );
			// GetTree().CallGroup( "debugdraw", "add_line", ItemGridToWorld( gridPos ), ItemGridToWorld( gridPos ) + new Vector3( 0, 10, 0 ), new Color( 1, 0, 0 ), 15 );
		}
		else
		{
			// GetTree().CallGroup( "debugdraw", "add_line", ItemGridToWorld( gridPos ), ItemGridToWorld( gridPos ) + new Vector3( 0, 10, 0 ), new Color( 0, 1, 0 ), 15 );
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process( double delta )
	{
		SaveData?.ProcessQueuedItemLoads();
	}

	public WorldSaveData SaveData;

	public void Save()
	{
		/* var worldSave = new WorldSaveData();
		worldSave.LoadFile( "user://world.json" );
		worldSave.SaveWorldItems( this );
		worldSave.SaveFile( "user://world.json" ); */
		DirAccess.MakeDirAbsolute( "user://worlds" );
		SaveData.SaveWorld( this );
		SaveData.SaveFile( $"user://worlds/{WorldId}.json" );
	}

	public async Task Load()
	{
		/* Logger.Info( $"Loading world {WorldName}" );
		var save = new WorldSaveData();
		if ( save.LoadFile( "user://world.json" ) )
		{
			save.LoadWorldItems( this );
		} */
		DirAccess.MakeDirAbsolute( "user://worlds" );
		var saveData = WorldSaveData.LoadFile( $"user://worlds/{WorldId}.json" );
		if ( saveData != null )
		{
			SaveData = saveData;
			await SaveData.LoadWorldItems( this );
		}
		else
		{
			SaveData = new WorldSaveData();
			Logger.Warn( $"Failed to load world {WorldName}" );
		}
	}

	/// <summary>
	/// Sets up nodes in the world that have the <see cref="IWorldItem.IsPlacedInEditor"/> flag set to true.
	/// </summary>
	public void LoadEditorPlacedItems()
	{

		var iWorldItems = FindChildren( "*" ).OfType<IWorldItem>().Where( x => x.IsPlacedInEditor ).ToList();

		Logger.Info( $"Loading {iWorldItems.Count} editor placed iWorlditems for world {WorldName}" );
		foreach ( var item in iWorldItems )
		{
			if ( item is not Node3D node )
			{
				Logger.Warn( $"Item {item} is not a Node3D" );
				continue;
			}

			Logger.Info( "World", $"Loading editor placed item {node.Name} ({node})" );

			var gridPosition = WorldToItemGrid( node.GlobalTransform.Origin );

			if ( GetItems( gridPosition ).Any( x => x.GridPlacement == item.Placement ) )
			{
				Logger.Warn( $"Item already exists at {gridPosition} with placement {item.Placement}" );
				continue;
			}

			AddItem( gridPosition, item.Placement, node );
			Logger.Info( "World",
				$"Loaded editor placed item {node.Name} ({item}) at {gridPosition} ({node.GlobalTransform.Origin})" );
		}

		/*var carriables = FindChildren( "*" ).OfType<BaseCarriable>().Where( x => x.IsPlacedInEditor ).ToList();
		Logger.Info( $"Loading {carriables.Count} editor placed carriables for world {WorldName}" );
		foreach ( var item in carriables )
		{
			Logger.Info( $"Loading editor placed carriable {item}" );

			var gridPosition = WorldToItemGrid( item.GlobalTransform.Origin );

			if ( GetItems( gridPosition ).Any( x => x.GridPlacement == ItemPlacement.Floor ) )
			{
				Logger.Warn( $"Item already exists at {gridPosition}" );
				continue;
			}

			AddItem( gridPosition, ItemPlacement.Floor, item );
			Logger.Info( $"Loaded editor placed carriable {item} at {gridPosition}" );
		}*/
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

	public Quaternion GetRotation( Direction direction )
	{
		return direction switch
		{
			Direction.North => new Quaternion( 0, 0, 0, 1 ),
			Direction.East => new Quaternion( 0, 0.7071068f, 0, 0.7071068f ),
			Direction.South => new Quaternion( 0, 1, 0, 0 ),
			Direction.West => new Quaternion( 0, -0.7071068f, 0, 0.7071068f ),
			Direction.NorthWest => new Quaternion( 0, -0.7071068f, 0, 0.7071068f ),
			Direction.NorthEast => new Quaternion( 0, 0.7071068f, 0, 0.7071068f ),
			Direction.SouthWest => new Quaternion( 0, -1, 0, 0 ),
			Direction.SouthEast => new Quaternion( 0, 1, 0, 0 ),
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
			Logger.Warn( "CanPlaceItem", $"Position {position} is outside the grid" );
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

		// check any nearby items
		foreach ( var pos in positions )
		{
			if ( IsBlockedGridPosition( pos ) )
			{
				Logger.Warn( "CanPlaceItem", $"Found blocked grid position at {pos}" );
				return false;
			}

			/*if ( GetItems( pos ).Any() )
			{
				Logger.Warn("CanPlaceItem", $"Found item at {pos}" );
				return false;
			}*/

			if ( Items.TryGetValue( Vector2IToString( pos ), out var dict ) )
			{
				if ( dict.ContainsKey( placement ) )
				{
					Logger.Warn( "CanPlaceItem", $"Found item at {pos} with placement {placement}" );
					return false;
				}
			}
		}

		return true;
	}

	public WorldNodeLink SpawnNode( ItemData item, Vector2I position, ItemRotation rotation, ItemPlacement placement,
		bool dropped = false )
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
			throw new Exception( $"Cannot place item {item.Name} ({item.GetType}) at {position} with placement {placement}" );
		}

		PackedScene sceneToSpawn;

		if ( dropped && item is IEquipableData )
		{
			sceneToSpawn = item.CarryScene;
		}
		else if ( dropped )
		{
			if ( item.DropScene != null )
			{
				sceneToSpawn = item.DropScene;
			}
			else
			{
				sceneToSpawn = Loader.LoadResource<PackedScene>( DefaultDropScene );
			}
		}
		else
		{
			sceneToSpawn = item.PlaceScene;
		}

		if ( sceneToSpawn == null )
		{
			throw new Exception( $"Item {item} does not have a scene" );
		}

		var itemInstance = sceneToSpawn.Instantiate<Node3D>();
		if ( !IsInstanceValid( itemInstance ) )
		{
			throw new Exception( $"Failed to instantiate item {item}" );
		}

		var nodeLink = AddItem( position, placement, itemInstance );

		nodeLink.GridRotation = rotation;
		nodeLink.PlacementType = dropped ? ItemPlacementType.Dropped : ItemPlacementType.Placed;
		nodeLink.ItemDataPath = item.ResourcePath;
		nodeLink.ItemScenePath = sceneToSpawn.ResourcePath;

		UpdateTransform( position, placement );

		return nodeLink;
	}

	public Node3D SpawnPersistentNode( PersistentItem item, Vector2I position, ItemRotation rotation,
		ItemPlacement placement, bool dropped = false )
	{
		var itemData = item.ItemData;

		if ( !itemData.Placements.HasFlag( placement ) )
		{
			throw new Exception( $"Item {item} does not support placement {placement}" );
		}

		if ( IsOutsideGrid( position ) )
		{
			throw new Exception( $"Position {position} is outside the grid" );
		}

		if ( !CanPlaceItem( itemData, position, rotation, placement ) )
		{
			throw new Exception( $"Cannot place item {item.GetName()} ({item.GetType}) at {position} with placement {placement}" );
		}

		PackedScene sceneToSpawn;

		if ( dropped )
		{
			sceneToSpawn = itemData.DropScene != null ? itemData.DropScene : Loader.LoadResource<PackedScene>( DefaultDropScene );
		}
		else
		{
			sceneToSpawn = itemData.PlaceScene;
		}

		if ( sceneToSpawn == null )
		{
			throw new Exception( $"Item {item} does not have a scene" );
		}

		var itemInstance = sceneToSpawn.Instantiate<Node3D>();

		if ( itemInstance == null )
		{
			throw new Exception( $"Failed to instantiate item {item}" );
		}

		item.SetNodeData( itemInstance );

		var nodeLink = AddItem( position, placement, itemInstance );

		nodeLink.GridRotation = rotation;
		nodeLink.PlacementType = dropped ? ItemPlacementType.Dropped : ItemPlacementType.Placed;
		nodeLink.ItemDataPath = itemData.ResourcePath;
		nodeLink.ItemScenePath = sceneToSpawn.ResourcePath;

		UpdateTransform( position, placement );

		return itemInstance;
	}

	public static string Vector2IToString( Vector2I vector )
	{
		return $"{vector.X},{vector.Y}";
	}

	public static Vector2I StringToVector2I( string str )
	{
		var split = str.Split( ',' );
		return new Vector2I( int.Parse( split[0] ), int.Parse( split[1] ) );
	}

	public WorldNodeLink AddItem( Vector2I position, ItemPlacement placement, Node3D item )
	{
		if ( IsOutsideGrid( position ) )
		{
			throw new Exception( $"Position {position} is outside the grid" );
		}

		/*if ( !item.GetItemData().Placements.HasFlag( placement ) )
		{
			throw new Exception( $"Item {item} does not support placement {placement}" );
		}*/

		var nodeLink = new WorldNodeLink( this, item );

		var positionString = Vector2IToString( position );

		if ( Items.TryGetValue( positionString, out var dict ) )
		{
			dict[placement] = nodeLink;
		}
		else
		{
			Items[positionString] = new Dictionary<ItemPlacement, WorldNodeLink> { { placement, nodeLink } };
		}

		nodeLink.GridPosition = position;
		nodeLink.GridPlacement = placement;

		if ( !item.IsInsideTree() )
		{
			// Logger.Warn( $"Added item {item} is not inside the node tree" );
			AddChild( item );
		}
		else if ( item.GetParent() != this )
		{
			Logger.Warn( "World", $"Added item {item} is not a child of world" );
		}

		Logger.Info( "World", $"Added item {nodeLink.GetName()} at {position} with placement {placement}" );
		UpdateTransform( position, placement );

		// Save();
		DebugPrint();

		return nodeLink;
	}

	public void ImportNodeLink( WorldNodeLink nodeLink, Node3D item )
	{
		if ( IsOutsideGrid( nodeLink.GridPosition ) )
		{
			throw new Exception( $"Position {nodeLink.GridPosition} is outside the grid" );
		}

		var positionString = Vector2IToString( nodeLink.GridPosition );

		if ( Items.TryGetValue( positionString, out var dict ) )
		{
			dict[nodeLink.GridPlacement] = nodeLink;
		}
		else
		{
			Items[positionString] =
				new Dictionary<ItemPlacement, WorldNodeLink> { { nodeLink.GridPlacement, nodeLink } };
		}

		nodeLink.Node = item;
		nodeLink.World = this;

		if ( !item.IsInsideTree() )
		{
			// Logger.Warn( $"Added item {item} is not inside the node tree" );
			AddChild( item );
		}
		else if ( item.GetParent() != this )
		{
			Logger.Warn( "World", $"Added item {item} is not a child of world" );
		}

		Logger.Info( "World",
			$"Imported item {nodeLink.GetName()} at {nodeLink.GridPosition} with placement {nodeLink.GridPlacement}" );
		UpdateTransform( nodeLink.GridPosition, nodeLink.GridPlacement );

		// Save();
		DebugPrint();
	}


	/// <summary>
	/// Removes an item from the world at the specified position and placement.
	/// </summary>
	/// <param name="position">The position of the item to remove.</param>
	/// <param name="placement">The placement of the item to remove.</param>
	/// <remarks>
	/// Do NOT remove nodes directly from the world, use this method instead.
	/// </remarks>
	public void RemoveItem( Vector2I position, ItemPlacement placement )
	{
		var positionString = Vector2IToString( position );
		if ( Items.TryGetValue( positionString, out var dict ) )
		{
			if ( dict.ContainsKey( placement ) )
			{
				var nodeLink = dict[placement];
				nodeLink.DestroyNode();
				dict.Remove( placement );
				if ( dict.Count == 0 )
				{
					Logger.Info( $"Removed last item at {position}" );
					Items.Remove( positionString );
				}

				Logger.Info( $"Removed item {nodeLink} at {position} with placement {placement}" );
				DebugPrint();
			}
			else
			{
				Logger.Warn( $"No item at {position} with placement {placement}" );
			}
		}
		else
		{
			Logger.Warn( $"No items at {position}" );
		}
	}

	public void RemoveItem( Node3D node )
	{
		// RemoveItem( item.GridPosition, item.Placement );
		var nodeLink = Items.Values.SelectMany( x => x.Values ).FirstOrDefault( x => x.Node == node );
		if ( nodeLink == null )
		{
			throw new Exception( $"Failed to find node link for {node}" );
		}

		RemoveItem( nodeLink.GridPosition, nodeLink.GridPlacement );
	}

	public void RemoveItem( WorldNodeLink nodeLink )
	{
		RemoveItem( nodeLink.GridPosition, nodeLink.GridPlacement );
	}

	public void DebugPrint()
	{
		return;
		Logger.Info( $"Items in world {WorldName}:" );
		foreach ( var item in Items )
		{
			Logger.Info( $"- Items at {item.Key}:" );
			foreach ( var placement in item.Value )
			{
				Logger.Info( $"  {placement.Key}: {placement.Value}" );
			}
		}
	}


	/// <summary>
	/// Updates the transform of an item in the world based on its grid position and placement.
	/// Should always be called after adding or moving an item.
	/// </summary>
	public void UpdateTransform( Vector2I position, ItemPlacement placement )
	{
		var positionString = Vector2IToString( position );
		var nodeLink = Items.TryGetValue( positionString, out var dict ) ? dict[placement] : null;
		if ( nodeLink == null || !nodeLink.IsValid() )
			throw new Exception( $"Failed to find item at {position} with placement {placement}" );

		if ( !nodeLink.Node.IsInsideTree() ) throw new Exception( $"Item {nodeLink} is not inside the node tree" );

		var newPosition = ItemGridToWorld( position );
		var newRotation = GetRotation( nodeLink.GridRotation );

		if ( placement == ItemPlacement.Underground )
		{
			newPosition = new Vector3( newPosition.X, -50, newPosition.Z );
		}
		else if ( placement == ItemPlacement.OnTop )
		{
			var floorNodeLink = GetItem( position, ItemPlacement.Floor );
			if ( floorNodeLink == null )
			{
				Logger.Warn( "UpdateTransform", $"No floor item at {position}" );
				return;
			}

			var onTopNode = floorNodeLink.GetPlaceableNodeAtGridPosition( position );
			if ( onTopNode == null )
			{
				Logger.Warn( "UpdateTransform", $"No on top node at {position}" );
				return;
			}

			Logger.Info( $"Updating transform of {nodeLink.GetName()} to be on top of {onTopNode}" );
			newPosition = onTopNode.GlobalTransform.Origin;
		}

		nodeLink.Node.Transform = new Transform3D( new Basis( newRotation ), newPosition );

		Logger.Info( "UpdateTransform",
			$"Updated transform of {nodeLink.GetName()} to {nodeLink.Node.GlobalPosition}, {nodeLink.Node.GlobalRotationDegrees}" );
	}

	public bool CheckGridPositionEligibility( Vector2I position, out Vector3 worldPosition )
	{
		if ( IsOutsideGrid( position ) )
		{
			Logger.Debug( "EligibilityCheck", $"Position {position} is outside the grid" );
			worldPosition = Vector3.Zero;
			return false;
		}

		if ( BlockedGridPositions.Contains( position ) )
		{
			Logger.Debug( "EligibilityCheck", $"Position {position} is already blocked" );
			worldPosition = Vector3.Zero;
			return false;
		}

		// trace a ray from the sky straight down in each corner, if height is the same on all corners then it's a valid position

		var basePosition = ItemGridToWorld( position, true );

		var margin = GridSizeCenter * 0.8f;
		var heightTolerance = 0.1f;

		var topLeft = new Vector3( basePosition.X - margin, 50, basePosition.Z - margin );
		var topRight = new Vector3( basePosition.X + margin, 50, basePosition.Z - margin );
		var bottomLeft = new Vector3( basePosition.X - margin, 50, basePosition.Z + margin );
		var bottomRight = new Vector3( basePosition.X + margin, 50, basePosition.Z + margin );

		var spaceState = GetWorld3D().DirectSpaceState;

		// uint collisionMask = 1010; // terrain is on layer 10
		// Logger.Info( "EligibilityCheck", $"Collision mask: {collisionMask}" );

		var traceTopLeft =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( topLeft, new Vector3( topLeft.X, -50, topLeft.Z ),
					TerrainLayer ) );
		var traceTopRight =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( topRight, new Vector3( topRight.X, -50, topRight.Z ),
					TerrainLayer ) );
		var traceBottomLeft = new Trace( spaceState ).CastRay(
			PhysicsRayQueryParameters3D.Create( bottomLeft, new Vector3( bottomLeft.X, -50, bottomLeft.Z ),
				TerrainLayer ) );
		var traceBottomRight = new Trace( spaceState ).CastRay(
			PhysicsRayQueryParameters3D.Create( bottomRight, new Vector3( bottomRight.X, -50, bottomRight.Z ),
				TerrainLayer ) );

		if ( traceTopLeft == null || traceTopRight == null || traceBottomLeft == null || traceBottomRight == null )
		{
			Logger.Warn( "ElegibilityCheck", $"Failed to trace rays at {position}" );
			worldPosition = Vector3.Zero;
			return false;
		}

		var heightTopLeft = traceTopLeft.Position.Y;
		var heightTopRight = traceTopRight.Position.Y;
		var heightBottomLeft = traceBottomLeft.Position.Y;
		var heightBottomRight = traceBottomRight.Position.Y;

		/*if ( heightTopLeft != heightTopRight || heightTopLeft != heightBottomLeft ||
		     heightTopLeft != heightBottomRight )
		{
			worldPosition = Vector3.Zero;
			return false;
		}*/

		if ( heightTopLeft <= -50 )
		{
			Logger.Warn( $"Height at {position} is below -50" );
		}

		// var averageHeight = (heightTopLeft + heightTopRight + heightBottomLeft + heightBottomRight) / 4;

		if ( Math.Abs( heightTopLeft - heightTopRight ) > heightTolerance ||
			 Math.Abs( heightTopLeft - heightBottomLeft ) > heightTolerance ||
			 Math.Abs( heightTopLeft - heightBottomRight ) > heightTolerance )
		{
			Logger.Debug( "ElegibilityCheck",
				$"Height difference at {position} is too high ({heightTopLeft}, {heightTopRight}, {heightBottomLeft}, {heightBottomRight})" );
			worldPosition = Vector3.Zero;
			return false;
		}

		worldPosition = new Vector3( basePosition.X, heightTopLeft, basePosition.Z );

		return true;

	}

	public Vector3 ItemGridToWorld( Vector2I gridPosition, bool noRecursion = false )
	{

		if ( GridSize == 0 ) throw new Exception( "Grid size is 0" );
		if ( GridSizeCenter == 0 ) throw new Exception( "Grid size center is 0" );

		var height = !noRecursion ? GetHeightAt( gridPosition ) : 0;

		return new Vector3(
			(gridPosition.X * GridSize) + GridSizeCenter + Position.X,
			height != 0 ? height : Position.Y,
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

	/* [Obsolete]
	public Vector2I GetAcreFromGridPosition( Vector2I gridPosition )
	{
		if ( !UseAcres ) return new Vector2I( 0, 0 );

		return new Vector2I( (int)Math.Floor( gridPosition.X / (float)AcreWidth ),
			(int)Math.Floor( gridPosition.Y / (float)AcreHeight ) );
	}

	[Obsolete]
	public Vector2I GetAcreFromWorldPosition( Vector3 worldPosition )
	{
		return GetAcreFromGridPosition( WorldToItemGrid( worldPosition ) );
	} */

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

	public static ItemRotation RandomItemRotation()
	{
		return (ItemRotation)GD.RandRange( 1, 4 );
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


	/// <summary>
	/// Retrieves the WorldNodeLinks at the specified grid position.
	/// This method will return items that are intersecting the grid position as well, if they are larger than 1x1.
	/// <br />Use <see cref="WorldNodeLink.Node"/> to get the actual node.
	/// </summary>
	/// <param name="gridPos">The grid position to retrieve items from.</param>
	/// <returns>An enumerable collection of WorldNodeLink items at the specified grid position.</returns>
	public IEnumerable<WorldNodeLink> GetItems( Vector2I gridPos )
	{
		if ( IsOutsideGrid( gridPos ) )
		{
			throw new Exception( $"Position {gridPos} is outside the grid" );
		}

		if ( Items == null )
		{
			throw new Exception( "Items is null" );
		}

		HashSet<WorldNodeLink> foundItems = new();

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

	/// <summary>
	///  Get a node link at a specific grid position and placement.
	///  Use <see cref="WorldNodeLink.Node"/> to get the node.
	/// </summary>
	/// <param name="gridPos"></param>
	/// <param name="placement"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public WorldNodeLink GetItem( Vector2I gridPos, ItemPlacement placement )
	{

		foreach ( var item in GetItems( gridPos ) )
		{
			if ( item.GridPlacement == placement )
			{
				return item;
			}
		}

		return null;
	}

	/// <summary>
	/// Set up a placement blocker which will prevent items from being placed in the specified area.
	/// An Area3D is used to define the bounds.
	/// </summary>
	public void AddPlacementBlocker( Area3D placementBlocker )
	{
		if ( !IsInstanceValid( placementBlocker ) ) throw new Exception( "Placement blocker is null" );

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

					Logger.Info( $"Adding placement blocker at {bbox.Min} to {bbox.Max}" );

					// Convert the min and max corners of the bbox to grid positions
					Vector2I minGridPos = WorldToItemGrid( bbox.Min );
					Vector2I maxGridPos = WorldToItemGrid( bbox.Max );

					// Ensure the grid positions are within the grid bounds
					minGridPos.X = Math.Max( 0, minGridPos.X );
					minGridPos.Y = Math.Max( 0, minGridPos.Y );
					maxGridPos.X = Math.Min( GridWidth - 1, maxGridPos.X );
					maxGridPos.Y = Math.Min( GridHeight - 1, maxGridPos.Y );

					for ( var x = minGridPos.X; x < maxGridPos.X; x++ )
					{
						for ( var y = minGridPos.Y; y < maxGridPos.Y; y++ )
						{
							var gridPos = new Vector2I( x, y );
							var worldPos = ItemGridToWorld( gridPos ) /*+
										   new Vector3( GridSize / 2f, 0, GridSize / 2f )*/;

							if ( bbox.Contains( worldPos ) )
							{
								positions.Add( gridPos );
								Logger.Info( "World", $"Blocked grid position {gridPos} ({worldPos})" );
							}
							else
							{
								// Logger.Info( $"No blocker at {gridPos} ({worldPos})" );
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
			Logger.Info( "World", $"No positions found for placement blocker {placementBlocker}" );
			return;
		}

		// BlockedGridPositions.AddRange( positions );
		foreach ( var pos in positions )
		{
			BlockedGridPositions.Add( pos );
		}
	}

	public void Unload()
	{
		Logger.Info( $"Unloading world {WorldName}" );
		Save();
	}

	public WorldNodeLink GetNodeLink( Node3D node )
	{
		return Items.Values.SelectMany( x => x.Values ).FirstOrDefault( x => x.Node == node );
	}

	public void LoadInteriors()
	{
		var interior = GetTree().GetNodesInGroup( "interior" ).Cast<HouseInterior>().FirstOrDefault();

		if ( !IsInstanceValid( interior ) )
		{
			Logger.Info( "World", "No interior found" );
			return;
		}

		interior.LoadRooms();
	}

	public void SetupInteriorCollisions()
	{
		var interior = GetTree().GetNodesInGroup( "interior" ).Cast<HouseInterior>().FirstOrDefault();

		if ( !IsInstanceValid( interior ) )
		{
			Logger.Info( "World", "No interior found" );
			return;
		}

		interior.SetupCollisions();

	}

	public void ActivateClasses()
	{
		var nodes = FindChildren( "*" );
		foreach ( var node in nodes )
		{
			if ( node is IWorldLoaded activatable )
			{
				activatable.WorldLoaded();
			}
		}
	}

}
