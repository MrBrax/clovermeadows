using System;
using Godot;
using vcrossing.Code.Data;
using vcrossing.Code.Helpers;

namespace vcrossing.Code.Helpers;

public partial class ResourceManager : Node3D
{

	public struct ItemEntry
	{
		public string Name;
		public string Path;
	}

	public static ResourceManager Instance { get; private set; }

	// public Dictionary<string, string> ResourcePaths = [];

	public Dictionary<string, ItemEntry> Items = [];

	public override void _Ready()
	{
		base._Ready();
		GetPaths();
		Instance = this;
	}

	/* public bool ResourceExists( string name )
	{
		return ResourcePaths.ContainsKey( name );
	}

	public string GetResourcePath( string name )
	{
		if ( ResourcePaths.ContainsKey( name ) )
		{
			return ResourcePaths[name];
		}

		return null;
	}

	public string GetResourceName( string path )
	{
		return ResourcePaths.FirstOrDefault( x => x.Value == path ).Key;
	} */

	public string GetItemPathByName( string name )
	{
		return Items.FirstOrDefault( x => x.Value.Name == name ).Value.Path;
	}

	/* public string GetItemPath( string id )
	{
		if ( Items.TryGetValue( id, out ItemEntry value ) )
		{
			return value.Path;
		}

		return null;
	} */

	/* public T LoadItemFromId<T>( string id ) where T : ItemData
	{
		var path = GetItemPath( id );
		if ( path != null )
		{
			return Loader.LoadResource<T>( path );
		}

		return null;
	} */

	public static T LoadItemFromId<T>( string id ) where T : ItemData
	{
		var path = GetItemPath( id );
		if ( path != null )
		{
			return Loader.LoadResource<T>( path );
		}

		Logger.Warn( "ResourceManager", $"Failed to load item from id: {id}" );

		return null;
	}

	private void GetPaths()
	{
		var paths = Resources.GetFiles( "res://items", ".*\\.tres" );
		foreach ( var path in paths )
		{
			var fileName = path.GetFile().GetBaseName();

			var resource = Loader.LoadResource<Resource>( path );

			if ( resource == null )
			{
				Logger.LogError( "ResourceManager", $"Failed to load resource: {fileName}" );
				continue;
			}

			if ( resource is not ItemData data )
			{
				Logger.LogError( "ResourceManager", $"Resource is not an item: {fileName}" );
				continue;
			}

			if ( data == null )
			{
				Logger.LogError( "ResourceManager", $"Failed to load item data: {fileName}" );
				continue;
			}

			var id = data.Id;

			/* if ( string.IsNullOrEmpty( id ) || !data.PropertyCanRevert( "Id" ) )
			{
				Logger.Info( "ResourceManager", $"Generating new id for {fileName}" );
				data.Id = Guid.NewGuid().ToString();
				ResourceSaver.Save( data );
			} */

			/* if ( ResourcePaths.ContainsKey( $"item:{fileName}" ) )
			{
				Logger.LogError( "ResourceManager", $"Duplicate resource name: {fileName}" );
				continue;
			} */

			// ResourcePaths[$"item:{fileName}"] = path;

			Items[id] = new ItemEntry
			{
				Name = fileName,
				Path = path
			};

			Logger.Verbose( "ResourceManager", $"Loaded item {fileName} with id {id}" );

		}
		Logger.Info( "ResourceManager", $"Loaded {Items.Count} resources" );
	}

	public static string GetItemPath( string itemDataId )
	{
		if ( Instance.Items.TryGetValue( itemDataId, out ItemEntry value ) )
		{
			return value.Path;
		}

		return null;
	}
}
