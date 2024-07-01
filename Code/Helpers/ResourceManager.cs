namespace vcrossing.Code.Helpers;

public partial class ResourceManager : Node3D
{

	public Dictionary<string, string> ItemPaths = new Dictionary<string, string>();

	public override void _Ready()
	{
		base._Ready();
		GetPaths();
	}

	public bool ItemExists( string name )
	{
		return ItemPaths.ContainsKey( name );
	}

	public string GetItemPath( string name )
	{
		if ( ItemPaths.ContainsKey( name ) )
		{
			return ItemPaths[name];
		}

		return null;
	}

	public T LoadResource<T>( string name ) where T : Resource
	{
		var path = GetItemPath( name );
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
			ItemPaths[name] = path;
		}
		Logger.Info( "ResourceManager", $"Loaded {ItemPaths.Count} items" );
	}

}
