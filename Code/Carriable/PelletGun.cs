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
	[Export] public Node3D PelletExit;

	private float _fireTimer;

	private bool _isAiming;

	public override void OnUseDown( PlayerController player )
	{
		base.OnUseDown( player );
		StartAiming();
	}

	public override void OnUseUp( PlayerController player )
	{
		base.OnUseUp( player );
		Fire();
		StopAiming();
	}

	public override void OnUnequip( PlayerController player )
	{
		base.OnUnequip( player );
		StopAiming();
	}

	private void StartAiming()
	{
		_isAiming = true;
	}

	private void StopAiming()
	{
		_isAiming = false;
	}

	internal override bool ShouldDisableMovement()
	{
		return base.ShouldDisableMovement() || _isAiming;
	}

	private void Fire()
	{
		var pellet = PelletScene.Instantiate<Pellet>();
		pellet.Direction = PelletExit.GlobalTransform.Basis.Z.Normalized();
		pellet.Speed = PelletSpeed;
		pellet.GlobalTransform = PelletExit.GlobalTransform;
		GetTree().CurrentScene.AddChild( pellet );

		GetNode<AudioStreamPlayer3D>( "Fire" ).Play();
	}
}
