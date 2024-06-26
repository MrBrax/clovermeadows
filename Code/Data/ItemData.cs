using System;
using Godot;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class ItemData : Resource
{

	[Export] public string Name;
	[Export] public string Description;
	[Export] public int Width = 1;
	[Export] public int Height = 1;
	[Export] public World.ItemPlacement Placements = World.ItemPlacement.Floor & World.ItemPlacement.Underground;

	[Export] public bool IsStackable = false;

	[Export] public bool CanDrop = true;
	[Export] public bool DisablePickup = false;
	[Export] public int StackSize = 1;

	[Export] public int BaseSellPrice = 0;
	[Export] public Godot.Collections.Array<TimeSpanData> SellDates = new();

	[Export] public PackedScene CarryScene;
	[Export] public PackedScene DropScene;
	[Export] public PackedScene PlaceScene;
	[Export] public CompressedTexture2D Icon;

	public virtual PackedScene DefaultTypeScene => PlaceScene;

	// [Export] public string PersistentType;

	public ItemData()
	{

	}

	public CompressedTexture2D GetIcon()
	{
		return Icon ?? Loader.LoadResource<CompressedTexture2D>( "res://icons/default_item.png" );
	}

	public bool IsBeingSold( DateTime date )
	{

		if ( SellDates.Count == 0 ) return true;

		foreach ( var sellDate in SellDates )
		{
			/* var startDate = new DateTime( date.Year, sellDate.MonthStart, sellDate.DayStart );
			var endDate = new DateTime( date.Year, sellDate.MonthEnd, sellDate.DayEnd );

			if ( date >= startDate && date <= endDate )
			{
				return true;
			} */

			// if the start month is greater than the end month, it means the year has changed
			if ( sellDate.MonthStart > sellDate.MonthEnd )
			{
				if ( date.Month >= sellDate.MonthStart || date.Month <= sellDate.MonthEnd )
				{
					if ( date.Month == sellDate.MonthStart && date.Day >= sellDate.DayStart )
					{
						return true;
					}
					if ( date.Month == sellDate.MonthEnd && date.Day <= sellDate.DayEnd )
					{
						return true;
					}
				}
			}
			else
			{
				if ( date.Month >= sellDate.MonthStart && date.Month <= sellDate.MonthEnd )
				{
					if ( date.Month == sellDate.MonthStart && date.Day >= sellDate.DayStart )
					{
						return true;
					}
					if ( date.Month == sellDate.MonthEnd && date.Day <= sellDate.DayEnd )
					{
						return true;
					}
				}
			}

		}
		return false;
	}

}
