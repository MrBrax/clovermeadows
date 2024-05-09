using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code.DTO;

namespace vcrossing2.Code.Save;

public class WorldSaveData : BaseSaveData
{
	// [JsonInclude] public Dictionary<string, Dictionary<World.ItemPlacement, BaseDTO>> WorldItems = new();

	public struct WorldInstance
	{
		[JsonInclude] public string Name;
		[JsonInclude] public Dictionary<string, Dictionary<World.ItemPlacement, BaseDTO>> Items;

		// Misc items
		// [JsonInclude] public ???

		public WorldInstance( string name )
		{
			Name = name;
			Items = new();
		}
	}

	[JsonInclude] public Dictionary<string, WorldInstance> Instances = new();

	public WorldInstance GetInstance( string name )
	{
		if ( !Instances.ContainsKey( name ) )
		{
			Instances[name] = new WorldInstance( name );
		}

		return Instances[name];
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

				if ( !items.ContainsKey( position ) )
				{
					items[position] = new();
				}

				worldItem.UpdateDTO();
				items[position][placement] = worldItem.DTO;
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

	public void LoadFile( string filePath )
	{
		if ( !FileAccess.FileExists( filePath ) )
		{
			throw new System.Exception( $"File {filePath} does not exist" );
			return;
		}

		using var file = FileAccess.Open( filePath, FileAccess.ModeFlags.Read );
		var json = file.GetAsText();
		var saveData =
			JsonSerializer.Deserialize<WorldSaveData>( json, new JsonSerializerOptions { IncludeFields = true, } );

		// WorldItems = saveData.WorldItems;
		Instances = saveData.Instances;
	}

	public void LoadWorldItems( World world )
	{
		foreach ( var item in GetInstance( world.WorldName ).Items )
		{
			var split = item.Key.Split( ',' );
			var position = new Vector2I( int.Parse( split[0] ), int.Parse( split[1] ) );
			foreach ( var itemEntry in item.Value )
			{
				var placement = itemEntry.Key;
				var dto = itemEntry.Value;

				try
				{
					var worldItem = world.SpawnDto( dto, position, placement );
				}
				catch ( Exception e )
				{
					GD.Print( e.Message );
				}
				// worldItem.UpdatePositionAndRotation();
			}
		}
	}
}
