﻿using vcrossing.Code.Items;
using vcrossing.Code.Save;
using vcrossing.Code.Ui;

namespace vcrossing.Code.Player;

public partial class PlayerInteract : Node3D
{
	private World World => GetNode<WorldManager>( "/root/Main/WorldManager" ).ActiveWorld;

	// private Node3D Model => GetNode<Node3D>( "../PlayerModel" );
	// private PlayerController Player => GetNode<PlayerController>( "../" );
	private PlayerController Player => GetParent<PlayerController>();

	public Vector3 GetBackPosition;
	public Vector3 GetBackRotation;
	public SittableNode SittingNode { get; set; }
	public LyingNode LyingNode { get; set; }

	[Export] public Node3D Crosshair { get; set; }

	[Export] public Area3D InteractBox { get; set; }
	[Export] public Node3D InteractPoint { get; set; }

	[Export] public Area3D NetBox { get; set; }

	public override void _Ready()
	{
	}

	private bool ShouldDisableInteract()
	{
		if ( Player.IsInVehicle ) return true;
		if ( SittingNode != null ) return true;
		if ( LyingNode != null ) return true;
		if ( World == null ) return true;
		if ( NodeManager.UserInterface.IsPaused ) return true;
		if ( NodeManager.UserInterface.AreWindowsOpen ) return true;
		return false;
	}

	public Vector2I GetAimingGridPosition()
	{
		/* if ( World == null ) throw new System.Exception( "World is null." );
		if ( Player.Model == null ) throw new System.Exception( "Model is null." );

		var currentPlayerGridPos = World.WorldToItemGrid( GlobalPosition );
		var aimDirectionYaw = Player.Model.RotationDegrees.Y;
		var gridDirection = World.Get8Direction( aimDirectionYaw );

		var nextGridPos = World.GetPositionInDirection( currentPlayerGridPos, gridDirection );

		var gridWorldPosition = World.ItemGridToWorld( nextGridPos );
		if ( Mathf.Abs( GlobalPosition.Y - gridWorldPosition.Y ) > 1f )
		{
			// GD.PushWarning( "Aiming at a higher position" );
			throw new System.Exception( $"Aiming at a higher position: {GlobalPosition} -> {gridWorldPosition}" );
		}

		// Logger.Info( "PlayerInteract",
		// 	$"AimGrid Current: {currentPlayerGridPos}, Yaw: {aimDirectionYaw}, Direction: {gridDirection}, Next: {nextGridPos}" );

		return nextGridPos; */

		var boxPos = InteractPoint.GlobalTransform.Origin;

		var gridPosition = World.WorldToItemGrid( boxPos );
		var worldPosition = World.ItemGridToWorld( gridPosition );

		if ( Mathf.Abs( boxPos.Y - worldPosition.Y ) > 1f )
		{
			// throw new System.Exception( $"Aiming at a higher position: {boxPos} -> {worldPosition}" );
			return default;
		}

		return gridPosition;

	}

	public Godot.Collections.Array<Node3D> GetInteractBoxNodes()
	{
		if ( InteractBox == null ) throw new System.Exception( "InteractBox is null." );
		return InteractBox.GetOverlappingBodies();
	}

	public override void _UnhandledInput( InputEvent @event )
	{

		if ( CheckForExitLyingAndSitting( @event ) ) return;

		if ( ShouldDisableInteract() ) return;

		if ( @event.IsActionPressed( "Interact" ) )
		{
			Interact();
		}
		else if ( @event.IsActionPressed( "PickUp" ) )
		{
			PickUp();
		}
		else if ( @event.IsActionPressed( "Drop" ) )
		{
			// var inventory = GetNode<Inventory>( "../Inventory" );
			// inventory.DropItem();
		}
		else if ( @event is InputEventMouseButton inputEventMouseButton && GetNode<SettingsSaveData>( "/root/SettingsSaveData" ).CurrentSettings.PlayerMouseControl )
		{
			if ( inputEventMouseButton.IsPressed() && inputEventMouseButton.ButtonIndex == MouseButton.Left )
			{
				MouseInteract();
			}
			else if ( inputEventMouseButton.IsPressed() && inputEventMouseButton.ButtonIndex == MouseButton.Right )
			{
				MousePickUp();
			}
		}

	}

