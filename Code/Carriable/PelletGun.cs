using vcrossing.Code.Interfaces;
using vcrossing.Code.Objects;
using vcrossing.Code.Player;

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
	private bool _isLookingAtWhenShot;

	private Vector3 _aimDirection;

	/// <summary>
	///  A scene that contains a camera and a gun model. When aiming, this scene is instantiated and the player model is hidden.
	///  The entire thing rotates to aim.
	/// </summary>
	private Node3D _pelletGunFpsNode;

	private const float _aimSensitivity = 0.1f;
	private const float _aimKeySensitivity = 0.3f;

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

		// _pelletGunFpsNode.GetNode<Control>( "Crosshair" ).Visible = true;

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
		if ( _pelletGunFpsNode == null || !IsInstanceValid( _pelletGunFpsNode ) ) return;
		var pellet = PelletScene.Instantiate<Pellet>();
		pellet.StartPosition = _pelletGunFpsNode.GlobalPosition;
		pellet.PelletGun = this;
		GetTree().CurrentScene.AddChild( pellet );

		pellet.Speed = PelletSpeed;

		// var pelletDirection = _pelletGunFpsNode.Transform.Basis.Z;
		pellet.GlobalPosition = _pelletGunFpsNode.GlobalPosition + -_pelletGunFpsNode.Transform.Basis.Z * 0.5f;
		pellet.GlobalRotation = _pelletGunFpsNode.GlobalRotation;

		_pelletGunFpsNode.GetNode<Control>( "Crosshair" ).Visible = false;

		pellet.OnTimeout += () =>
		{
			_isWaitingForHit = false;
			StopAiming();
		};

		pellet.OnHit += OnPelletHit;

		_fireTimer = 0;

		_isWaitingForHit = true;

		GetNode<AudioStreamPlayer3D>( "Fire" ).Play();

		_pelletGunFpsNode.GetNode<AnimationPlayer>( "AnimationPlayer" ).Play( "fire" );
	}

	private void OnPelletHit( Node3D hitNode, PelletGun pelletGun )
	{

		var shootable = hitNode.GetAncestorOfType<IShootable>();

		var stopTimer = 1f;

		/* if ( shootable != null && shootable.LookAtWhenShot )
		{
			// aim the camera at the hit node
			var camera = _pelletGunFpsNode.GetNode( "Camera3D" );
			camera.Set( "look_at_mode", 2 ); // simple look at mode
			camera.Set( "look_at_target", shootable.LookAtWhenShotTarget );
			stopTimer = shootable.LookAtWhenShotTimeout;
			Logger.Info( "PelletGun", $"Looking at {shootable.LookAtWhenShotTarget?.Name} for {stopTimer} seconds" );
			_isLookingAtWhenShot = true;
		} */

		// wait for a second before exiting aiming mode. 
		ToSignal( GetTree().CreateTimer( stopTimer ), Timer.SignalName.Timeout ).OnCompleted( () =>
		{
			if ( !IsInstanceValid( this ) ) return;
			_isWaitingForHit = false;
			_isLookingAtWhenShot = false;
			StopAiming();
		} );
	}

	/* public void SetCameraLookAt( Node3D target, float timeout = 1f )
	{
		if ( _pelletGunFpsNode == null || !IsInstanceValid( _pelletGunFpsNode ) ) return;

		var camera = _pelletGunFpsNode.GetNode( "Camera3D" );
		camera.Set( "look_at_mode", 2 ); // simple look at mode
		camera.Set( "look_at_target", target );
	} */


	public override void _UnhandledInput( InputEvent @event )
	{
		// handle aiming, rotate the fps node
		if ( @event is InputEventMouseMotion mouseMotion && _isAiming && !_isWaitingForHit && !_isLookingAtWhenShot )
		{
			var eyeAngles = _aimDirection;
			eyeAngles += new Vector3( -mouseMotion.Relative.Y * _aimSensitivity, -mouseMotion.Relative.X * _aimSensitivity, 0 );
			eyeAngles.Z = 0;
			eyeAngles.X = Mathf.Clamp( eyeAngles.X, -89, 89 );
			_aimDirection = eyeAngles;
		}

		if ( @event.IsActionPressed( "ui_cancel" ) )
		{
			StopAiming();
		}

	}

	private Vector3 _smoothAim;

	public override void _Process( double delta )
	{
		base._Process( delta );

		if ( !_isLookingAtWhenShot && _isAiming && _pelletGunFpsNode != null && IsInstanceValid( _pelletGunFpsNode ) )
		{

			// rotate the entire fps node to the aim direction
			_pelletGunFpsNode.GlobalRotationDegrees = _aimDirection;

			// smoothly rotate only the gun to the aim direction. makes it look less static
			var gun = _pelletGunFpsNode.GetNode<Node3D>( "Gun" );
			_smoothAim = _smoothAim.Slerp( _aimDirection, 15f * (float)delta );
			gun.GlobalRotationDegrees = _smoothAim;

			// handle aiming with keys
			var vec = Input.GetVector( "Left", "Right", "Up", "Down" );
			if ( vec != Vector2.Zero )
			{
				var eyeAngles = _aimDirection;
				eyeAngles += new Vector3( -vec.Y * _aimKeySensitivity, -vec.X * _aimKeySensitivity, 0 );
				eyeAngles.Z = 0;
				eyeAngles.X = Mathf.Clamp( eyeAngles.X, -89, 89 );
				_aimDirection = eyeAngles;
			}
		}
	}
}
