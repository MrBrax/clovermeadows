using System;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

[GlobalClass]
public partial class Furniture : WorldItem, IUsable
{

	public bool LightState { get; set; } = false;

	[Export] public Godot.Collections.Array<Light3D> Lights { get; set; } = new();

	[Export] public AudioStreamPlayer3D UseSoundPlayer { get; set; }

	[Export] public AnimationPlayer AnimationPlayer { get; set; }

	[Export] public Godot.Collections.Array<SittableNode> SittableNodes { get; set; } = new();
	[Export] public Godot.Collections.Array<LyingNode> LyingNodes { get; set; } = new();

	public bool IsSittable => SittableNodes.Count > 0;
	public bool IsLying => LyingNodes.Count > 0; // TODO: rename

	public override void _Ready()
	{
		base._Ready();
		SetLightState( LightState );
	}

	public void SetLightState( bool state )
	{
		LightState = state;
		foreach ( var light in Lights )
		{
			light.Visible = state;
		}
	}

	string IUsable.GetUseText()
	{
		if ( ItemData is not FurnitureData furnitureData ) return "Use";
		if ( furnitureData.CanToggleLight ) return "Toggle light";
		if ( furnitureData.CanPlayAnimation ) return "TEMP NAME FOR ANIMATION";
		if ( IsSittable ) return "Sit";
		if ( IsLying ) return "Lie";
		return "Use";
	}

	public override bool CanBePickedUp()
	{
		return !SittableNodes.Any( x => x.IsOccupied ) && !LyingNodes.Any( x => x.IsOccupied ) && base.CanBePickedUp();
	}

	public bool CanUse( PlayerController player )
	{
		if ( ItemData is not FurnitureData furnitureData ) return false;

		if ( furnitureData.CanToggleLight ) return true;
		if ( furnitureData.CanPlayAnimation ) return true;
		if ( IsSittable ) return true;
		if ( IsLying ) return true;

		return false;
	}

	public void OnUse( PlayerController player )
	{
		if ( ItemData is not FurnitureData furnitureData ) throw new Exception( "ItemData is not FurnitureData" );

		if ( furnitureData.CanToggleLight )
		{
			SetLightState( !LightState );
			UseSoundPlayer?.Play();
			return;
		}
		else if ( furnitureData.CanPlayAnimation )
		{
			AnimationPlayer.Play( furnitureData.AnimationName );
			UseSoundPlayer?.Play();
			return;
		}

		if ( IsSittable )
		{
			var sittableNode = SittableNodes.Where( x => !x.IsOccupied )
				.MinBy( x => x.GlobalPosition.DistanceTo( player.GlobalPosition ) );

			if ( !IsInstanceValid( sittableNode ) )
			{
				Logger.Info( "No sittable nodes available" );
				return;
			}

			player.Interact.Sit( sittableNode );
			return;
		}
		else if ( IsLying )
		{
			var lyingNode = LyingNodes.Where( x => !x.IsOccupied )
				.MinBy( x => x.GlobalPosition.DistanceTo( player.GlobalPosition ) );

			if ( !IsInstanceValid( lyingNode ) )
			{
				Logger.Info( "No lying nodes available" );
				return;
			}

			player.Interact.Lie( lyingNode );
			return;
		}

		throw new Exception( "Furniture was usable but no action got taken" );

	}
}
