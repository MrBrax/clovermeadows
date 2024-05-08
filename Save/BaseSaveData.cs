using System.Text.Json;
using Godot;

namespace vcrossing.Save;

public class BaseSaveData
{
	public void SaveFile( string path )
	{
		var data = JsonSerializer.Serialize( this, new JsonSerializerOptions { WriteIndented = true, } );
		using var file = FileAccess.Open( path, FileAccess.ModeFlags.Write );
		file.StoreString( data );
		GD.Print( "Saved file to: " + path );
	}
}
