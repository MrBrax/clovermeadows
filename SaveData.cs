using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using vcrossing.DTO;

namespace vcrossing;

public class SaveData
{
	
	[JsonInclude] public Dictionary<string, Dictionary<World.ItemPlacement, BaseDTO>> WorldItems = new();
	
	public void AddWorldItems( World world )
	{
		WorldItems.Clear();

		// var items = world.Items.Duplicate( true );
		foreach ( var item in world.Items )
		{
			var position = item.Key;
			foreach ( var itemEntry in item.Value )
			{
				var placement = itemEntry.Key;
				var worldItem = itemEntry.Value;
				
				if ( !WorldItems.ContainsKey( position ) )
				{
					WorldItems[position] = new();
				}
				worldItem.UpdateDTO();
				WorldItems[position][placement] = worldItem.DTO;
			}
		}
		
		GD.Print( $"Added {WorldItems.Count} world items" );

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
		var saveData = JsonSerializer.Deserialize<SaveData>( json, new JsonSerializerOptions
		{
			IncludeFields = true,
		} );

		WorldItems = saveData.WorldItems;
	}

	public void LoadWorldItems( World world )
	{
		foreach ( var item in WorldItems )
		{
			var split = item.Key.Split( ',' );
			var position = new Vector2I( int.Parse( split[0] ), int.Parse( split[1] ) );
			foreach ( var itemEntry in item.Value )
			{
				var placement = itemEntry.Key;
				var dto = itemEntry.Value;
				
				var worldItem = world.SpawnDto( dto, position, placement );
				// worldItem.UpdatePositionAndRotation();
			}
		}
	}

	public void SaveFile( string path )
	{
		var data = JsonSerializer.Serialize( this, new JsonSerializerOptions
		{
			WriteIndented = true,
		} );
		using var file = FileAccess.Open( path, FileAccess.ModeFlags.Write );
		file.StoreString( data );
	}
}
