using System.Text.Json;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Save;

public class NpcSaveData
{
	public string NpcId;

	public Dictionary<string, int> PlayerReputation = new();
	public Dictionary<string, int> NpcReputation = new();

	public Dictionary<string, string> PlayerNicknames = new();
	public Dictionary<string, string> NpcNicknames = new();

	public class Outfit
	{
		public string HatPath;
		public string ShirtPath;
		public string PantsPath;
		public string ShoesPath;
	}

	protected string SaveDataPath => $"user://npcs/{NpcId}.json";
	
	public NpcSaveData()
	{
		
	}
	
	public NpcSaveData( string npcId )
	{
		NpcId = npcId;
	}

	public static NpcSaveData Load( string npcId )
	{
		
		DirAccess.MakeDirAbsolute( "user://npcs" );
		
		var saveDataPath = $"user://npcs/{npcId}.json";
		if ( !FileAccess.FileExists( saveDataPath ) )
		{
			Logger.Info( $"No save data found for {npcId}" );
			return new NpcSaveData { NpcId = npcId };
		}

		var text = FileAccess.Open( saveDataPath, FileAccess.ModeFlags.Read ).GetAsText();

		Logger.Info( $"Loaded save data for {npcId}" );
		var data = JsonSerializer.Deserialize<NpcSaveData>( text, new JsonSerializerOptions { IncludeFields = true } );
		data.NpcId = npcId;
		return data;
	}
	
	public void Save()
	{
		var text = JsonSerializer.Serialize( this, new JsonSerializerOptions { IncludeFields = true, WriteIndented = true} );
		// FileAccess.Open( SaveDataPath, FileAccess.ModeFlags.Write ).StoreString( text );
		using var file = FileAccess.Open( SaveDataPath, FileAccess.ModeFlags.Write );
		file.StoreString( text );
		file.Close();
		Logger.Info( $"Saved data for {NpcId}" );
	}

	public void AddPlayerReputation( string playerId, int amount )
	{
		if ( !PlayerReputation.ContainsKey( playerId ) )
		{
			PlayerReputation.Add( playerId, 0 );
		}

		PlayerReputation[playerId] += amount;
		
		Save();
	}
}
