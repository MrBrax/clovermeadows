using System.Linq;
using Godot;
using vcrossing.items;
using vcrossing2.items;

namespace vcrossing.Player;

public partial class PlayerInteract : Node3D
{
	private World World => GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;
	// private Node3D Model => GetNode<Node3D>( "../PlayerModel" );
	private PlayerController Player => GetNode<PlayerController>( "../" );

	public Vector3 GetBackPosition;
	public Vector3 GetBackRotation;
	public SittableNode SittingNode { get; set; }
	public LyingNode LyingNode { get; set; }

	public override void _Ready()
	{
	}

	public Vector2I GetAimingGridPosition()
	{
		if ( World == null ) throw new System.Exception( "World is null." );
		if ( Player.Model == null ) throw new System.Exception( "Model is null." );

		var currentPlayerGridPos = World.WorldToItemGrid( GlobalPosition );
		var aimDirectionYaw = Player.Model.RotationDegrees.Y;
		var gridDirection = World.Get8Direction( aimDirectionYaw );

		var nextGridPos = World.GetPositionInDirection( currentPlayerGridPos, gridDirection );

		GD.Print(
			$"AimGrid Current: {currentPlayerGridPos}, Yaw: {aimDirectionYaw}, Direction: {gridDirection}, Next: {nextGridPos}" );

		return nextGridPos;
	}

	public override void _Process( double delta )
	{
		if ( Input.IsActionJustPressed( "Interact" ) )
		{
			Interact();
		}
		else if ( Input.IsActionJustPressed( "PickUp" ) )
		{
			PickUp();
		}
		/*else if ( Input.IsActionJustPressed( "Drop" ) )
		{
			// var inventory = GetNode<Inventory>( "../Inventory" );
			// inventory.DropItem();
		}*/
	}

	private void PickUp()
	{
		var pos = GetAimingGridPosition();

		var items = World.GetItems( pos ).ToList();

		var floorItem = items.FirstOrDefault( i => i.Placement == World.ItemPlacement.Floor );
		var onTopItem = items.FirstOrDefault( i => i.Placement == World.ItemPlacement.OnTop );

		if ( onTopItem != null )
		{
			onTopItem.OnPlayerPickUp( this );
			return;
		}
		else if ( floorItem != null )
		{
			floorItem.OnPlayerPickUp( this );
			return;
		}

		var state = GetWorld3D().DirectSpaceState;
		var query = state.IntersectRay( PhysicsRayQueryParameters3D.Create( GlobalTransform.Origin, GlobalTransform.Origin + Player.Model.Basis.Z * 10 ) );
		if ( query.Count > 0 )
		{
			GD.Print( $"No item to pick up at {pos}, but there is a {query[0]}" );
		}

		GD.Print( $"No item to pick up at {pos}" );
		World.DebugPrint();
	}

	private void Interact()
	{
		
		if ( SittingNode != null )
		{
			GD.Print( "Getting up" );
			SittingNode.Occupant = null;
			SittingNode = null;
			GetBack();
			return;
		} else if ( LyingNode != null )
		{
			GD.Print( "Getting up" );
			LyingNode.Occupant = null;
			LyingNode = null;
			GetBack();
			return;
		}
		
		var pos = GetAimingGridPosition();

		var items = World.GetItems( pos ).ToList();

		if ( items.Count == 0 )
		{
			GD.Print( $"No items at {pos}" );
			return;
		}

		var floorItem = items.FirstOrDefault( i => i.Placement == World.ItemPlacement.Floor );
		var onTopItem = items.FirstOrDefault( i => i.Placement == World.ItemPlacement.OnTop );

		if ( onTopItem != null )
		{
			onTopItem.OnPlayerUse( this, pos );
			return;
		}
		else if ( floorItem != null )
		{
			floorItem.OnPlayerUse( this, pos );
			return;
		}

		GD.Print( $"No item to interact with at {pos}" );

		// World.SpawnPlacedItem( GD.Load<ItemData>( "res://items/misc/hole.tres" ), pos, World.ItemPlacement.Floor,
		// 	World.ItemRotation.North );
	}

	public void Sit( SittableNode sittableNode )
	{
		if ( SittingNode != null )
		{
			GD.Print( "Already sitting" );
			return;
		}

		GD.Print( $"Sitting at {sittableNode.Name}" );
		SittingNode = sittableNode;
		sittableNode.Occupant = this;
		GetBackPosition = Player.Position;
		GetBackRotation = Player.Model.RotationDegrees;
		Player.GlobalPosition = sittableNode.GlobalPosition;
		Player.Model.RotationDegrees = new Vector3( 0, sittableNode.GlobalRotationDegrees.Y, 0 );
	}
	
	public void Lie( LyingNode lyingNode )
	{
		if ( LyingNode != null )
		{
			GD.Print( "Already lying" );
			return;
		}

		GD.Print( $"Lying on {lyingNode.Name}" );
		LyingNode = lyingNode;
		lyingNode.Occupant = this;
		GetBackPosition = Player.Position;
		GetBackRotation = Player.Model.RotationDegrees;
		Player.GlobalPosition = lyingNode.GlobalPosition;
		Player.Model.RotationDegrees = lyingNode.GlobalRotationDegrees;
	}
	
	public void GetBack()
	{
		GD.Print( "Getting back" );
		Player.GlobalPosition = GetBackPosition;
		Player.Model.RotationDegrees = GetBackRotation;
	}
	
}
