using System;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Carriable;

public partial class Net : BaseCarriable
{

	private AnimationPlayer _animationPlayer => GetNode<AnimationPlayer>( "AnimationPlayer" );

	private bool _isSwinging = false;

	private bool _isReady;

	public override void OnEquip( PlayerController player )
	{
		base.OnEquip( player );
		_isReady = false;
	}

	internal override bool ShouldDisableMovement()
	{
		return base.ShouldDisableMovement() || _isSwinging;
	}

	public override float CustomPlayerSpeed()
	{
		return _isReady ? 0.2f : 1;
	}

	public override bool CanUse()
	{
		return base.CanUse() && !_isSwinging;
	}


	public override void OnUseDown( PlayerController player )
	{
		if ( !CanUse() ) return;
		_isReady = true;
		_animationPlayer.Play( "ready" );
	}

	public override void OnUseUp( PlayerController player )
	{
		Swing( player );
		_isReady = false;
	}

	private async void Swing( PlayerController player )
	{
		Logger.Info( "Swing" );
		_animationPlayer.Play( "swing" );
		_isSwinging = true;

		// GetNode<AudioStreamPlayer3D>( "Swing" ).Play();

		await ToSignal( _animationPlayer, AnimationPlayer.SignalName.AnimationFinished );

		_animationPlayer.Play( "return" );

		FindTarget( player );

		await ToSignal( _animationPlayer, AnimationPlayer.SignalName.AnimationFinished );

		_isSwinging = false;

		_animationPlayer.Play( "RESET" );

		// GetNode<AudioStreamPlayer3D>( "Hit" ).Play();
	}

	private void FindTarget( PlayerController player )
	{

		var box = player.Interact.NetBox;

		var bodies = box.GetOverlappingBodies();

		foreach ( var body in bodies )
		{
			// Logger.Info( "Net", $"Found body: {body}" );
			if ( body is INettable nettable )
			{
				nettable.OnNetted( this );
			}
		}

		if ( bodies.Count() == 0 ) Logger.Info( "Net", "No bodies found" );

	}
}
