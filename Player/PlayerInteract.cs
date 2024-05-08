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
		
		return nextGridPos;

	}

	public override void _Process( double delta )
	{
		
		if ( Input.IsActionJustPressed( "Interact" ) )
		{
			var pos = GetAimingGridPosition();

			// var item = World.GetItems
			// World.SpawnPlacedItem( GD.Load<ItemData>( "res://items/misc/hole.tres" ), pos, World.ItemPlacement.Floor,
			// 	World.ItemRotation.North );
		}
		
	}
	
}
