using Godot;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Player;
using vcrossing2.Code.WorldBuilder;

namespace vcrossing2.Code.Items;

public partial class AreaTrigger : Area3D /*, IUsable*/
{
	// [Export] public bool IsPlacedInEditor { get; set; }
	// [Export] public World.ItemPlacement Placement { get; set; }
	// [Export] public string ItemDataPath { get; set; }

	[Export( PropertyHint.File, "*.tres" )]
	public string DestinationWorld { get; set; }
	[Export]
	public string DestinationExit { get; set; }

	[Export]
	public float ActivationDelay { get; set; } = 1f;

	// rotation of the player when entering the trigger
	// [Export( PropertyHint.Range, "0,360,0.1" )]
	// public Vector3 MoveDirection { get; set; }

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered( Node body )
	{
		if ( body is not PlayerController player )
		{
			// throw new System.Exception( "Area trigger entered by non-player." );
			Logger.Info( "AreaTrigger", $"Area trigger entered by non-player: {body.Name}" );
			return;
		}

		Activate( player );
	}

	/* public bool ShouldBeSaved()
	{
		return false;
	} */

	/*public void OnAreaEntered( Node3D node )
	{
		if ( node is not PlayerController player )
		{
			// throw new System.Exception( "Area trigger entered by non-player." );
			Logger.Info( "Area trigger entered by non-player." );
			return;
		}

		Activate();
	}*/

	public void Activate( PlayerController playerController )
	{
		if ( string.IsNullOrEmpty( DestinationWorld ) )
		{
			throw new System.Exception(
				$"Destination world not set for area trigger {Name} (exit {DestinationExit})." );
		}


		playerController.CutsceneTarget = GlobalTransform.Origin + GlobalTransform.Basis.Z * 2;

		playerController.EmitSignal( PlayerController.SignalName.PlayerEnterArea, DestinationExit, DestinationWorld, ActivationDelay );

	}

	/*public override void OnPlayerUse( PlayerInteract playerInteract, Vector2I pos )
	{
		Activate();
	}*/

	/*public bool CanUse( PlayerController player )
	{
		return true;
	}

	public void OnUse( PlayerController player )
	{
		Activate( player );
	}*/
}
