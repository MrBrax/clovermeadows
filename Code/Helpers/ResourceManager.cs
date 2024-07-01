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

	public Dictionary<string, string> ResourcePaths = [];

	public Dictionary<string, ItemEntry> Items = [];

	public override void _Ready()
	{
		base._Ready();
		GetPaths();
		Instance = this;
	}

	public bool ResourceExists( string name )
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
	}

	public T LoadResource<T>( string name ) where T : Resource
	{
		var path = GetResourcePath( name );
		if ( path != null )
		{
			return Loader.LoadResource<T>( path );
		}

		return null;
	}

	private void GetPaths()
	{
		var paths = Resources.GetFiles( "res://items", ".*\\.tres" );
		foreach ( var path in paths )
		{
			var fileName = path.GetFile().GetBaseName();

			var data = Loader.LoadResource<ItemData>( path );

			if ( data == null )
			{
				Logger.LogError( "ResourceManager", $"Failed to load resource: {fileName}" );
				continue;
			}

			var id = data.Id;

			if ( string.IsNullOrEmpty( id ) )
			{
				data.Id = Guid.NewGuid().ToString();
				ResourceSaver.Save( data );
			}

			/* if ( ResourcePaths.ContainsKey( $"item:{fileName}" ) )
			{
				Logger.LogError( "ResourceManager", $"Duplicate resource name: {fileName}" );
				continue;
			} */

			ResourcePaths[$"item:{fileName}"] = path;

			Items[id] = new ItemEntry
			{
				Name = fileName,
				Path = path
			};

		}
		Logger.Info( "ResourceManager", $"Loaded {ResourcePaths.Count} resources" );
	}

}
