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
		
		// ActiveWorld = GD.Load<PackedScene>( path ).Instantiate<World>();
		ActiveWorld = worldData.WorldScene.Instantiate<World>();
		ActiveWorld.WorldName = worldData.WorldId;
		ActiveWorld.GridWidth = worldData.Width;
		ActiveWorld.GridHeight = worldData.Height;
		AddChild( ActiveWorld );
		ActiveWorld.LoadEditorPlacedItems();
		ActiveWorld.Load();
	}
	
}
