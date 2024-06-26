using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using vcrossing.Code.Components;
using vcrossing.Code.Helpers;
using vcrossing.Code.Inventory;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using vcrossing.Code.Ui;

namespace vcrossing.Code.Save;

public partial class SettingsSaveData : Node
{

	public class GameSettings
	{
		public bool Fullscreen { get; set; }
		public bool VSync { get; set; }
		public bool SSIL { get; set; }
		public bool SSAO { get; set; }
		public bool PlayerMouseControl { get; set; }
		public bool ShowTouchControls { get; set; } = false;

		public float VolumeMaster { get; set; }
		public float VolumeEffects { get; set; }
		public float VolumeMusic { get; set; }
		public float VolumeAmbience { get; set; }
		public float VolumeEating { get; set; }
		public float VolumeUI { get; set; }

		public float RenderScale { get; set; } = 1f;
		public Viewport.Scaling3DModeEnum Scaling3DMode { get; set; } = Viewport.Scaling3DModeEnum.Bilinear;

	}

	[JsonInclude] public GameSettings CurrentSettings { get; set; } = new GameSettings();

	public void SaveSettings()
	{
		var data = JsonSerializer.Serialize( CurrentSettings, new JsonSerializerOptions { WriteIndented = true, } );
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
			JsonSerializer.Deserialize<GameSettings>( data );
			Logger.Info( "Loaded settings from: user://settings.json" );
		}
		else
		{
			Logger.Info( "No settings file found, using default settings" );

			SetVolume( "master", AudioServer.GetBusVolumeDb( AudioServer.GetBusIndex( "Master" ) ) );
			SetVolume( "effects", AudioServer.GetBusVolumeDb( AudioServer.GetBusIndex( "Effects" ) ) );
			SetVolume( "music", AudioServer.GetBusVolumeDb( AudioServer.GetBusIndex( "Music" ) ) );
			SetVolume( "ambience", AudioServer.GetBusVolumeDb( AudioServer.GetBusIndex( "Ambience" ) ) );
			SetVolume( "eating", AudioServer.GetBusVolumeDb( AudioServer.GetBusIndex( "Eating" ) ) );
			SetVolume( "ui", AudioServer.GetBusVolumeDb( AudioServer.GetBusIndex( "UserInterface" ) ) );
		}
	}

	public override void _Ready()
	{
		base._Ready();
		CurrentSettings = new GameSettings();
		LoadSettings();
		ApplySettings();
	}

	public void ApplySettings()
	{
		SetFullscreen( CurrentSettings.Fullscreen );
		SetVSync( CurrentSettings.VSync );
		SetSSIL( CurrentSettings.SSIL );
		SetSSAO( CurrentSettings.SSAO );
		SetRenderScale( CurrentSettings.RenderScale );
		SetRenderMode( CurrentSettings.Scaling3DMode );

		SetVolume( "master", CurrentSettings.VolumeMaster );
		SetVolume( "effects", CurrentSettings.VolumeEffects );
		SetVolume( "music", CurrentSettings.VolumeMusic );
		SetVolume( "ambient", CurrentSettings.VolumeAmbience );
		SetVolume( "eating", CurrentSettings.VolumeEating );
		SetVolume( "ui", CurrentSettings.VolumeUI );
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

	public void SetRenderScale( float value, bool save = false )
	{
		Logger.Info( $"Setting render scale to {value}" );
		CurrentSettings.RenderScale = value;
		// ProjectSettings.SetSetting( "rendering/scaling_3d/scale", value );
		GetTree().Root.Scaling3DScale = value;
		// GetTree().Root.Scaling3DMode = Viewport.Scaling3DModeEnum
		if ( save ) SaveSettings();
	}

	public void SetRenderMode( Viewport.Scaling3DModeEnum value, bool save = false )
	{
		Logger.Info( $"Setting render scale to {value}" );
		// ProjectSettings.SetSetting( "rendering/scaling_3d/scale", value );
		GetTree().Root.Scaling3DMode = value;
		if ( save ) SaveSettings();
	}

	public void SetPlayerMouseControl( bool value, bool save = false )
	{
		CurrentSettings.PlayerMouseControl = value;
		if ( save ) SaveSettings();
	}

	public void SetVolume( string bus, float value, bool save = false )
	{
		Logger.Info( $"Setting volume for {bus} to {value}" );
		switch ( bus )
		{
			case "master":
				CurrentSettings.VolumeMaster = value;
				AudioServer.SetBusVolumeDb( AudioServer.GetBusIndex( "Master" ), value );
				break;
			case "effects":
				CurrentSettings.VolumeEffects = value;
				AudioServer.SetBusVolumeDb( AudioServer.GetBusIndex( "Effects" ), value );
				break;
			case "music":
				CurrentSettings.VolumeMusic = value;
				AudioServer.SetBusVolumeDb( AudioServer.GetBusIndex( "Music" ), value );
				break;
			case "ambient":
				CurrentSettings.VolumeAmbience = value;
				AudioServer.SetBusVolumeDb( AudioServer.GetBusIndex( "Ambience" ), value );
				AudioServer.SetBusVolumeDb( AudioServer.GetBusIndex( "AmbienceOutside" ), value );
				break;
			case "eating":
				CurrentSettings.VolumeEating = value;
				AudioServer.SetBusVolumeDb( AudioServer.GetBusIndex( "Eating" ), value );
				break;
			case "ui":
				CurrentSettings.VolumeUI = value;
				AudioServer.SetBusVolumeDb( AudioServer.GetBusIndex( "UserInterface" ), value );
				break;
			default:
				Logger.Warn( $"Unknown volume bus: {bus}" );
				return;
		}

		if ( save ) SaveSettings();
	}

	public void SetShowTouchControls( bool value, bool save = false )
	{
		CurrentSettings.ShowTouchControls = value;
		var t = GetNodeOrNull<TouchInterface>( "/root/Main/UserInterface/Touchinterface" );
		if ( t != null ) t.Visible = value;
		if ( save ) SaveSettings();
	}
}
