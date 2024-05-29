using Godot;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Player;
using vcrossing2.Code.WorldBuilder;

namespace vcrossing2.Code.Items;

public partial class AreaTrigger : Area3D, IUsable
{
	[Export] public bool IsPlacedInEditor { get; set; }
	[Export] public World.ItemPlacement Placement { get; set; }
	[Export] public string ItemDataPath { get; set; }

	[Export] public string DestinationWorld { get; set; }
	[Export] public string DestinationExit { get; set; }

	public override void _Ready()
	{
	}

	/* public bool ShouldBeSaved()
	{
		return false;
	} */

	public void OnAreaEntered( Node3D node )
	{
		if ( node is not PlayerController player )
		{
			// throw new System.Exception( "Area trigger entered by non-player." );
			Logger.Info( "Area trigger entered by non-player." );
			return;
		}

		Activate();
	}

	public void Activate()
	{
		if ( string.IsNullOrEmpty( DestinationWorld ) )
		{
			throw new System.Exception(
				$"Destination world not set for area trigger {Name} (exit {DestinationExit})." );
		}

		var player = GetNode<PlayerController>( "/root/Main/Player" );

		player.ExitName = DestinationExit;
		Logger.Info( "AreaTrigger", $"Player exit set to {DestinationExit}" );

		// var newWorldNode = DestinationScene.Instantiate<World>();

		/*var worldNode = GetNode<World>( "/root/Main/WorldContainer/World" );
		worldNode.QueueFree();

		newWorldNode.Name = "World";
		newWorldNode.WorldName = "House";

		// GetTree().Root.AddChild( newWorldNode );
		GetTree().Root.GetNode<Node3D>( "Main/WorldContainer" ).AddChild( newWorldNode );

		newWorldNode.Name = "World";*/

		Logger.Info("AreaTrigger", "New world node added. Entering new world." );

		var manager = GetNode<WorldManager>( "/root/Main/WorldContainer" );
		manager.LoadWorld( DestinationWorld );

		Logger.Info("AreaTrigger", "World loaded sync." );

		// player.OnAreaEntered();

		/*World.GetParent().AddChild( scene );
		playerInteract.GetParent().RemoveChild( playerInteract );
		scene.AddChild( playerInteract );
		playerInteract.GlobalTransform = GlobalTransform;
		playerInteract.GlobalTransform = playerInteract.GlobalTransform.LookingAt( GlobalTransform.origin, Vector3.Up );
		playerInteract.GlobalTransform = playerInter*/
	}

	/*public override void OnPlayerUse( PlayerInteract playerInteract, Vector2I pos )
	{
		Activate();
	}*/

	public bool CanUse( PlayerController player )
	{
		return true;
	}

	public void OnUse( PlayerController player )
	{
		Activate();
	}
}
