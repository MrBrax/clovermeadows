using Godot.Collections;

namespace vcrossing2.Code.Npc;

public partial class NpcManager : Node3D
{
	
	public struct NpcWorldData
	{
		public string Id;
		public Vector3 Position;
		public string World;
	}

	// public Godot.Collections.Dictionary<string, Vector3> NpcPositions = new();
	public System.Collections.Generic.Dictionary<string, NpcWorldData> NpcInstanceData = new();

	public override void _Ready()
	{
		base._Ready();
		SetupNpcs();
	}

	private void SetupNpcs()
	{
		var vpup = GD.Load<NpcData>( "res://npc/vdog.tres" );
		
		
	}

	public void OnWorldUnloaded( World world )
	{
		
	}

	public void OnWorldLoaded( World world )
	{
		
	}
	
}
