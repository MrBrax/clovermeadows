using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using vcrossing.Code.Helpers;

namespace vcrossing.Code.Save;

[JsonDerivedType( typeof( WorldSaveData ) )]
[JsonDerivedType( typeof( PlayerSaveData ) )]
public partial class BaseSaveData
{
	public void SaveFile( string path )
	{
		var data = JsonSerializer.Serialize( this, MainGame.JsonOptions );
		using var file = FileAccess.Open( path, FileAccess.ModeFlags.Write );
		file.StoreString( data );
		Logger.Info( "Saved file to: " + path );
	}
}
