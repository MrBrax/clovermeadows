using Godot;

namespace vcrossing;

public partial class WorldManager : Node3D
{
	
	[Export] public World ActiveWorld { get; set; }
	
	public override void _Ready()
	{
		if ( ActiveWorld == null )
		{
			LoadWorld("res://island.tscn");
		}
	}
	
	public void LoadWorld( string path )
	{
		if ( ActiveWorld != null )
		{
			ActiveWorld.QueueFree();
		}
		
		ActiveWorld = GD.Load<PackedScene>( path ).Instantiate<World>();
		AddChild( ActiveWorld );
	}
	
}
