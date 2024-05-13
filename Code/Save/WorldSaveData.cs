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
		[JsonInclude] public Dictionary<string, Dictionary<World.ItemPlacement, PersistentItem>> Items;

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

	public void AddWorldItems( World world )
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
				var worldItem = itemEntry.Value;

				if ( !worldItem.ShouldBeSaved() )
				{
					GD.Print( $"Skipping {worldItem.Name} at {position}" );
					continue;
				}

				if ( !items.ContainsKey( position ) )
				{
					items[position] = new();
				}

				// worldItem.UpdateDTO();
				// items[position][placement] = worldItem.DTO;
				
				var persistentItem = PersistentItem.Create( worldItem );
				
				if ( string.IsNullOrEmpty( persistentItem.ItemDataPath ) )
				{
					GD.PushWarning( $"Item data path is empty for {worldItem.Name}" );
					continue;
				}

				persistentItem.PlacementType = worldItem.PlacementType;
				
				items[position][placement] = persistentItem;
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

		foreach ( var item in items )
		{
			var split = item.Key.Split( ',' );
			var position = new Vector2I( int.Parse( split[0] ), int.Parse( split[1] ) );
			foreach ( var itemEntry in item.Value )
			{
				var placement = itemEntry.Key;
				var persistentItem = itemEntry.Value;

				var worldItem = persistentItem.CreateAuto();
				
				world.AddItem( position, placement, worldItem );

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
