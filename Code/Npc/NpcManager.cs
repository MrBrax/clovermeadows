using Godot.Collections;
using vcrossing2.Code.Helpers;

namespace vcrossing2.Code.Npc;

public partial class NpcManager : Node3D
{
	public class NpcWorldData
	{
		public NpcData Data;
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
		Logger.Info( "NpcManager", $"Setting up npcs." );
		
		var vpup = GD.Load<NpcData>( "res://npc/vdog.tres" );
		NpcInstanceData.Add( "vpup",
			new NpcWorldData
			{
				Data = vpup, Position = new Vector3( 3, 0, 45 ), World = "island"
			} );
	}

	public void OnWorldUnloaded( World world )
	{
		Logger.Info( "NpcManager", $"Unloading npcs from {world.WorldId}." );
		// remove all npcs from the world
		foreach ( var node in GetChildren() )
		{
			if ( node is not BaseNpc npc )
			{
				Logger.Warn( "NpcManager", $"Node {node.Name} is not a BaseNpc." );
				continue;
			}

			var data = npc.GetData();
			if ( data == null ) throw new System.Exception( "Npc data not found." );

			NpcInstanceData[data.NpcId].Position = npc.GlobalPosition;
			NpcInstanceData[data.NpcId].World = world.WorldId;

			npc.QueueFree();
		}
	}

	public void OnWorldLoaded( World world )
	{
		// panic remove all npcs
		foreach ( var node in GetChildren() )
		{
			if ( node is not BaseNpc npc )
			{
				Logger.Warn( "NpcManager", $"Node {node.Name} is not a BaseNpc." );
				continue;
			}

			Logger.Warn( "NpcManager", $"Panic removing npc {npc.Name}." );
			npc.QueueFree();
		}
		
		Logger.Info( "NpcManager", $"Loading npcs for {world.WorldId}." );
		foreach ( var npcData in NpcInstanceData )
		{
			var data = npcData.Value.Data;
			var position = npcData.Value.Position;
			var worldId = npcData.Value.World;

			if ( worldId != world.WorldId )
			{
				Logger.Info( "NpcManager", $"Skipping npc {data.NpcId} in {worldId} since it's not in {world.WorldId}." );
				continue;
			}
			
			if ( data.NpcScene == null ) throw new System.Exception( "Npc scene is null." );

			var npc = data.NpcScene.Instantiate<BaseNpc>();
			AddChild( npc );
			npc.GlobalPosition = position;
			Logger.Info( "NpcManager", $"Added npc {data.NpcId} to {world.WorldId} ({npc.GlobalPosition})." );
		}
		if ( NpcInstanceData.Count == 0 ) Logger.Warn( "NpcManager", "No npcs to load." );
	}
}
