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

	// public GrowthStage Stage { get; set; } = GrowthStage.Seed;
	public GrowthStage Stage
	{
		get
		{
			if ( Growth < 20f )
			{
				return GrowthStage.Seed;
			}
			else if ( Growth < 40f )
			{
				return GrowthStage.Sprout;
			}
			else if ( Growth < 60f )
			{
				return GrowthStage.Stem;
			}
			else if ( Growth < 80f )
			{
				return GrowthStage.Budding;
			}
			else
			{
				return GrowthStage.Flowering;
			}
		}

	}

	public DateTime LastProcess;
	public float Growth { get; set; } = 0f;
	public float Wilt { get; set; } = 0f;
	public float Water { get; set; } = 0f;

	public const float GrowthPerHour = 0.1f;
	public const float WiltPerHour = 0.1f;
	public const float WaterUsedPerHour = 0.1f;

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

		if ( DateTime.Now - LastProcess > TimeSpan.FromHours( 1 ) )
		{
			SimulateHour( DateTime.Now );
			LastProcess = DateTime.Now;
		}

		if ( Model != null )
		{
			Model.Scale = new Vector3( Growth / 100f, Growth / 100f, Growth / 100f );
		}

	}



	public void SimulateHour( DateTime time )
	{
		var lastRain = GetNode<WeatherManager>( "/root/Main/WeatherManager" ).GetLastPrecipitation( time );
		var hoursSinceLastRain = (time - lastRain.Time).TotalHours;
		if ( hoursSinceLastRain <= 1 )
		{
			Water += 100f;
			LastWatered = lastRain.Time;
		}

		// growth
		if ( Growth < 100f && Wilt == 0 )
		{
			Growth += GrowthPerHour;
		}

		// wilt
		if ( Water > 0 )
		{
			Water -= WaterUsedPerHour;
		}
		else
		{
			Wilt += WiltPerHour;
		}

		Logger.Info( $"Simulated hour for plant: Growth: {Growth}, Wilt: {Wilt}, Water: {Water}" );

	}


	public void WorldLoaded()
	{
		// TODO: calculate the grow, wilt and water amount based on the last watered time

		var hoursSinceLastProcess = (DateTime.Now - LastProcess).TotalHours;

		for ( var i = 0; i < hoursSinceLastProcess; i++ )
		{
			SimulateHour( LastProcess.AddHours( i ) );
		}

		LastProcess = DateTime.Now;

	}

}
