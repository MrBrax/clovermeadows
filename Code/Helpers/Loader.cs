using vcrossing.Code.Helpers;

public static class Loader
{
	/// <summary>
	///  Load a resource from the resource path, avoiding garbage collection.
	///  Temporary hotfix for https://github.com/godotengine/godot/issues/83762#issuecomment-1876725818
	/// </summary>
	public static T LoadResource<T>( string resPath ) where T : Resource
	{
		if ( !loadedResources.TryGetValue( resPath, out var loadedRes ) )
		{
			if ( !ResourceLoader.Exists( resPath ) )
				return null;
			loadedRes = ResourceLoader.Load<Resource>( resPath );
			loadedResources[resPath] = loadedRes;
		}
		Logger.Debug( "Loader", $"Loaded resource: {resPath}" );
		return loadedRes as T;
	}
	readonly private static Dictionary<string, Resource> loadedResources = [];

	public static void ClearLoadedResources()
	{
		// foreach ( var res in loadedResources.Values )
		// 	res.Free();
		// loadedResources.Clear();
	}
}
