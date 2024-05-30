using System;
using System.Text.RegularExpressions;
using Godot;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;

public partial class Door : Node3D, IUsable
{

	[Export] public StaticBody3D Collider { get; set; }

	private bool _isBeingUsed;

	private bool _openState;
	private float _openAngle = -90;
	private float _lastUse;

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
		RotationDegrees = new Vector3( 0, state ? _openAngle : 0, 0 );
	}

	public void Open()
	{
		if ( _openState ) return;
		_openState = true;
		_lastUse = Time.GetTicksMsec();
		_isBeingUsed = true;
	}

	public void Close()
	{
		if ( !_openState ) return;
		_openState = false;
		_lastUse = Time.GetTicksMsec();
		_isBeingUsed = true;
	}

	private async void PlayerEnter( PlayerController player )
	{
		if ( _isBeingUsed ) return;

		Open();

		SetCollision( false );

		// wait just a bit before moving the player
		await ToSignal( GetTree().CreateTimer( 0.5f ), Timer.SignalName.Timeout );

		// move the player through the door
		player.Velocity = new Vector3( 0, 0, -2 );
		player.InCutscene = true;

		// await ToSignal( GetTree(), SceneTree.SignalName.ProcessFrame );

		// wait for the player to move through the door
		await ToSignal( GetTree().CreateTimer( 0.5 ), Timer.SignalName.Timeout );

		// check if we're still in the same world or valid
		if ( !IsInstanceValid( this ) ) {
			Logger.Warn( "Door", "Door instance is invalid" );
			return;
		}

		Close();

		await ToSignal( GetTree().CreateTimer( 0.5f ), Timer.SignalName.Timeout );

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
		if ( _isBeingUsed && Time.GetTicksMsec() - _lastUse > 1000 )
		{
			_isBeingUsed = false;
		}

		// animate door opening/closing by rotating it

		var destionationAngle = _openState ? _openAngle : 0;

		var angle = Mathf.Lerp( RotationDegrees.Y, destionationAngle, (float)delta * 10f );

		RotationDegrees = new Vector3( 0, angle, 0 );

		/* var angle = RotationDegrees.Y;
		if ( angle < _openAngle )
		{
			angle += 90f * -(float)delta;
			RotationDegrees = new Vector3( 0, angle, 0 );
		}
		else if ( angle > 0 )
		{
			angle -= 90f * -(float)delta;
			RotationDegrees = new Vector3( 0, angle, 0 );
		}
		else
		{
			_isBeingUsed = false;
		} */
	}
}
