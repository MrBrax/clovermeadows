using Godot;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.WorldBuilder;

namespace vcrossing2.Code;

public partial class WorldManager : Node3D
{
	[Export] public World ActiveWorld { get; set; }

	public delegate void WorldLoadedDelegate( World world );

	public event WorldLoadedDelegate WorldLoaded;

	public override void _Ready()
	{
		if ( ActiveWorld == null )
		{
			LoadWorld( GD.Load<WorldData>( "res://world/worlds/island.tres" ) );
		}
	}

	public async void LoadWorld( WorldData worldData )
	{
		if ( ActiveWorld != null )
		{
			ActiveWorld.Unload();
			ActiveWorld.QueueFree();
		}

		if ( worldData == null )
		{
			throw new System.Exception( "World data is null." );
			return;
		}

		if ( worldData.WorldScene == null )
		{
			throw new System.Exception( "World scene is null." );
			return;
		}

		Logger.Info( "Waiting for old world to be freed." );
		await ToSignal( GetTree(), SceneTree.SignalName.ProcessFrame );

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
		WorldLoaded?.Invoke( ActiveWorld );
	}
}
