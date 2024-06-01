using System;
using vcrossing2.Code.Dependencies;
using vcrossing2.Code.Objects;
using vcrossing2.Code.Player;
using vcrossing2.Code.WorldBuilder;

namespace vcrossing2.Code.Carriable;

public partial class FishingRod : BaseCarriable
{


	[Export, Require] public PackedScene BobberScene { get; set; }

	private FishingBobber Bobber { get; set; }

	private bool _hasCasted = false;
	private bool _isCasting = false;

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
			Logger.Warn( "FishingRod", "No water found." );
			return;
		}

		_isCasting = true;

		await ToSignal( GetTree().CreateTimer( 1f ), Timer.SignalName.Timeout );

		if ( !IsInstanceValid( Bobber ) )
		{
			Bobber = BobberScene.Instantiate<FishingBobber>();
			Bobber.Rod = this;
			Player.World.AddChild( Bobber );
			Bobber.GlobalPosition = GetCastPosition();
		}

		_isCasting = false;

		_hasCasted = true;

	}

	private bool CheckForWater( Vector3 vector3 )
	{
		return true; // TODO: check for water
	}

	private void ReelIn()
	{
		if ( IsInstanceValid( Bobber ) )
		{
			Bobber.QueueFree();
		}

		_hasCasted = false;
	}

	public void CatchFish( Fish fish )
	{
		Logger.Info( "FishingRod", "Caught fish." );
		fish.QueueFree();
	}

}