	private bool CheckForExitLyingAndSitting( InputEvent @event )
	{
		if ( @event.IsActionPressed( "Interact" ) )
		{
			// if sitting or lying, get up
			if ( SittingNode != null )
			{
				Logger.Info( "Getting up" );
				SittingNode.Occupant = null;
				SittingNode = null;
				GetBack();
				return true;
			}
			else if ( LyingNode != null )
			{
				Logger.Info( "Getting up" );
				LyingNode.Occupant = null;
				LyingNode = null;
				GetBack();
				return true;
			}
		}

		return false;
	}

	private void MouseInteract()
	{
		// get mouse position
		var mousePosition = GetViewport().GetMousePosition();

		// trace a ray from the camera to the mouse position
		var spaceState = GetWorld3D().DirectSpaceState;

		var camera = GetTree().GetNodesInGroup( "camera" )[0] as Camera3D;

		var from = camera.ProjectRayOrigin( mousePosition );
		var to = from + camera.ProjectRayNormal( mousePosition ) * 1000;

		var result = new Trace( spaceState ).CastRay( PhysicsRayQueryParameters3D.Create( from, to, 1 << 13 ) );

		if ( result == null )
		{
			Logger.Info( "MouseInteract", "No item to interact with" );
			return;
		}

		/* var item = result.Collider as Node3D;
		if ( item != null )
		{
			Logger.Info( "MouseInteract", $"Interacting with {item.Name}" );
			if ( item is IUsable iUsable )
			{
				if ( iUsable.CanUse( Player ) )
				{
					iUsable.OnUse( Player );
				}
				else
				{
					Logger.Info( "MouseInteract", $"Cannot use {item.Name} (returned false from CanUse)" );
				}
			}
		} */

		Node3D node = result.Collider;

		Logger.Info( "MouseInteract", $"Interacting with {node.Name}" );

		var iUsableNode = node.GetAncestorOfType<IUsable>();

		if ( iUsableNode == null )
		{
			Logger.Info( "MouseInteract", "No usable item found" );
			return;
		}

		if ( node.GlobalPosition.DistanceTo( GlobalPosition ) > 1.5f )
		{
			Logger.Info( "MouseInteract", $"Too far from {node.Name}" );
			return;
		}

		if ( iUsableNode.CanUse( Player ) )
		{
			iUsableNode.OnUse( Player );
		}
		else
		{
			Logger.Info( "MouseInteract", $"Cannot use {node.Name} (returned false from CanUse)" );
		}

	}

	private void MousePickUp()
	{
		// get mouse position
		var mousePosition = GetViewport().GetMousePosition();

		// trace a ray from the camera to the mouse position
		var spaceState = GetWorld3D().DirectSpaceState;

		var camera = GetTree().GetNodesInGroup( "camera" )[0] as Camera3D;

		var from = camera.ProjectRayOrigin( mousePosition );
		var to = from + camera.ProjectRayNormal( mousePosition ) * 1000;

		var result = new Trace( spaceState ).CastRay( PhysicsRayQueryParameters3D.Create( from, to, World.TerrainLayer ) );

		if ( result == null )
		{
			return;
		}

		var worldPosition = result.Position;

		if ( worldPosition.DistanceTo( GlobalPosition ) > 1.5f )
		{
			return;
		}

		var gridPosition = World.WorldToItemGrid( worldPosition );

		var nodeLinks = World.GetItems( gridPosition ).ToList();

		var floorItem = nodeLinks.FirstOrDefault( i => i.GridPlacement == World.ItemPlacement.Floor );
		var onTopItem = nodeLinks.FirstOrDefault( i => i.GridPlacement == World.ItemPlacement.OnTop );

		if ( onTopItem != null )
		{
			Player.ModelLookAt( onTopItem.Node.GlobalPosition );
			onTopItem.OnPlayerPickUp( this );
			return;
		}
		else if ( floorItem != null )
		{
			Player.ModelLookAt( floorItem.Node.GlobalPosition );
			floorItem.OnPlayerPickUp( this );
			return;
		}

	}


