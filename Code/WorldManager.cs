using System;
using Godot;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.WorldBuilder;
using Array = Godot.Collections.Array;

namespace vcrossing2.Code;

public partial class WorldManager : Node3D
{
	[Export] public World ActiveWorld { get; set; }

	// public event Action WorldChanged;
	[Signal]
	public delegate void WorldUnloadEventHandler( World world );

	[Signal]
	public delegate void WorldLoadedEventHandler( World world );

	// public event WorldLoadedEventHandler WorldLoaded;

	public string CurrentWorldDataPath { get; set; }

	public bool IsLoading;
	
	public Array LoadingProgress { get; set; }

	public override void _Ready()
	{
		if ( ActiveWorld == null )
		{
			LoadWorld( "res://world/worlds/island.tres" );
		}

		/*WorldLoaded += ( world ) =>
		{
			SetupNewWorld();
		};*/
	}

	private void SetLoadingScreen( bool visible, string text = "" )
	{
		Logger.Info( "WorldManager", $"Setting loading screen: {visible}, {text}" );
		GetNode<PanelContainer>( "/root/Main/UserInterface/LoadingScreen" ).Visible = visible;
		GetNode<Label>( "/root/Main/UserInterface/LoadingScreen/MarginContainer/LoadingLabel" ).Text = text;
	}

	public async void LoadWorld( string worldDataPath )
	{
		if ( IsLoading )
		{
			Logger.LogError( "WorldManager", "Already loading a world." );
			return;
		}

		SetLoadingScreen( true, $"Loading {worldDataPath}..." );
		
		await ToSignal( GetTree(), SceneTree.SignalName.ProcessFrame );

		if ( ActiveWorld != null )
		{
			EmitSignal( SignalName.WorldUnload, ActiveWorld );
			ActiveWorld.Unload();
			ActiveWorld.QueueFree();
			ActiveWorld = null;
		}
		
		// WorldChanged?.Invoke();

		Logger.Info( "WorldManager", "Waiting for old world to be freed." );
		await ToSignal( GetTree(), SceneTree.SignalName.ProcessFrame );
		
		Logger.Info( "WorldManager", "Waited for old world to be freed, hopefully it's gone now." );

		CurrentWorldDataPath = worldDataPath;

		if ( ResourceLoader.HasCached( CurrentWorldDataPath ) )
		{
			Logger.Info( "WorldManager", "Loading world data from cache." );
			var resource = ResourceLoader.Load( CurrentWorldDataPath );
			if ( resource is WorldData worldData )
			{
				SetupNewWorld( worldData );
			}
			else
			{
				Logger.LogError( "WorldManager", $"Failed to load world data: {CurrentWorldDataPath}" );
				IsLoading = false;
				SetLoadingScreen( false );
			}

			return;
		}

		Logger.Info( "WorldManager", "Loading world data threaded..." );
		var error = ResourceLoader.LoadThreadedRequest( CurrentWorldDataPath );
		if ( error != Error.Ok )
		{
			Logger.LogError( "WorldManager", $"Failed to load world data: {CurrentWorldDataPath} ({error})" );
			IsLoading = false;
			SetLoadingScreen( false );
		} else {
			Logger.Info( "WorldManager", $"World data loading response: {error}" );
			IsLoading = true;
		}
	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		if ( !IsLoading )
		{
			return;
		}

		if ( !string.IsNullOrEmpty( CurrentWorldDataPath ) && ActiveWorld == null )
		{
			var status = ResourceLoader.LoadThreadedGetStatus( CurrentWorldDataPath, LoadingProgress );
			if ( status == ResourceLoader.ThreadLoadStatus.Loaded )
			{
				var resource = ResourceLoader.LoadThreadedGet( CurrentWorldDataPath );
				if ( resource is WorldData worldData )
				{
					SetupNewWorld( worldData );
				}
			}
			else if ( status == ResourceLoader.ThreadLoadStatus.Failed )
			{
				Logger.LogError( "WorldManager", $"Failed to load world data: {CurrentWorldDataPath}" );
				IsLoading = false;
				SetLoadingScreen( false );
			}
			else if ( status == ResourceLoader.ThreadLoadStatus.InvalidResource )
			{
				Logger.LogError( "WorldManager", $"Invalid resource: {CurrentWorldDataPath}" );
				IsLoading = false;
				SetLoadingScreen( false );
			}
			else
			{
				// Logger.Info( "World data not loaded yet." );
			}
		}
	}

	

	private void SetupNewWorld( WorldData worldData )
	{
		/*if ( worldData == null )
		{
			throw new System.Exception( "World data is null." );
			return;
		}

		if ( worldData.WorldScene == null )
		{
			throw new System.Exception( "World scene is null." );
			return;
		}*/

		Logger.Info( "WorldManager", "Loading new world." );

		// TODO: loading screen
		ActiveWorld = worldData.WorldScene.Instantiate<World>();
		ActiveWorld.WorldId = worldData.WorldId;
		ActiveWorld.WorldName = worldData.WorldName;
		ActiveWorld.WorldPath = worldData.ResourcePath;
		ActiveWorld.GridWidth = worldData.Width;
		ActiveWorld.GridHeight = worldData.Height;
		ActiveWorld.UseAcres = worldData.UseAcres;

		Logger.Info( "WorldManager", "Adding new world to scene." );
		AddChild( ActiveWorld );

		// Logger.Info( "WorldManager", "Checking terrain." );
		// ActiveWorld.CheckTerrain();

		Logger.Info( "WorldManager", "Loading editor placed items." );
		ActiveWorld.LoadEditorPlacedItems();

		Logger.Info( "WorldManager", "Loading world data." );
		ActiveWorld.Load();

		Logger.Info( "WorldManager", "World loaded." );
		IsLoading = false;
		GetNode<PanelContainer>( "/root/Main/UserInterface/LoadingScreen" ).Hide();
		// WorldLoaded?.Invoke( ActiveWorld );
		// WorldLoaded
		EmitSignal( SignalName.WorldLoaded, ActiveWorld );
	}
}
