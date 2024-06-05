using Godot;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Items;

namespace vcrossing.Code.WorldBuilder;

public partial class House : Node3D
{
	
	// [Export] public string HouseId { get; set; }
	
	[Export] public string DestinationWorld { get; set; }
	[Export] public string DestinationExit { get; set; } = "entrance";
	[Export, Require] public AreaTrigger EntranceTrigger { get; set; }
	
	public override void _Ready()
	{
		// SpawnTrigger();
		// CallDeferred( nameof( SpawnTrigger ) );
		
		var worldManager = GetNode<WorldManager>( "/root/Main/WorldContainer" );
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
		
		/*GD.Print( "Spawning house entrance trigger." );
		var world = GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;

		if ( world == null ) throw new System.Exception( "World not found." );*/

		/*var entrancePosition = world.WorldToItemGrid( GlobalPosition + new Vector3( 0, 0, 1 ) );

		var trigger = world.SpawnPlacedItem<AreaTrigger>( GD.Load<ItemData>( "res://items/misc/area_trigger.tres" ),
			entrancePosition,
			World.ItemPlacement.Floor, World.ItemRotation.North );

		trigger.DestinationWorld = DestinationWorld;
		trigger.DestinationExit = "entrance";

		GD.Print( $"Spawned house entrance trigger at {entrancePosition} ({trigger.GlobalPosition})" );*/
	}
}
