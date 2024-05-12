using Godot;
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
		
		GD.Print( "Waiting for old world to be freed." );
		await ToSignal( GetTree(), SceneTree.SignalName.ProcessFrame );
		
		GD.Print( "Loading new world." );
		
		// TODO: loading screen
		ActiveWorld = worldData.WorldScene.Instantiate<World>();
		ActiveWorld.WorldName = worldData.WorldId;
		ActiveWorld.GridWidth = worldData.Width;
		ActiveWorld.GridHeight = worldData.Height;
		ActiveWorld.UseAcres = worldData.UseAcres;
		
		GD.Print( "Adding new world to scene." );
		AddChild( ActiveWorld );
		
		GD.Print( "Loading editor placed items." );
		ActiveWorld.LoadEditorPlacedItems();
		
		GD.Print( "Loading world data." );
		ActiveWorld.Load();
		
		GD.Print( "World loaded." );
		WorldLoaded?.Invoke( ActiveWorld );
	}
	
}
