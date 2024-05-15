using Godot;
using System;
using vcrossing2.Code;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Player;

public partial class PlayerCamera : Camera3D
{
	private PlayerController Player => GetNode<PlayerController>( "../" );
	private World World => GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;

	private Vector2I CurrentAcre = new Vector2I( 0, 0 );

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process( double delta )
	{
		
	}

	private void AcreCamera( double delta )
	{
		var newAcreCheck = World.GetAcreFromWorldPosition( Player.GlobalPosition );
		// Logger.Info( $"Player is in acre {newAcreCheck}" );
		if ( newAcreCheck != CurrentAcre )
		{
			AcreChanged( newAcreCheck );
		}

		var acreWidth = World.GridSize * World.AcreWidth;
		var acreHeight = World.GridSize * World.AcreHeight;
		var edgeBoundsX = 4f;
		var edgeBoundsY = 3f;

		// Vector3 minCameraPos = Vector3.Zero;
		// Vector3 maxCameraPos = new Vector3( acreWidth, 0, acreHeight );
		var minCameraPos = new Vector3( acreWidth * CurrentAcre.X + edgeBoundsX, 0,
			acreHeight * CurrentAcre.Y + edgeBoundsY );
		var maxCameraPos = new Vector3( acreWidth * CurrentAcre.X + acreWidth - edgeBoundsX, 0,
			acreHeight * CurrentAcre.Y + acreHeight - edgeBoundsY );

		var playerPos = Player.GlobalPosition;

		var cameraPosX = Mathf.Clamp( playerPos.X, minCameraPos.X, maxCameraPos.X );
		var cameraPosZ = Mathf.Clamp( playerPos.Z, minCameraPos.Z, maxCameraPos.Z );

		var cameraPos = new Vector3( cameraPosX, 0, cameraPosZ );

		cameraPos += new Vector3( 0, 8f, 5f );

		GlobalPosition = GlobalPosition.Lerp( cameraPos, (float)delta * 5f );
	}

	private void AcreChanged( Vector2I newAcreCheck )
	{
		Logger.Info( $"Player moved from {CurrentAcre} to {newAcreCheck}" );
		CurrentAcre = newAcreCheck;
	}
}
