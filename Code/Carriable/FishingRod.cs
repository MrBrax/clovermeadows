using System;
using vcrossing2.Code.Dependencies;
using vcrossing2.Code.Objects;
using vcrossing2.Code.Player;
using vcrossing2.Code.WorldBuilder;

namespace vcrossing2.Code.Carriable;

public partial class FishingRod : BaseCarriable
{


	[Export, Require] public PackedScene BobberScene { get; set; }

	[Export] public MeshInstance3D LineMesh { get; set; }

	[Export] public Node3D LinePoint { get; set; }

	private FishingBobber Bobber { get; set; }

	private bool _hasCasted = false;
	private bool _isCasting = false;

	private int _lineSegments = 5;

	public override void _Ready()
	{
		base._Ready();
		if ( IsInstanceValid( LineMesh ) ) LineMesh.Mesh = new ImmediateMesh();
	}

	public override void OnEquip( PlayerController player )
	{
		base.OnEquip( player );
		Logger.Info( "Equipped shovel." );
	}

	public override void OnUnequip( PlayerController player )
	{
		base.OnUnequip( player );
		Logger.Info( "Unequipped shovel." );
	}

	public override void OnUse( PlayerController player )
	{
		if ( !CanUse() )
		{
			return;
		}

		Logger.Info( "FishingRod", "Using fishing rod." );

		if ( _isCasting ) return;

		if ( _hasCasted && !IsInstanceValid( Bobber ) )
		{
			Logger.Warn( "FishingRod", "Bobber is not valid." );
			_hasCasted = false;
		}

		if ( _hasCasted )
		{
			ReelIn();
		}
		else
		{
			Cast();
		}


	}

	private Vector3 GetCastPosition()
	{
		return Player.GlobalTransform.Origin + Player.AimDirection * 3;
	}

	internal override bool ShouldDisableMovement()
	{
		return _hasCasted;
	}

	private async void Cast()
	{

		if ( Player == null ) throw new Exception( "Player is null." );

		if ( !CheckForWater( GetCastPosition() ) )
		{
			Logger.Warn( "FishingRod", $"No water found at {GetCastPosition()}." );
			return;
		}

		_isCasting = true;

		GetNode<AudioStreamPlayer3D>( "Cast" ).Play();

		GetNode<AnimationPlayer>( "AnimationPlayer" ).Play( "cast" );

		await ToSignal( GetTree().CreateTimer( 1f ), Timer.SignalName.Timeout );

		if ( !IsInstanceValid( Bobber ) )
		{
			var waterPosition = GetWaterSurface( GetCastPosition() );
			Bobber = BobberScene.Instantiate<FishingBobber>();
			Bobber.Rod = this;
			Player.World.AddChild( Bobber );
			Bobber.GlobalPosition = waterPosition;
		}

		CreateLine();

		_isCasting = false;

		_hasCasted = true;

	}

	private void CreateLine()
	{

		// var basePosition = GlobalTransform.Origin;

		var startPoint = LinePoint.GlobalPosition;
		var endPoint = Bobber.GlobalPosition;

		LineMesh.GlobalPosition = Vector3.Zero;
		LineMesh.GlobalRotation = new Vector3( 0, 0, 0 );

		var mesh = (ImmediateMesh)LineMesh.Mesh;

		mesh.ClearSurfaces();

		mesh.SurfaceBegin( Mesh.PrimitiveType.Lines );
		mesh.SurfaceSetColor( new Color( 0, 0, 0 ) );
		// mesh.SurfaceAddVertex( startPoint );

		// draw sagging line
		for ( var i = 0; i < _lineSegments; i++ )
		{
			var saggingPointStart = startPoint.Lerp( endPoint, i / (float)_lineSegments );
			var saggingPointEnd = startPoint.Lerp( endPoint, (i + 1) / (float)_lineSegments );

			saggingPointStart += Vector3.Down * Mathf.Sin( i / (float)_lineSegments * Mathf.Pi ) * 0.5f;
			saggingPointEnd += Vector3.Down * Mathf.Sin( (i + 1) / (float)_lineSegments * Mathf.Pi ) * 0.5f;

			mesh.SurfaceAddVertex( saggingPointStart );
			mesh.SurfaceAddVertex( saggingPointEnd );
		}

		// mesh.SurfaceAddVertex( endPoint );
		mesh.SurfaceEnd();

	}

	private bool CheckForWater( Vector3 position )
	{

		var spaceState = GetWorld3D().DirectSpaceState;

		var traceTerrain =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( position + Vector3.Up * 1f, position + Vector3.Down * 1f, World.TerrainLayer ) );

		if ( traceTerrain != null )
		{
			Logger.Warn( "FishingRod", $"Terrain found at {position}." );
			return false;
		}

		var traceWater =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( position + Vector3.Up * 1f, position + Vector3.Down * 1f, World.WaterLayer ) );

		if ( traceWater == null )
		{
			Logger.Warn( "FishingRod", $"No water found at {position}." );
			return false;
		}

		// TODO: check if it's the waterfall or something
		/* if ( traceWater.Normal != Vector3.Up )
		{
			Logger.Warn( "FishingRod", $"Water normal is not up ({traceWater.Normal})." );
			return false;
		} */

		return true;

	}

	public Vector3 GetWaterSurface( Vector3 position )
	{
		var spaceState = GetWorld3D().DirectSpaceState;

		var traceWater =
			new Trace( spaceState ).CastRay(
				PhysicsRayQueryParameters3D.Create( position + Vector3.Up * 1f, position + Vector3.Down * 1f, World.WaterLayer ) );

		if ( traceWater == null )
		{
			Logger.Warn( "FishingRod", $"No water found at {position}." );
			return Vector3.Zero;
		}

		return traceWater.Position;
	}

	private void ReelIn()
	{
		if ( IsInstanceValid( Bobber ) )
		{
			Bobber.QueueFree();
		}

		_hasCasted = false;

		((ImmediateMesh)LineMesh.Mesh).ClearSurfaces();
	}

	public async void CatchFish( Fish fish )
	{
		Logger.Info( "FishingRod", "Caught fish." );
		await ToSignal( GetTree().CreateTimer( 1f ), Timer.SignalName.Timeout );
		fish.QueueFree();
		ReelIn();
	}

}
