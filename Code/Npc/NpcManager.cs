using Godot.Collections;
using vcrossing2.Code.Helpers;

namespace vcrossing2.Code.Npc;

public partial class NpcManager : Node3D
{

	[Export] public Array<NpcData> Npcs { get; set; }

	public class NpcWorldData
	{
		public NpcData Data;
		public Vector3 Position;
		// public string World;
		public string WorldPath;
		public Node3D FollowTarget;
		public bool IsFollowing;
		public string FollowTargetExit;
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

		/*var vpup = GD.Load<NpcData>( "res://npc/vdog.tres" );
		NpcInstanceData.Add( "vpup",
			new NpcWorldData
			{
				Data = vpup, Position = new Vector3( 3, 0, 45 ), World = "island"
			} );*/

		foreach ( var npc in Npcs )
		{
			GenerateNpc( npc );
		}
	}

	private void GenerateNpc( NpcData npc )
	{

		var world = GetRandomNpcSpawnWorld();
		var position = GetRandomNpcSpawnPosition();

		if ( npc.NpcScene == null ) throw new System.Exception( "Npc scene is null." );

		var npcInstance = new NpcWorldData
		{
			Data = npc,
			Position = position,
			WorldPath = world,
		};

		NpcInstanceData.Add( npc.NpcId, npcInstance );
		Logger.Info( "NpcManager", $"Added npc {npc.NpcId}." );

	}

	private Vector3 GetRandomNpcSpawnPosition()
	{
		return new Vector3( 3, 0, 45 );
	}

	private string GetRandomNpcSpawnWorld()
	{
		return "res://world/worlds/island.tres";
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
			NpcInstanceData[data.NpcId].WorldPath = world.WorldPath;

			npc.OnWorldUnloaded( world );

			if ( NpcInstanceData[data.NpcId].FollowTarget == null )
			{
				Logger.Info( "NpcManager", $"Unloading npc {data.NpcId}." );
				npc.QueueFree();
			}
			else
			{
				Logger.Info( "NpcManager", $"Keeping npc {data.NpcId} due to follow target." );
			}
		}
	}

	public bool HasNpc( string npcId )
	{
		return GetChildren().Any( node => node is BaseNpc npc && npc.GetData().NpcId == npcId );
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

			// Logger.Warn( "NpcManager", $"Panic removing npc {npc.Name}." );
			// npc.QueueFree();
			npc.OnWorldLoaded( world );
		}

		Logger.Info( "NpcManager", $"Loading npcs for {world.WorldId}." );
		foreach ( var npcData in NpcInstanceData )
		{
			var data = npcData.Value.Data;
			var position = npcData.Value.Position;
			var worldPath = npcData.Value.WorldPath;

			if ( HasNpc( data.NpcId ) )
			{
				Logger.Info( "NpcManager", $"Npc {data.NpcId} already exists." );
				continue;
			}

			if ( worldPath != world.WorldPath )
			{
				Logger.Info( "NpcManager", $"Skipping npc {data.NpcId} in {worldPath} since it's not in {world.WorldPath}." );
				continue;

			}

			if ( data.NpcScene == null ) throw new System.Exception( "Npc scene is null." );

			var npc = data.NpcScene.Instantiate<BaseNpc>();
			AddChild( npc );
			npc.GlobalPosition = position;
			npc.OnWorldLoaded( world );
			Logger.Info( "NpcManager", $"Added npc {data.NpcId} to {world.WorldId} ({npc.GlobalPosition})." );
		}
		if ( NpcInstanceData.Count == 0 ) Logger.Warn( "NpcManager", "No npcs to load." );
	}
}
