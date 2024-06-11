using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using vcrossing.Code.Helpers;
using vcrossing.Code.Persistence;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Save;

public partial class WorldSaveData : BaseSaveData
{
	// [JsonInclude] public Dictionary<string, Dictionary<World.ItemPlacement, BaseDTO>> WorldItems = new();


	[JsonInclude] public string Name;
	[JsonInclude] public Dictionary<string, Dictionary<World.ItemPlacement, NodeEntry>> Items = new();

	public struct NodeEntry
	{
		[JsonInclude] public PersistentItem Item;
		[JsonInclude] public WorldNodeLink NodeLink;
	}

	[JsonInclude] public Dictionary<int, string> Wallpapers = new();
	[JsonInclude] public Dictionary<int, string> Floors = new();


	/*public class Items
	{
		// world items
		// public Dictionary<string, Dictionary<World.ItemPlacement, BaseDTO>> World = new();

		// instance items (houses, etc)
		public Dictionary<string, Dictionary<string, BaseDTO>> Instances = new();

	}*/

	public void SaveWorldItems( World world )
	{
		// ClearInstance( world.WorldId );

		// add world items to the save data
		// var worldInstance = GetInstance( world.WorldId );
		// var items = Items;

		if ( Items == null )
		{
			Items = new();
		}

		// var items = world.Items.Duplicate( true );
		foreach ( var item in world.Items )
		{
			var position = item.Key;
			foreach ( var itemEntry in item.Value )
			{
				var placement = itemEntry.Key;
				var nodeLink = itemEntry.Value;

				if ( !nodeLink.ShouldBeSaved() )
				{
					Logger.Info( "SaveWorldItems", $"Skipping {nodeLink} at {position}" );
					continue;
				}

				if ( !Items.ContainsKey( position ) )
				{
					Items[position] = new();
				}

				// worldItem.UpdateDTO();
				// items[position][placement] = worldItem.DTO;

				PersistentItem persistentItem;

				try
				{
					persistentItem = PersistentItem.Create( nodeLink );
				}
				catch ( Exception e )
				{
					Logger.Warn( "SaveWorldItems", $"Failed to create persistent item for {nodeLink}: {e.Message}" );
					continue;
				}

				if ( string.IsNullOrEmpty( persistentItem.ItemDataPath ) )
				{
					Logger.Warn( "SaveWorldItems", $"Item data path is empty for {nodeLink}" );
					continue;
				}

				// persistentItem.PlacementType = nodeLink.PlacementType;

				Items[position][placement] = new NodeEntry
				{
					Item = persistentItem,
					NodeLink = nodeLink,
				};
			}
		}

		Logger.Info( "SaveWorldItems", $"Added {Items.Count} world items" );

		/*foreach ( var item in world.GetChildren() )
		{
			if ( item is WorldItem worldItem )
			{
				if ( !WorldItems.ContainsKey( worldItem.Name ) )
				{
					WorldItems[worldItem.Name] = new();
				}

				WorldItems[worldItem.Name][worldItem.Placement] = worldItem;
			}
		}*/
	}

	public static WorldSaveData LoadFile( string filePath )
	{
		if ( !FileAccess.FileExists( filePath ) )
		{
			// throw new System.Exception( $"File {filePath} does not exist" );
			Logger.Warn( $"File {filePath} does not exist" );
			return null;
		}

		using var file = FileAccess.Open( filePath, FileAccess.ModeFlags.Read );
		var json = file.GetAsText();
		var saveData =
			JsonSerializer.Deserialize<WorldSaveData>( json, new JsonSerializerOptions { IncludeFields = true, } );

		// WorldItems = saveData.WorldItems;
		// Instances = saveData.Instances;
		if ( saveData.Wallpapers == null ) saveData.Wallpapers = new();
		if ( saveData.Floors == null ) saveData.Floors = new();

		Logger.Info( "LoadFile", "Loaded save data from file" );

		return saveData;
	}

	public class QueuedItemLoad
	{
		public WorldNodeLink NodeLink;
		public PersistentItem PersistentItem;
		public string ItemScenePath;
		public Godot.Collections.Array Progress = new();
		public bool IsLoaded = false;
		public Resource LoadedResource = null;
	}

