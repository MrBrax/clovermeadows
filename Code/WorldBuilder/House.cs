using Godot;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Items;

namespace vcrossing.Code.WorldBuilder;

public partial class House : Node3D
{

	// [Export] public string HouseId { get; set; }

	[Export( PropertyHint.File, "*.tres" )]
	public string DestinationWorld { get; set; }
	[Export] public string DestinationExit { get; set; } = "entrance";
	[Export, Require] public AreaTrigger EntranceTrigger { get; set; }

	public override void _Ready()
	{
		// SpawnTrigger();
		// CallDeferred( nameof( SpawnTrigger ) );

		var worldManager = NodeManager.WorldManager;
		if ( worldManager == null ) throw new System.Exception( "WorldManager not found." );

		worldManager.WorldLoaded += OnWorldLoaded;
	}

	private void OnWorldLoaded( World world )
	{
		SpawnTrigger();
	}

	private void SpawnTrigger()
	{
		EntranceTrigger.DestinationWorld = DestinationWorld;
		EntranceTrigger.DestinationExit = DestinationExit;
	}
}
