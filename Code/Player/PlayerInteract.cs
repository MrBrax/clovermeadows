using System;
using vcrossing.Code.Items;
using vcrossing.Code.Save;
using vcrossing.Code.Ui;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Player;

public partial class PlayerInteract : Node3D
{
	private World World => NodeManager.WorldManager.ActiveWorld;

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
		UpdateButtonPrompts();
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

	// private bool _showInteractButtonPrompt;
	/* private bool ShowInteractButtonPrompt
	{
		get => _showInteractButtonPrompt;
		set
		{
			_showInteractButtonPrompt = value;
			UpdateButtonPrompts();
		}
	} */

	// private bool _showPickUpButtonPrompt;
	/* private bool ShowPickUpButtonPrompt
	{
		get => _showPickUpButtonPrompt;
		set
		{
			_showPickUpButtonPrompt = value;
			UpdateButtonPrompts();
		}
	} */

	private void UpdateButtonPrompts()
	{

		var _promptContainer = NodeManager.UserInterface.GetNode<Control>( "%ButtonPrompts" );
		var _pickUpButton = _promptContainer.GetNode<ButtonPrompt>( "%PromptPickUp" );
		var _interactButton = _promptContainer.GetNode<ButtonPrompt>( "%PromptInteract" );

		var showPickUpButton = false;
		var showInteractButton = false;

		if ( ShouldDisableInteract() )
		{
			_pickUpButton?.Hide();
			_interactButton?.Hide();
			return;
		}

		// Node3D mainNode = null;

		/* var nodes = GetInteractBoxNodes();
		foreach ( var node in nodes )
		{
			if ( node is IUsable iUsable )
			{
				showInteractButton = iUsable.CanUse( Player );
				// _interactButton.Visible = showInteractButton;
				if ( showInteractButton )
				{
					mainNode = node;
				}
			}

			if ( node is IPickupable iPickupable )
			{
				showPickUpButton = iPickupable.CanPickup( Player );
				// _pickUpButton.Visible = showPickUpButton;
				if ( showPickUpButton )
				{
					mainNode = node;
				}
			}
		}

		// var aimingGridPosition = GetAimingGridPosition();
		// var floorItem = World.GetItem( aimingGridPosition, World.ItemPlacement.Floor );
		// var onTopItem = World.GetItem( aimingGridPosition, World.ItemPlacement.OnTop );

		if ( mainNode != null && (showInteractButton || showPickUpButton) )
		{
			_promptContainer.Visible = true;
			_promptContainer.GlobalPosition = GetViewport().GetCamera3D().UnprojectPosition( mainNode.GlobalPosition );
		}
		else
		{
			// _promptContainer.Visible = false;
			var aimingGridPosition = GetAimingGridPosition();
			var floorItem = World.GetItem( aimingGridPosition, World.ItemPlacement.Floor );
			var onTopItem = World.GetItem( aimingGridPosition, World.ItemPlacement.OnTop );

			if ( floorItem != null && floorItem.CanPlayerUse( Player ) )
			{
				showInteractButton = true;
				mainNode = floorItem.Node;
			}

			if ( onTopItem != null && onTopItem.CanPlayerUse( Player ) )
			{
				showInteractButton = true;
				mainNode = onTopItem.Node;
			}

			if ( floorItem != null && floorItem.CanPlayerPickUp( this ) )
			{
				showPickUpButton = true;
				mainNode = floorItem.Node;
			}

			if ( onTopItem != null && onTopItem.CanPlayerPickUp( this ) )
			{
				showPickUpButton = true;
				mainNode = onTopItem.Node;
			}


		} */

		/* if ( !showInteractButton )
		{
			_interactButton.Visible = false;
		}

		if ( !showPickUpButton )
		{
			_pickUpButton.Visible = false;
		} */

		var interactableNode = GetInteractableNode();
		showInteractButton = interactableNode.node != null && interactableNode.iUsable != null;
		if ( showInteractButton )
		{
			_interactButton.SetLabel( interactableNode.iUsable.GetUseText() );
		}

		var pickupableNode = GetPickupableNode();
		showPickUpButton = pickupableNode.node != null && pickupableNode.iPickupable != null;

		// GD.Print( $"Interactable: {interactableNode.node?.Name}, Pickupable: {pickupableNode.node?.Name}" );

		_interactButton.Visible = showInteractButton;
		_pickUpButton.Visible = showPickUpButton;

		Node3D mainNode = showInteractButton ? interactableNode.node : pickupableNode.node;

		if ( mainNode != null && (showInteractButton || showPickUpButton) )
		{
			_promptContainer.Visible = true;
			_promptContainer.GlobalPosition = GetViewport().GetCamera3D().UnprojectPosition( mainNode.GlobalPosition + Vector3.Up * 0.5f );
		}
		else
		{
			_promptContainer.Visible = false;
		}

	}

	private void PickUp()
	{

		var node = GetPickupableNode();

		if ( node.node != null && node.iPickupable != null )
		{

			var nodeLink = World.GetNodeLink( node.node );

			if ( nodeLink != null )
			{
				nodeLink.OnPlayerPickUp( this );
				return;
			}

			// TODO: pick up non-nodelink items

			/* if ( !CanBePickedUp() )
			{
				Logger.Info( $"Cannot pick up {GetName()}" );
				return;
			}

			var playerInventory = playerInteract.GetNode<Components.Inventory>( "../PlayerInventory" );
			playerInventory.PickUpItem( this );

			node.iPickupable.OnPickup( Player ); */
			return;
		}

	}

