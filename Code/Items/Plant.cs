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
			if ( Growth < 0.2f )
			{
				return GrowthStage.Seed;
			}
			else if ( Growth < 0.4f )
			{
				return GrowthStage.Sprout;
			}
			else if ( Growth < 0.6f )
			{
				return GrowthStage.Stem;
			}
			else if ( Growth < 0.8f )
			{
				return GrowthStage.Budding;
			}
			else
			{
				return GrowthStage.Flowering;
			}
		}

	}

	private DateTime Now => GetNode<TimeManager>( "/root/Main/TimeManager" ).Time;

	public DateTime LastProcess { get; set; }
	public float Growth { get; set; } = 0f;
	public float Wilt { get; set; } = 0f;
	public float Water { get; set; } = 0f;

	// flower grows fully in 3 days
	public const float GrowthPerHour = 1f / 72f;

	// wilting takes a day
	public const float WiltPerHour = 1f / 24f;

	// watered once per day
	public const float WaterUsedPerHour = 1f / 24f;

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
		Logger.Info( "Plant", "Watered plant" );
		LastWatered = Now;
		Water = 1f;
	}

	public override void _Ready()
	{
		base._Ready();
	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		if ( Now - LastProcess > TimeSpan.FromHours( 1 ) )
		{
			SimulateHour( Now );
			LastProcess = Now;
		}

		if ( Model != null )
		{
			Model.Scale = new Vector3( Growth / 1f, Growth / 1f, Growth / 1f );
		}

	}



	public void SimulateHour( DateTime time )
	{
		var lastRain = GetNode<WeatherManager>( "/root/Main/WeatherManager" ).GetLastPrecipitation( time );
		var hoursSinceLastRain = (time - lastRain.Time).TotalHours;
		if ( hoursSinceLastRain <= 1 )
		{
			Water = 1f;
			LastWatered = lastRain.Time;
		}

		// growth
		if ( Growth < 1f && Wilt == 0 )
		{
			// Growth += GrowthPerHour;
			Growth = Mathf.Clamp( Growth + GrowthPerHour, 0f, 1f );
		}

		// wilt
		if ( Water > 0 )
		{
			Water = Mathf.Clamp( Water - WaterUsedPerHour, 0f, 1f );
			Wilt = Mathf.Clamp( Wilt - WiltPerHour, 0f, 1f );
		}
		else
		{
			Wilt = Mathf.Clamp( Wilt + WiltPerHour, 0f, 1f );
		}

		Logger.Info( "Plant", $"Simulated hour for plant: Growth: {Growth}, Wilt: {Wilt}, Water: {Water}" );

	}


	public void WorldLoaded()
	{
		// TODO: calculate the grow, wilt and water amount based on the last watered time

		var hoursSinceLastProcess = (Now - LastProcess).TotalHours;

		if ( hoursSinceLastProcess > 0 )
		{
			for ( var i = 0; i < hoursSinceLastProcess; i++ )
			{
				SimulateHour( LastProcess.AddHours( i ) );
			}

			LastProcess = Now;
		}

	}

}
