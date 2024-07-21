using System;
using vcrossing.Code.Data;
using vcrossing.Code.Inventory;
using vcrossing.Code.Items;
using vcrossing.Code.Objects;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Carriable;

public sealed partial class PelletGun : BaseCarriable
{

	[Export] public float FireRate = 3f;
	[Export] public float PelletSpeed = 10f;
	[Export] public PackedScene PelletScene;

	[Export] public PackedScene PelletGunFpsScene;

	private float _fireTimer;

	private bool _isAiming;

	private bool _isWaitingForHit;

	/// <summary>
	///  A scene that contains a camera and a gun model. When aiming, this scene is instantiated and the player model is hidden.
	///  The entire thing rotates to aim.
	/// </summary>
	private Node3D _pelletGunFpsNode;

	public override void OnUseDown( PlayerController player )
	{
		if ( _isWaitingForHit ) return;
		base.OnUseDown( player );
		StartAiming();
	}

	public override void OnUseUp( PlayerController player )
	{
		if ( _isWaitingForHit ) return;
		base.OnUseUp( player );
		Fire();
		// StopAiming();
	}

	public override void OnUnequip( PlayerController player )
	{
		base.OnUnequip( player );
		StopAiming();
	}

	private void StartAiming()
	{
		_isAiming = true;

		_pelletGunFpsNode = PelletGunFpsScene.Instantiate<Node3D>();
		GetTree().CurrentScene.AddChild( _pelletGunFpsNode );
		_pelletGunFpsNode.GlobalPosition = Player.GlobalPosition + Vector3.Up * 1f;

		// _pelletGunFpsNode.RotationDegrees = Player.Model.RotationDegrees;
		_pelletGunFpsNode.GlobalRotationDegrees = new Vector3( 0, Player.Model.GlobalRotationDegrees.Y + 180f, 0 );

		Player.Model.Visible = false;

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	private void StopAiming()
	{
		GD.Print( "Stop aiming" );
		_isAiming = false;
		Player.Model.GlobalRotationDegrees = new Vector3( 0, _pelletGunFpsNode.GlobalRotationDegrees.Y + 180f, 0 );
		_pelletGunFpsNode?.QueueFree();
		Player.Model.Visible = true;

		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	internal override bool ShouldDisableMovement()
	{
		return base.ShouldDisableMovement() || _isAiming;
	}

	private void Fire()
	{
		var pellet = PelletScene.Instantiate<Pellet>();
		pellet.StartPosition = _pelletGunFpsNode.GlobalPosition;
		GetTree().CurrentScene.AddChild( pellet );

		pellet.Speed = PelletSpeed;

		var pelletDirection = _pelletGunFpsNode.Transform.Basis.Z;
		pellet.GlobalPosition = _pelletGunFpsNode.GlobalPosition;
		pellet.GlobalRotation = _pelletGunFpsNode.GlobalRotation;

		pellet.OnTimeout += () =>
		{
			_isWaitingForHit = false;
			StopAiming();
		};

		pellet.OnHit += ( hitNode ) =>
		{
			// StopAiming();
			ToSignal( GetTree().CreateTimer( 1f ), Timer.SignalName.Timeout ).OnCompleted( () =>
			{
				_isWaitingForHit = false;
				StopAiming();
			} );
		};

		_fireTimer = 0;

		_isWaitingForHit = true;

		GetNode<AudioStreamPlayer3D>( "Fire" ).Play();
	}

	private const float _aimSensitivity = 0.2f;

	public override void _UnhandledInput( InputEvent @event )
	{
		// handle aiming, rotate the fps node
		if ( @event is InputEventMouseMotion mouseMotion && _isAiming && !_isWaitingForHit )
		{
			var eyeAngles = _pelletGunFpsNode.GlobalRotationDegrees;
			eyeAngles += new Vector3( -mouseMotion.Relative.Y * _aimSensitivity, -mouseMotion.Relative.X * _aimSensitivity, 0 );
			eyeAngles.Z = 0;
			eyeAngles.X = Mathf.Clamp( eyeAngles.X, -89, 89 );
			_pelletGunFpsNode.GlobalRotationDegrees = eyeAngles;
		}

	}
}
