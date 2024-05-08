using System.Linq;
using Godot;

namespace vcrossing.Player;

public partial class PlayerInteract : Node3D
{
	private World World => GetNode<World>( "/root/Main/World" );


	private Node3D Model => GetNode<Node3D>( "../PlayerModel" );

	public override void _Ready()
	{
	}

	public Vector2I GetAimingGridPosition()
	{
		if ( World == null ) throw new System.Exception( "World is null." );
		if ( Model == null ) throw new System.Exception( "Model is null." );

		var currentPlayerGridPos = World.WorldToItemGrid( GlobalPosition );
		var aimDirectionYaw = Model.RotationDegrees.Y;
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
				onTopItem.OnPlayerUse( this );
				return;
			}
			else if ( floorItem != null )
			{
				floorItem.OnPlayerUse( this );
				return;
			}

			GD.Print( $"No item to interact with at {pos}" );

			// World.SpawnPlacedItem( GD.Load<ItemData>( "res://items/misc/hole.tres" ), pos, World.ItemPlacement.Floor,
			// 	World.ItemRotation.North );
		}
		else if ( Input.IsActionJustPressed( "PickUp" ) )
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

			GD.Print( $"No item to pick up at {pos}" );
			World.DebugPrint();
		}
		/*else if ( Input.IsActionJustPressed( "Drop" ) )
		{
			// var inventory = GetNode<Inventory>( "../Inventory" );
			// inventory.DropItem();
		}*/
	}
}
