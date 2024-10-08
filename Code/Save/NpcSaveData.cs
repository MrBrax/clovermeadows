﻿using System.Text.Json;
using vcrossing.Code.Helpers;
using vcrossing.Code.Items;
using vcrossing.Code.Player;

namespace vcrossing.Code.Save;

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
			Logger.Info( "NpcSaveData", $"No save data found for {npcId}" );
			return new NpcSaveData { NpcId = npcId };
		}

		var text = FileAccess.Open( saveDataPath, FileAccess.ModeFlags.Read ).GetAsText();

		Logger.Info( $"Loaded save data for {npcId}" );
		var data = JsonSerializer.Deserialize<NpcSaveData>( text, MainGame.JsonOptions );
		data.NpcId = npcId;
		return data;
	}

	public void Save()
	{
		var text = JsonSerializer.Serialize( this, MainGame.JsonOptions );
		// FileAccess.Open( SaveDataPath, FileAccess.ModeFlags.Write ).StoreString( text );
		using var file = FileAccess.Open( SaveDataPath, FileAccess.ModeFlags.Write );
		file.StoreString( text );
		file.Close();
		Logger.Info( $"Saved data for {NpcId}" );
	}

	public void AddPlayerReputation( string playerId, int amount )
	{
		if ( string.IsNullOrWhiteSpace( playerId ) ) throw new System.ArgumentNullException( nameof( playerId ) );
		if ( PlayerReputation == null ) PlayerReputation = new();
		PlayerReputation.TryAdd( playerId, 0 );

		PlayerReputation[playerId] += amount;

		Save();
	}
}
