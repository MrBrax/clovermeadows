using Godot;
using vcrossing2.Code.Player;
using vcrossing2.Code.WorldBuilder;

namespace vcrossing2.Code.Items;

public partial class AreaTrigger : WorldItem
{
	[Export] public WorldData DestinationWorld { get; set; }
	[Export] public string DestinationExit { get; set; }

	public override void _Ready()
	{
	}

	public override bool CanBePickedUp()
	{
		return false;
	}

	public override bool ShouldBeSaved()
	{
		return false;
	}
	
	public void OnAreaEntered( Node3D node )
	{
		if ( node is not PlayerController player )
		{
			// throw new System.Exception( "Area trigger entered by non-player." );
			GD.Print( "Area trigger entered by non-player." );
			return;
		}
		
		Activate();
	}

	public void Activate()
	{
		if ( DestinationWorld == null )
		{
			throw new System.Exception( "Destination scene not set." );
		}

		var player = GetNode<PlayerController>( "/root/Main/Player" );
		
		player.ExitName = DestinationExit;
		
		// var newWorldNode = DestinationScene.Instantiate<World>();

		/*var worldNode = GetNode<World>( "/root/Main/WorldContainer/World" );
		worldNode.QueueFree();

		newWorldNode.Name = "World";
		newWorldNode.WorldName = "House";

		// GetTree().Root.AddChild( newWorldNode );
		GetTree().Root.GetNode<Node3D>( "Main/WorldContainer" ).AddChild( newWorldNode );

		newWorldNode.Name = "World";*/
		
		var manager = GetNode<WorldManager>( "/root/Main/WorldContainer" );
		manager.LoadWorld( DestinationWorld );
		
		GD.Print( "New world node added. Entering new world." );
		player.OnAreaEntered();

		/*World.GetParent().AddChild( scene );
		playerInteract.GetParent().RemoveChild( playerInteract );
		scene.AddChild( playerInteract );
		playerInteract.GlobalTransform = GlobalTransform;
		playerInteract.GlobalTransform = playerInteract.GlobalTransform.LookingAt( GlobalTransform.origin, Vector3.Up );
		playerInteract.GlobalTransform = playerInter*/
	}

	public override void OnPlayerUse( PlayerInteract playerInteract, Vector2I pos )
	{
		Activate();
	}
}