	public override void _Process( double delta )
	{
		RenderCrosshair();
	}

	private void RenderCrosshair()
	{
		if ( Crosshair == null ) return;

		if ( World == null || Player.IsInVehicle || !GetNode<SettingsSaveData>( "/root/SettingsSaveData" ).CurrentSettings.ShowCrosshair )
		{
			Crosshair.Visible = false;
			return;
		}

		Crosshair.Visible = true;

		var aimingGridPosition = GetAimingGridPosition();
		var aimingWorldPosition = World.ItemGridToWorld( aimingGridPosition );
		Crosshair.GlobalPosition = Crosshair.GlobalPosition.Lerp( aimingWorldPosition, 0.5f );
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

		/* var state = GetWorld3D().DirectSpaceState;
		var query = new Trace( state ).CastRay( PhysicsRayQueryParameters3D.Create( GlobalTransform.Origin,
			GlobalTransform.Origin + Player.Model.Basis.Z * 10 ) );

		if ( query != null )
		{
			Logger.Info( $"No item to pick up at {pos}, but there is a {query.Collider}" );
		}

		Logger.Info( $"No item to pick up at {pos}" );
		World.DebugPrint(); */

		var nodes = GetInteractBoxNodes();
		foreach ( var node in nodes )
		{
			// Logger.Info( "PlayerInteract", $"Box node: {node.Name} ({node.GetType()})" );
			/* Node3D baseNode;

			if ( node is IUsable )
			{
				baseNode = node;
			}
			else
			{

				var iUsable = node.GetAncestorOfType<IUsable>();

				if ( iUsable == null )
				{
					Logger.Info( "PlayerInteract", $"No usable node found in {node.Name}" );
					continue;
				}

				baseNode = iUsable as Node3D;

			}

			var usable = baseNode as IUsable; */

			var baseNode = node.GetAncestorOfType<IPickupable>();

			if ( baseNode == null )
			{
				Logger.Info( "PlayerInteract", $"No usable node found in {node.Name}" );
				continue;
			}

			if ( baseNode.CanPickup( Player ) )
			{
				baseNode.OnPickup( Player );
				return;
			}
		}


	}

	private void Interact()
	{
		Logger.Info( "PlayerInteract", "Interact" );

		// npc interaction
		var playerGlobalPosition = Player.GlobalPosition;
		var playerInteractPosition = playerGlobalPosition + Player.Model.Basis.Z * 1;
		// GetTree().CallGroup( "debugdraw", "add_sphere", playerInteractPosition, 0.5f );


		/* // var children = GetNode("/root/Main").FindChildren("*");
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
		
		if ( usable is IUsable iUsable )
		{
			if ( iUsable.CanUse( Player ) )
			{
				iUsable.OnUse( Player );
			}
			else
			{
				Logger.Info( "PlayerInteract", $"Cannot use {usable.Name} (returned false from CanUse)" );
			}
			return;
		} */

		var nodes = GetInteractBoxNodes();
		foreach ( var node in nodes )
		{
			// Logger.Info( "PlayerInteract", $"Box node: {node.Name} ({node.GetType()})" );
			Node3D baseNode;

			if ( node is IUsable )
			{
				baseNode = node;
			}
			else
			{

				var iUsable = node.GetAncestorOfType<IUsable>();

				if ( iUsable == null )
				{
					Logger.Info( "PlayerInteract", $"No usable node found in {node.Name}" );
					continue;
				}

				baseNode = iUsable as Node3D;

			}

			var usable = baseNode as IUsable;

			if ( usable.CanUse( Player ) )
			{
				usable.OnUse( Player );
				return;
			}
		}
		/* var usable = nodes.FirstOrDefault( n => n is IUsable iUsable );
		if ( usable is IUsable iUsable )
		{
			Logger.Info( "PlayerInteract", $"Interacting with {usable.Name}" );
			if ( iUsable.CanUse( Player ) )
			{
				iUsable.OnUse( Player );
				return;
			}
			Logger.Info( "PlayerInteract", $"Cannot use {usable.Name} (returned false from CanUse)" );
		} */


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
