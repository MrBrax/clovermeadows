using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using vcrossing.Code.Components;
using vcrossing.Code.Helpers;
using vcrossing.Code.Inventory;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;

namespace vcrossing.Code.Save;

public partial class SettingsSaveData : Node
{

	public class Settings
	{
		public bool Fullscreen { get; set; }
		public bool VSync { get; set; }
		public bool SSIL { get; set; }
		public bool SSAO { get; set; }
		public bool PlayerMouseControl { get; set; }

	}

	[JsonInclude] public Settings CurrentSettings { get; set; } = new Settings();

	public void SaveSettings()
	{
		var data = JsonSerializer.Serialize( this, new JsonSerializerOptions { WriteIndented = true, } );
		using var file = FileAccess.Open( "user://settings.json", FileAccess.ModeFlags.Write );
		file.StoreString( data );
		Logger.Info( "Saved settings to: user://settings.json" );
	}

	public void LoadSettings()
	{
		if ( FileAccess.FileExists( "user://settings.json" ) )
		{
			using var file = FileAccess.Open( "user://settings.json", FileAccess.ModeFlags.Read );
			var data = file.GetAsText();
			JsonSerializer.Deserialize<SettingsSaveData>( data );
			Logger.Info( "Loaded settings from: user://settings.json" );
		}
		else
		{
			Logger.Info( "No settings file found, using default settings" );
		}
	}

	public override void _Ready()
	{
		base._Ready();
		LoadSettings();
		ApplySettings();
	}

	public void ApplySettings()
	{
		SetFullscreen( CurrentSettings.Fullscreen );
		SetVSync( CurrentSettings.VSync );
		SetSSIL( CurrentSettings.SSIL );
		SetSSAO( CurrentSettings.SSAO );
	}

	private WorldEnvironment _worldEnvironment => GetTree().GetNodesInGroup<WorldEnvironment>( "worldenvironment" ).FirstOrDefault();

	public void SetFullscreen( bool value, bool save = false )
	{
		CurrentSettings.Fullscreen = value;
		DisplayServer.WindowSetMode( value ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed );
		if ( save ) SaveSettings();
	}

	public void SetVSync( bool value, bool save = false )
	{
		CurrentSettings.VSync = value;
		DisplayServer.WindowSetVsyncMode( value ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled );
		if ( save ) SaveSettings();
	}

	public void SetSSIL( bool value, bool save = false )
	{
		CurrentSettings.SSIL = value;
		if ( _worldEnvironment != null && _worldEnvironment.Environment != null )
		{
			_worldEnvironment.Environment.SsilEnabled = value;
		}
		else
		{
			Logger.Warn( "SSIL not enabled, world environment not found" );
		}
		if ( save ) SaveSettings();
	}

	public void SetSSAO( bool value, bool save = false )
	{
		CurrentSettings.SSAO = value;
		if ( _worldEnvironment != null && _worldEnvironment.Environment != null )
		{
			_worldEnvironment.Environment.SsaoEnabled = value;
		}
		else
		{
			Logger.Warn( "SSAO not enabled, world environment not found" );
		}
		if ( save ) SaveSettings();
	}

	public void SetPlayerMouseControl( bool value, bool save = false )
	{
		CurrentSettings.PlayerMouseControl = value;
		if ( save ) SaveSettings();
	}
}