	private Tuple<Node3D, IUsable, IPickupable> GetMainNode()
	{
		var aimingGridPosition = GetAimingGridPosition();

		var onTopItem = World.GetItem( aimingGridPosition, World.ItemPlacement.OnTop );
		if ( onTopItem != null )
		{
			var onTopItemUsable = onTopItem.Node.GetAncestorOfType<IUsable>();
			var onTopItemPickupable = onTopItem.Node.GetAncestorOfType<IPickupable>();

			if ( onTopItemUsable != null || onTopItemPickupable != null )
			{
				return new Tuple<Node3D, IUsable, IPickupable>( onTopItem.Node, onTopItemUsable, onTopItemPickupable );
			}
		}

		var floorItem = World.GetItem( aimingGridPosition, World.ItemPlacement.Floor );
		if ( floorItem != null )
		{
			var floorItemUsable = floorItem.Node.GetAncestorOfType<IUsable>();
			var floorItemPickupable = floorItem.Node.GetAncestorOfType<IPickupable>();

			if ( floorItemUsable != null || floorItemPickupable != null )
			{
				return new Tuple<Node3D, IUsable, IPickupable>( floorItem.Node, floorItemUsable, floorItemPickupable );
			}
		}

		var nodes = GetInteractBoxNodes();
		foreach ( var node in nodes )
		{
			var usable = node.GetAncestorOfType<IUsable>();
			var pickupable = node.GetAncestorOfType<IPickupable>();

			if ( usable != null || pickupable != null )
			{
				return new Tuple<Node3D, IUsable, IPickupable>( node, usable, pickupable );
			}
		}

		/* var floorItem = World.GetItem( aimingGridPosition, World.ItemPlacement.Floor );
		if ( floorItem != null )
		{
			var floorItemUsable = floorItem.Node.GetAncestorOfType<IUsable>();
			if ( floorItemUsable != null ) return floorItem.Node;

			var floorItemPickupable = floorItem.Node.GetAncestorOfType<IPickupable>();
			if ( floorItemPickupable != null ) return floorItem.Node;
		}

		var nodes = GetInteractBoxNodes();
		foreach ( var node in nodes )
		{
			var usable = node.GetAncestorOfType<IUsable>();
			if ( usable != null ) return node;

			var pickupable = node.GetAncestorOfType<IPickupable>();
			if ( pickupable != null ) return node;
		} */

		return null;

	}

	// TODO: check if being picked up
	private (Node3D node, IUsable iUsable) GetInteractableNode()
	{
		/* var nodes = GetInteractBoxNodes();
		foreach ( var node in nodes )
		{
			var usable = node.GetAncestorOfType<IUsable>();

			if ( usable != null && usable.CanUse( Player ) )
			{
				// Logger.Info( "PlayerInteract", $"Found usable item: {node.Name}" );
				return (node, usable);
			}
		}

		var aimingGridPosition = GetAimingGridPosition();

		var floorItem = World.GetItem( aimingGridPosition, World.ItemPlacement.Floor );
		if ( floorItem != null && floorItem.CanPlayerUse( Player ) )
		{
			// Logger.Info( "PlayerInteract", $"Found floor item: {floorItem.Node.Name}" );
			return (floorItem.Node, floorItem.Node.GetAncestorOfType<IUsable>());
		}

		var onTopItem = World.GetItem( aimingGridPosition, World.ItemPlacement.OnTop );
		if ( onTopItem != null && onTopItem.CanPlayerUse( Player ) )
		{
			// Logger.Info( "PlayerInteract", $"Found on top item: {onTopItem.Node.Name}" );
			return (onTopItem.Node, onTopItem.Node.GetAncestorOfType<IUsable>());
		}

		// Logger.Info( "PlayerInteract", "No interactable item found" );

		return (null, null); */

		var mainNode = GetMainNode();

		if ( mainNode == null ) return (null, null);

		return mainNode.Item2 != null && mainNode.Item2.CanUse( Player ) ? (mainNode.Item1, mainNode.Item2) : (null, null);
	}

	private (Node3D node, IPickupable iPickupable) GetPickupableNode()
	{
		var mainNode = GetMainNode();

		if ( mainNode == null ) return (null, null);

		return mainNode.Item3 != null && mainNode.Item3.CanPickup( Player ) ? (mainNode.Item1, mainNode.Item3) : (null, null);
	}

	private void Interact()
	{
		Logger.Info( "PlayerInteract", "Interact" );

		/* var nodes = GetInteractBoxNodes();
		foreach ( var node in nodes )
		{
			var usable = node.GetAncestorOfType<IUsable>();

			if ( usable != null && usable.CanUse( Player ) )
			{
				usable.OnUse( Player );
				return;
			}
		}

		// grid interaction
		var aimingGridPosition = GetAimingGridPosition();

		var floorItem = World.GetItem( aimingGridPosition, World.ItemPlacement.Floor );
		var onTopItem = World.GetItem( aimingGridPosition, World.ItemPlacement.OnTop );

		if ( floorItem == null && onTopItem == null )
		{
			Logger.Info( "PlayerInteract", $"No items at {aimingGridPosition}" );
			return;
		}

		if ( onTopItem != null )
		{
			onTopItem.OnPlayerUse( Player );
			return;
		}
		else if ( floorItem != null )
		{
			floorItem.OnPlayerUse( Player );
			return;
		} */

		var node = GetInteractableNode();

		if ( node.node != null && node.iUsable != null )
		{
			node.iUsable.OnUse( Player );
			return;
		}

		// Logger.Debug( $"No item to interact with at {aimingGridPosition}" );

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
