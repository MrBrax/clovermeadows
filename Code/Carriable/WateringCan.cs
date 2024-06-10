using System;
using System.Threading.Tasks;
using vcrossing.Code.Carriable.Actions;
using vcrossing.Code.Data;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Carriable;

public partial class WateringCan : BaseCarriable
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
			PourWater();
			return;
		}

		var floorItem = worldItems.FirstOrDefault( x => x.GridPlacement == World.ItemPlacement.Floor && x.Node is IWaterable );

		if ( floorItem != null )
		{
			WaterItem( pos, floorItem );
			return;
		}

		PourWater();

	}

	private async void WaterItem( Vector2I pos, WorldNodeLink floorItem )
	{
		Logger.Info( "Watering item." );
		(floorItem.Node as IWaterable)?.OnWater( this );

		await PourWater();
		Logger.Info( "Item watered." );
	}

	private async Task PourWater()
	{
		Logger.Info( "Wasting water." );
		WaterParticles.Emitting = true;
		GetNode<AudioStreamPlayer3D>( "Watering" ).Play();
		await ToSignal( GetTree().CreateTimer( UseTime ), Timer.SignalName.Timeout );
		WaterParticles.Emitting = false;
		GetNode<AudioStreamPlayer3D>( "Watering" ).Stop();
		Logger.Info( "Water wasted." );
	}
}
