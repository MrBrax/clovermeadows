using System;
using Godot;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.WorldBuilder;

namespace vcrossing2.Code;

public partial class WorldManager : Node3D
{
	[Export] public World ActiveWorld { get; set; }

	public event Action WorldChanged;
	
	public delegate void WorldLoadedDelegate( World world );
	public event WorldLoadedDelegate WorldLoaded;
	
	public string CurrentWorldDataPath { get; set; }

	public bool IsLoading;

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

	public async void LoadWorld( string worldDataPath )
	{
		if ( IsLoading )
		{
			Logger.LogError( "Already loading a world." );
			return;
		}
		
		if ( ActiveWorld != null )
		{
			ActiveWorld.Unload();
			ActiveWorld.QueueFree();
			ActiveWorld = null;
		}
		
		IsLoading = true;
		WorldChanged?.Invoke();
		
		Logger.Info( "Waiting for old world to be freed." );
		await ToSignal( GetTree(), SceneTree.SignalName.ProcessFrame );
		
		CurrentWorldDataPath = worldDataPath;

		var error = ResourceLoader.LoadThreadedRequest( CurrentWorldDataPath );

	}

	public override void _Process( double delta )
	{
		base._Process( delta );
		
		if ( !string.IsNullOrEmpty( CurrentWorldDataPath ) && ActiveWorld == null )
		{
			
			var status = ResourceLoader.LoadThreadedGetStatus( CurrentWorldDataPath );
			if ( status == ResourceLoader.ThreadLoadStatus.Loaded )
			{
				var resource = ResourceLoader.LoadThreadedGet( CurrentWorldDataPath );
				if ( resource is WorldData worldData )
				{
					SetupNewWorld( worldData );
				}
			}
			else
			{
				// Logger.Info( "World data not loaded yet." );
			}
			
		}
	}

	private void SetupNewWorld( WorldData worldData ) {
		
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

		Logger.Info( "Loading new world." );

		// TODO: loading screen
		ActiveWorld = worldData.WorldScene.Instantiate<World>();
		ActiveWorld.WorldName = worldData.WorldId;
		ActiveWorld.GridWidth = worldData.Width;
		ActiveWorld.GridHeight = worldData.Height;
		ActiveWorld.UseAcres = worldData.UseAcres;

		Logger.Info( "LoadWorld", "Adding new world to scene." );
		AddChild( ActiveWorld );

		// Logger.Info( "LoadWorld", "Checking terrain." );
		// ActiveWorld.CheckTerrain();

		Logger.Info( "LoadWorld", "Loading editor placed items." );
		ActiveWorld.LoadEditorPlacedItems();

		Logger.Info( "LoadWorld", "Loading world data." );
		ActiveWorld.Load();

		Logger.Info( "LoadWorld", "World loaded." );
		IsLoading = false;
		WorldLoaded?.Invoke( ActiveWorld );
	}
}
