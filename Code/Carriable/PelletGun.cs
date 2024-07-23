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

	private Vector3 _aimDirection;

	/// <summary>
	///  A scene that contains a camera and a gun model. When aiming, this scene is instantiated and the player model is hidden.
	///  The entire thing rotates to aim.
	/// </summary>
	private Node3D _pelletGunFpsNode;

	private const float _aimSensitivity = 0.1f;

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

		pellet.OnHit += ( hitNode, pelletGun ) =>
		{
			// StopAiming();
			ToSignal( GetTree().CreateTimer( 1f ), Timer.SignalName.Timeout ).OnCompleted( () =>
			{
				if ( !IsInstanceValid( this ) ) return;
				_isWaitingForHit = false;
				StopAiming();
			} );
		};

		_fireTimer = 0;

		_isWaitingForHit = true;

		GetNode<AudioStreamPlayer3D>( "Fire" ).Play();

		_pelletGunFpsNode.GetNode<AnimationPlayer>( "AnimationPlayer" ).Play( "fire" );
	}



	public override void _UnhandledInput( InputEvent @event )
	{
		// handle aiming, rotate the fps node
		if ( @event is InputEventMouseMotion mouseMotion && _isAiming && !_isWaitingForHit )
		{
			var eyeAngles = _aimDirection;
			eyeAngles += new Vector3( -mouseMotion.Relative.Y * _aimSensitivity, -mouseMotion.Relative.X * _aimSensitivity, 0 );
			eyeAngles.Z = 0;
			eyeAngles.X = Mathf.Clamp( eyeAngles.X, -89, 89 );
			_aimDirection = eyeAngles;
		}

	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		if ( _isAiming )
		{
			_pelletGunFpsNode.GlobalRotationDegrees = _aimDirection;
			// _pelletGunFpsNode.GlobalRotationDegrees = _pelletGunFpsNode.GlobalRotationDegrees.Lerp( _aimDirection, 2.8f * (float)delta );
			// _pelletGunFpsNode.GlobalRotationDegrees = _pelletGunFpsNode.GlobalRotationDegrees.Slerp( _aimDirection, 2.8f * (float)delta );
		}
	}
}
