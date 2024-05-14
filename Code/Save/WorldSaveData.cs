using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code.DTO;
using vcrossing2.Code.Persistence;

namespace vcrossing2.Code.Save;

public class WorldSaveData : BaseSaveData
{
	// [JsonInclude] public Dictionary<string, Dictionary<World.ItemPlacement, BaseDTO>> WorldItems = new();

	public struct WorldInstance
	{
		[JsonInclude] public string Name;
		[JsonInclude] public Dictionary<string, Dictionary<World.ItemPlacement, NodeEntry>> Items;
		
		public struct NodeEntry
		{
			[JsonInclude] public PersistentItem Item;
			[JsonInclude] public WorldNodeLink NodeLink;
		}

		// Misc items
		// [JsonInclude] public ???

		public WorldInstance( string name )
		{
			Name = name;
			Items = new();
		}
	}

	[JsonInclude] public Dictionary<string, WorldInstance> Instances;

	public WorldSaveData()
	{
		Instances = new Dictionary<string, WorldInstance>();
	}

	public WorldInstance GetInstance( string name )
	{
		if ( !Instances.ContainsKey( name ) )
		{
			Instances[name] = new WorldInstance( name );
		}

		return Instances[name];
	}

	public void ClearInstance( string name )
	{
		Instances[name] = new WorldInstance( name );
	}

	/*public class Items
	{
		// world items
		// public Dictionary<string, Dictionary<World.ItemPlacement, BaseDTO>> World = new();

		// instance items (houses, etc)
		public Dictionary<string, Dictionary<string, BaseDTO>> Instances = new();

	}*/

	public void SaveWorldItems( World world )
	{
		ClearInstance( world.WorldName );

		// add world items to the save data
		var worldInstance = GetInstance( world.WorldName );
		var items = worldInstance.Items;

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
					GD.Print( $"Skipping {nodeLink} at {position}" );
					continue;
				}

				if ( !items.ContainsKey( position ) )
				{
					items[position] = new();
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
					GD.PushWarning( $"Failed to create persistent item for {nodeLink}: {e.Message}" );
					continue;
				}

				if ( string.IsNullOrEmpty( persistentItem.ItemDataPath ) )
				{
					GD.PushWarning( $"Item data path is empty for {nodeLink}" );
					continue;
				}

				// persistentItem.PlacementType = nodeLink.PlacementType;

				items[position][placement] = new WorldInstance.NodeEntry
				{
					Item = persistentItem,
					NodeLink = nodeLink,
				};
			}
		}

		GD.Print( $"Added {items.Count} world items" );

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

	public bool LoadFile( string filePath )
	{
		if ( !FileAccess.FileExists( filePath ) )
		{
			// throw new System.Exception( $"File {filePath} does not exist" );
			GD.PushWarning( $"File {filePath} does not exist" );
			return false;
		}

		using var file = FileAccess.Open( filePath, FileAccess.ModeFlags.Read );
		var json = file.GetAsText();
		var saveData =
			JsonSerializer.Deserialize<WorldSaveData>( json, new JsonSerializerOptions { IncludeFields = true, } );

		// WorldItems = saveData.WorldItems;
		Instances = saveData.Instances;

		GD.Print( "Loaded save data from file" );

		return true;
	}

	public void LoadWorldItems( World world )
	{
		var items = GetInstance( world.WorldName ).Items;

		if ( items == null || items.Count == 0 )
		{
			GD.Print( "No items found" );
			return;
		}

		GD.Print( $"Loading {items.Count} world items from save file" );
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
					GD.PushWarning( $"Skipping {nodeLink} at {position}, should not have been saved" );
					continue;
				}
				
				var existingItem = world.GetItem( position, placement );
				if ( existingItem != null )
				{
					GD.PushWarning( $"Item already exists at {position} ({persistentItem.PlacementType})" );
					continue;
				}

				GD.Print( $"Loading {persistentItem.GetName()} at {position} ({persistentItem.PlacementType})" );

				Node3D worldItem;

				try
				{
					worldItem = persistentItem.CreateAuto();
				}
				catch ( Exception e )
				{
					GD.PushWarning( $"Failed to create world item for {persistentItem.GetName()}: {e.Message}" );
					continue;
				}
				
				worldItem.Name = persistentItem.GetName();

				// world.AddItem( position, placement, worldItem );
				world.ImportNodeLink( nodeLink, worldItem );

				/*try
				{
					// var worldItem = world.SpawnDto( dto, position, placement );
				}
				catch ( Exception e )
				{
					GD.Print( e.Message );
				}*/
				// worldItem.UpdatePositionAndRotation();
			}
		}
	}
}
