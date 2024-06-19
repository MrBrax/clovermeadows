using System;
using vcrossing.Code.Data;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Carriable;

public partial class Net : BaseCarriable
{

	private AnimationPlayer _animationPlayer => GetNode<AnimationPlayer>( "AnimationPlayer" );

	private bool _isSwinging = false;

	internal override bool ShouldDisableMovement()
	{
		return base.ShouldDisableMovement() || _isSwinging;
	}

	public override void OnUse( PlayerController player )
	{
		if ( !CanUse() ) return;

		_timeUntilUse = UseTime;

		Swing( player );
	}

	private async void Swing( PlayerController player )
	{
		Logger.Info( "Swing" );
		_animationPlayer.Play( "swing" );
		_isSwinging = true;

		// GetNode<AudioStreamPlayer3D>( "Swing" ).Play();

		await ToSignal( _animationPlayer, AnimationPlayer.SignalName.AnimationFinished );

		_animationPlayer.Play( "return" );

		await ToSignal( _animationPlayer, AnimationPlayer.SignalName.AnimationFinished );

		_isSwinging = false;

		_animationPlayer.Play( "RESET" );

		// GetNode<AudioStreamPlayer3D>( "Hit" ).Play();
	}
}
