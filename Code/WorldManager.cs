using Godot;
using vcrossing2.Code.WorldBuilder;

namespace vcrossing2.Code;

public partial class WorldManager : Node3D
{
	
	[Export] public World ActiveWorld { get; set; }
	
	public override void _Ready()
	{
		if ( ActiveWorld == null )
		{
			LoadWorld( GD.Load<WorldData>( "res://world/worlds/island.tres" ) );
		}
	}
	
	public void LoadWorld( WorldData worldData )
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
		
		// TODO: loading screen
		ActiveWorld = worldData.WorldScene.Instantiate<World>();
		ActiveWorld.WorldName = worldData.WorldId;
		ActiveWorld.GridWidth = worldData.Width;
		ActiveWorld.GridHeight = worldData.Height;
		ActiveWorld.UseAcres = worldData.UseAcres;
		AddChild( ActiveWorld );
		ActiveWorld.LoadEditorPlacedItems();
		ActiveWorld.Load();
	}
	
}
