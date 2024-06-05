using System.Linq;
using Godot;
using vcrossing.Code.Helpers;
using vcrossing.Code.Items;
using vcrossing.Code.Npc;

namespace vcrossing.Code.Player;

public partial class PlayerInteract : Node3D
{
	private World World => GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;

	// private Node3D Model => GetNode<Node3D>( "../PlayerModel" );
	// private PlayerController Player => GetNode<PlayerController>( "../" );
	private PlayerController Player => GetParent<PlayerController>();

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
		
		var gridWorldPosition = World.ItemGridToWorld( nextGridPos );
		if ( Mathf.Abs(GlobalPosition.Y - gridWorldPosition.Y) > 1f )
		{
			// GD.PushWarning( "Aiming at a higher position" );
			throw new System.Exception( $"Aiming at a higher position: {GlobalPosition} -> {gridWorldPosition}" );
		}

		Logger.Info("PlayerInteract",
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

		var nodeLinks = World.GetItems( pos ).ToList();

		var floorItem = nodeLinks.FirstOrDefault( i => i.GridPlacement == World.ItemPlacement.Floor );
		var onTopItem = nodeLinks.FirstOrDefault( i => i.GridPlacement == World.ItemPlacement.OnTop );

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
		var query = state.IntersectRay( PhysicsRayQueryParameters3D.Create( GlobalTransform.Origin,
			GlobalTransform.Origin + Player.Model.Basis.Z * 10 ) );
		if ( query.Count > 0 )
		{
			Logger.Info( $"No item to pick up at {pos}, but there is a {query[0]}" );
		}

		Logger.Info( $"No item to pick up at {pos}" );
		World.DebugPrint();
	}

	private void Interact()
	{
		// if sitting or lying, get up
		if ( SittingNode != null )
		{
			Logger.Info( "Getting up" );
			SittingNode.Occupant = null;
			SittingNode = null;
			GetBack();
			return;
		}
		else if ( LyingNode != null )
		{
			Logger.Info( "Getting up" );
			LyingNode.Occupant = null;
			LyingNode = null;
			GetBack();
			return;
		}

		// npc interaction
		var playerGlobalPosition = Player.GlobalPosition;
		var playerInteractPosition = playerGlobalPosition + Player.Model.Basis.Z * 1;
		// GetTree().CallGroup( "debugdraw", "add_sphere", playerInteractPosition, 0.5f );

		// var npcs = GetNode("/root/Main").GetChildren().Where( c => c is BaseNpc npc );
		// TODO: refine this
		/* var npcs = GetTree().GetNodesInGroup( "npc" )
			.Where( c => c is BaseNpc npc && npc.GlobalPosition.DistanceTo( playerInteractPosition ) < 1 )
			.Cast<BaseNpc>().ToList();
		if ( npcs.Count > 0 )
		{
			var npc = npcs.FirstOrDefault();
			if ( npc != null )
			{
				npc.OnUse( Player );
				return;
			}
		} */

		// var children = GetNode("/root/Main").FindChildren("*");
		var children = GetTree().GetNodesInGroup( "usables" );
		Logger.Info( "PlayerInteract", $"Children: {children.Count}" );
		foreach ( var child in children )
		{
			Logger.Info( "PlayerInteract", $"Child: {child.Name} ({child.GetType().Name})" );
		}
		var usables = children.Where( c => c is IUsable );
		Logger.Info( "PlayerInteract", $"Usables: {usables.Count()}" );
		var usable = usables.FirstOrDefault( c => c is Node3D node3d && node3d.GlobalPosition.DistanceTo( playerInteractPosition ) < 1 );
		Logger.Info( "PlayerInteract", $"Usable: {usable}" );
		if ( usable != null )
		{
			(usable as IUsable).OnUse( Player );
			return;
		}

		// grid interaction
		var aimingGridPosition = GetAimingGridPosition();

		// var items = World.GetItems( pos ).ToList();
		var floorItem = World.GetItem( aimingGridPosition, World.ItemPlacement.Floor );
		var onTopItem = World.GetItem( aimingGridPosition, World.ItemPlacement.OnTop );

		if ( floorItem == null && onTopItem == null )
		{
			Logger.Info( "PlayerInteract", $"No items at {aimingGridPosition}" );
			return;
		}

		// var floorItem = items.FirstOrDefault( i => i.GridPlacement == World.ItemPlacement.Floor );
		// var onTopItem = items.FirstOrDefault( i => i.GridPlacement == World.ItemPlacement.OnTop );

		if ( onTopItem != null )
		{
			onTopItem.OnPlayerUse( Player );
			return;
		}
		else if ( floorItem != null )
		{
			floorItem.OnPlayerUse( Player );
			return;
		}

		Logger.Info( $"No item to interact with at {aimingGridPosition}" );

		// World.SpawnPlacedItem( GD.Load<ItemData>( "res://items/misc/hole.tres" ), pos, World.ItemPlacement.Floor,
		// 	World.ItemRotation.North );
	}

	public void Sit( SittableNode sittableNode )
	{
		if ( SittingNode != null )
		{
			Logger.Info( "Already sitting" );
			return;
		}

		Logger.Info( $"Sitting at {sittableNode.Name}" );
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
			Logger.Info( "Already lying" );
			return;
		}

		Logger.Info( $"Lying on {lyingNode.Name}" );
		LyingNode = lyingNode;
		lyingNode.Occupant = this;
		GetBackPosition = Player.Position;
		GetBackRotation = Player.Model.RotationDegrees;
		Player.GlobalPosition = lyingNode.GlobalPosition;
		Player.Model.RotationDegrees = lyingNode.GlobalRotationDegrees;
	}

	public void GetBack()
	{
		Logger.Info( "Getting back" );
		Player.GlobalPosition = GetBackPosition;
		Player.Model.RotationDegrees = GetBackRotation;
	}
}
