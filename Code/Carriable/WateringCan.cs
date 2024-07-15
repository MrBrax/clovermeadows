using System;
using System.Threading.Tasks;
using vcrossing.Code.Carriable.Actions;
using vcrossing.Code.Data;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Carriable;

public sealed partial class WateringCan : BaseCarriable
{

	[Export] public GpuParticles3D WaterParticles { get; set; }

	// private bool _isWatering = false;


	public override void _Ready()
	{
		base._Ready();
		WaterParticles.Emitting = false;
	}

	internal override bool ShouldDisableMovement()
	{
		return WaterParticles.Emitting;
	}

	public override void OnUse( PlayerController player )
	{
		if ( !CanUse() )
		{
			return;
		}

		base.OnUse( player );
		_timeUntilUse = UseTime;

		var pos = player.Interact.GetAimingGridPosition();

		var worldItems = player.World.GetItems( pos ).ToList();

		if ( worldItems.Count == 0 )
		{
			PourWaterAsync();
			return;
		}

		var floorItem = worldItems.FirstOrDefault( x => x.GridPlacement == World.ItemPlacement.Floor && x.Node is IWaterable );

		if ( floorItem != null )
		{
			WaterItem( pos, floorItem );
			return;
		}

		PourWaterAsync();

	}

	private async void WaterItem( Vector2I pos, WorldNodeLink floorItem )
	{
		Logger.Info( "Watering item." );
		(floorItem.Node as IWaterable)?.OnWater( this );

		await PourWaterAsync();
		Logger.Info( "Item watered." );
	}

	public void StartEmitting()
	{
		WaterParticles.Emitting = true;
	}

	public void StopEmitting()
	{
		WaterParticles.Emitting = false;
	}

	private async Task PourWaterAsync()
	{
		Logger.Info( "Wasting water." );
		// WaterParticles.Emitting = true;
		GetNode<AnimationPlayer>( "AnimationPlayer" ).Play( "watering" );
		GetNode<AudioStreamPlayer3D>( "Watering" ).Play();
		await ToSignal( GetTree().CreateTimer( UseTime ), Timer.SignalName.Timeout );
		// WaterParticles.Emitting = false;
		GetNode<AudioStreamPlayer3D>( "Watering" ).Stop();
		Logger.Info( "Water wasted." );
	}
}
