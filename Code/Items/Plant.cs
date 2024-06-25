using System;
using System.Threading.Tasks;
using Godot.Collections;
using vcrossing.Code.Carriable;
using vcrossing.Code.Carriable.Actions;
using vcrossing.Code.Data;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public partial class Plant : WorldItem, IUsable, IWaterable, IWorldLoaded
{

	public enum GrowthStage
	{
		Seed = 0,
		Sprout = 1,
		Stem = 2,
		Budding = 3,
		Flowering = 4,
	}

	[Export] public Node3D SeedHole { get; set; } // TODO: better name

	// DateTime Placed 

	// time since last watered
	public DateTime LastWatered { get; set; }

	public GrowthStage Stage { get; set; } = GrowthStage.Seed;

	public float Growth { get; set; } = 0f;
	public float Wilt { get; set; } = 0f;

	// private const float WiltSpeed = 10f;
	// private const float WaterUseSpeed = 10f;
	// private const float GrowSpeed = 10f;

	public override Type PersistentType => typeof( Persistence.Plant );

	public bool CanUse( PlayerController player )
	{
		return true;
	}

	public void OnUse( PlayerController player )
	{
		throw new NotImplementedException();
	}

	public void OnWater( WateringCan wateringCan )
	{
		Logger.Info( "Watered plant" );
		LastWatered = DateTime.Now;
		// WaterAmount = Math.Min( 100, WaterAmount + wateringCan.WaterAmount );
		// WaterAmount = 100f;
		// WiltAmount = 0f;
	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		Render();


	}

	private void Render()
	{

		if ( Model == null ) return;

		var model = GetNode<MeshInstance3D>( Model );

		if ( model == null ) return;




	}

	public void WorldLoaded()
	{
		// TODO: calculate the grow, wilt and water amount based on the last watered time
	}

}
