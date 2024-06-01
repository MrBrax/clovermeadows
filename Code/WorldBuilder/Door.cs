using System;
using System.Text.RegularExpressions;
using Godot;
using vcrossing2.Code.Dependencies;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;

public partial class Door : Node3D, IUsable
{

	[Export, Require] public Node3D DoorModel { get; set; }
	[Export, Require] public StaticBody3D Collider { get; set; }

	[Export] public AudioStreamPlayer3D OpenSound { get; set; }
	[Export] public AudioStreamPlayer3D CloseSound { get; set; }
	[Export] public AudioStreamPlayer3D SqueakSound { get; set; }

	private bool _isBeingUsed;

	private bool _openState;
	private float _openAngle = -90;
	private float _lastUse;

	private float _doorSpeed = 0.5f;

	override public void _Ready()
	{
		AddToGroup( "usables" );
	}

	public bool CanUse( PlayerController player )
	{
		return true;
	}

	public void OnUse( PlayerController player )
	{
		PlayerEnter( player );
	}

	public void SetState( bool state )
	{
		_openState = state;
		DoorModel.RotationDegrees = new Vector3( 0, state ? _openAngle : 0, 0 );
	}

	public void Open()
	{
		// if ( _openState ) return;
		_openState = true;
		_lastUse = Time.GetTicksMsec();
		_isBeingUsed = true;
		SetCollision( false );
		OpenSound?.Play();
		SqueakSound?.Play();
	}

	public void Close()
	{
		// if ( !_openState ) return;
		_openState = false;
		_lastUse = Time.GetTicksMsec();
		_isBeingUsed = true;
		SqueakSound?.Play();
	}

	private async void PlayerEnter( PlayerController player )
	{
		if ( _isBeingUsed ) return;

		player.Velocity = Vector3.Zero;

		Open();

		SetCollision( false );

		// wait just a bit before moving the player
		await ToSignal( GetTree().CreateTimer( _doorSpeed ), Timer.SignalName.Timeout );

		// move the player through the door
		// player.Velocity = new Vector3( 0, 0, -2 );
		player.InCutscene = true;
		player.CutsceneTarget = GlobalPosition + new Vector3( 0.5f, 0, -2 );

		// await ToSignal( GetTree(), SceneTree.SignalName.ProcessFrame );

		// wait for the player to move through the door
		await ToSignal( GetTree().CreateTimer( 0.5 ), Timer.SignalName.Timeout );

		// check if we're still in the same world or valid
		if ( !IsInstanceValid( this ) )
		{
			Logger.Warn( "Door", "Door instance is invalid" );
			return;
		}

		Close();

		await ToSignal( GetTree().CreateTimer( _doorSpeed + 0.2f ), Timer.SignalName.Timeout );

		// re-enable collision on the door
		// collider.Disabled = false;
		SetCollision( true );

		player.InCutscene = false;

		_isBeingUsed = false;
	}

	private void SetCollision( bool state )
	{

		// disable collision on the door
		/* foreach ( var child in GetChildren() )
		{
			if ( child is CollisionShape3D collider )
			{
				collider.Disabled = !state;
			}
		} */

		// disable collision on the door
		Collider.CollisionLayer = state ? 1u : 0u;
		Collider.CollisionMask = state ? 1u : 0u;

		Collider.GetChild<CollisionShape3D>( 0 ).Disabled = !state;

		Logger.Info( "Door", $"Collision: {state}" );

	}

	override public void _Process( double delta )
	{
		if ( !_isBeingUsed ) return;
		var time = Time.GetTicksMsec();
		if ( _isBeingUsed && time - _lastUse > _doorSpeed * 1000 )
		{
			_isBeingUsed = false;
			if ( !_openState )
			{
				SetCollision( true );
				CloseSound?.Play();
			}
			SqueakSound?.Stop();
		}

		// animate door opening/closing by rotating it

		var sourceAngle = _openState ? 0 : _openAngle;
		var destinationAngle = _openState ? _openAngle : 0;

		var frac = (float)(time - _lastUse) / (_doorSpeed * 1000);

		var angle = Mathf.CubicInterpolate( sourceAngle, destinationAngle, 0, 1, frac );

		DoorModel.RotationDegrees = new Vector3( 0, angle, 0 );


	}
}