	private List<QueuedItemLoad> _queuedItemLoads = new();

	public async Task LoadWorldItems( World world )
	{
		var items = Items;

		if ( items == null || items.Count == 0 )
		{
			Logger.Info( "LoadWorldItems", "No items found" );
			return;
		}

		Logger.Info( "LoadWorldItems", $"Loading {items.Count} world items from save file" );
		foreach ( var item in items )
		{
			var position = World.StringToVector2I( item.Key );

			foreach ( var itemEntry in item.Value )
			{
				var placement = itemEntry.Key;
				var nodeEntry = itemEntry.Value;

				var persistentItem = nodeEntry.Item;
				var nodeLink = nodeEntry.NodeLink;

				if ( !nodeLink.ShouldBeSaved() )
				{
					Logger.Warn( "LoadWorldItems",
						$"Skipping {nodeLink} at {position}, should not have been saved" );
					continue;
				}

				var existingItem = world.GetItem( position, placement );
				if ( existingItem != null )
				{
					Logger.Warn( "LoadWorldItems",
						$"Item already exists at {position} ({persistentItem.PlacementType})" );
					continue;
				}

				Logger.Info( "LoadWorldItems",
					$"Loading {persistentItem.GetName()} at {position} ({persistentItem.PlacementType})" );

				nodeLink.LoadItemData();

				var error = ResourceLoader.LoadThreadedRequest( nodeLink.ItemScenePath );
				if ( error != Error.Ok )
				{
					Logger.Warn( "LoadWorldItems", $"Failed to load {nodeLink.ItemScenePath}: {error}" );
					continue;
				}

				_queuedItemLoads.Add( new QueuedItemLoad
				{
					NodeLink = nodeLink,
					PersistentItem = persistentItem,
					ItemScenePath = nodeLink.ItemScenePath,
				} );

				/* var packedScene = Loader.LoadResource<PackedScene>( nodeLink.ItemScenePath );
				var worldItem = packedScene.Instantiate<Node3D>();

				worldItem.Name = persistentItem.GetName();
				persistentItem.SetNodeData( worldItem );

				world.ImportNodeLink( nodeLink, worldItem ); */

			}
		}

		while ( _queuedItemLoads.Any( x => !x.IsLoaded ) )
		{
			await Task.Delay( 100 );
		}

		foreach ( var queuedItemLoad in _queuedItemLoads )
		{
			if ( queuedItemLoad.LoadedResource == null )
			{
				Logger.Warn( "LoadWorldItems", $"Failed to load {queuedItemLoad.ItemScenePath}" );
				continue;
			}

			var packedScene = queuedItemLoad.LoadedResource as PackedScene;
			var worldItem = packedScene.Instantiate<Node3D>();

			worldItem.Name = queuedItemLoad.PersistentItem.GetName();
			queuedItemLoad.PersistentItem.SetNodeData( worldItem );

			Logger.Info( "LoadWorldItems", $"Loaded {queuedItemLoad.PersistentItem.GetName()}" );

			world.ImportNodeLink( queuedItemLoad.NodeLink, worldItem );
		}
	}

	public void ProcessQueuedItemLoads()
	{
		for ( int i = 0; i < _queuedItemLoads.Count; i++ )
		{
			var queuedItemLoad = _queuedItemLoads[i];
			if ( queuedItemLoad.IsLoaded ) continue;

			var status = ResourceLoader.LoadThreadedGetStatus( queuedItemLoad.ItemScenePath, queuedItemLoad.Progress );

			if ( status == ResourceLoader.ThreadLoadStatus.Loaded )
			{
				var packedScene = ResourceLoader.LoadThreadedGet( queuedItemLoad.ItemScenePath );
				queuedItemLoad.LoadedResource = packedScene;
				queuedItemLoad.IsLoaded = true;
			}
			else if ( status == ResourceLoader.ThreadLoadStatus.Failed )
			{
				Logger.Warn( "ProcessQueuedItemLoads", $"Failed to load {queuedItemLoad.ItemScenePath}" );
				_queuedItemLoads.RemoveAt( i );
				i--;
			}

		}
	}
}
