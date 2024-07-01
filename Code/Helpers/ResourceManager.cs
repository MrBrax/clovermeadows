using System;
using Godot;
using vcrossing.Code.Helpers;

namespace vcrossing.Code.Helpers;

public partial class ResourceManager : Node3D
{

	public static ResourceManager Instance { get; private set; }

	public Dictionary<string, string> ResourcePaths = [];

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
			var name = path.GetFile().GetBaseName();
			ResourcePaths[$"item:{name}"] = path;
		}
		Logger.Info( "ResourceManager", $"Loaded {ResourcePaths.Count} resources" );
	}

}
