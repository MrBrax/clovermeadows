using System;
using vcrossing.Code.Carriable;
using vcrossing.Code.Carriable.Actions;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;

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

	public DateTime LastWatered { get; set; }

	public GrowthStage Stage { get; set; } = GrowthStage.Seed;

	public float Growth { get; set; } = 0f;
	public float Wilt { get; set; } = 0f;
	public float Water { get; set; } = 0f;

	public const float GrowthPerHour = 0.1f;
	public const float WiltPerHour = 0.1f;
	public const float WaterPerHour = 0.1f;

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
		Water = 100f;
	}

	public override void _Ready()
	{
		base._Ready();
	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		var currentTime = DateTime.Now;
		var lastWorldSave = GetNode<WorldManager>( "/root/WorldManager" ).ActiveWorld.SaveData.LastSave;
		var lastRain = GetNode<WeatherManager>( "/root/WeatherManager" ).GetLastPrecipitation( currentTime );

	}


	public void WorldLoaded()
	{
		// TODO: calculate the grow, wilt and water amount based on the last watered time
	}

}
