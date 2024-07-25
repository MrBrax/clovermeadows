using System;
using Godot;
using vcrossing.Code.Persistence;
using static vcrossing.Code.World;

namespace vcrossing.Code.Data;

[GlobalClass]
[Icon( "res://icons/editor/itemdata.png" )]
public partial class ItemData : Resource
{

	[Export] public string Id { get; set; } = Guid.NewGuid().ToString();

	[Export] public string Name;


	[Export( PropertyHint.MultilineText )]
	public string Description;

	[Export] public int Width = 1;
	[Export] public int Height = 1;
	[Export] public World.ItemPlacement Placements = World.ItemPlacement.Floor & World.ItemPlacement.Underground;


	[Export] public bool CanDrop = true;
	[Export] public bool DisablePickup = false;
	[Export] public bool IsStackable = false;
	[Export] public int StackSize = 1;

	[Export] public int BaseSellPrice = 100;
	[Export] public int BaseBuyPrice = 100;
	[Export] public Godot.Collections.Array<TimeSpanData> SellDates = new();

	/// <summary>
	///  The scene used for previews or icons. If this is not set, it will try to extract the model from <see cref="PlaceScene"/> by the <see cref="BaseItem.Model"/> property.
	/// </summary>
	[Export] public PackedScene ModelScene;

	/// <summary>
	///  The scene used when the item is carried. Usually represented by the full version of the item.
	///  TODO: move this to a Carriable class
	/// </summary>
	[Export] public PackedScene CarryScene;

	/// <summary>
	///  The scene used when the item is dropped. Usually represented by a cardboard box or a simple version of the item.
	/// </summary>
	[Export] public PackedScene DropScene;

	/// <summary>
	///  The scene used when the item is placed. Usually represented by the full version of the item. Is also used as a fallback for other things.
	/// </summary>
	[Export] public PackedScene PlaceScene;
	[Export] public CompressedTexture2D Icon;

	public virtual PackedScene DefaultTypeScene => PlaceScene;

	// [Export] public string PersistentType;

	public static T GetById<T>( string id ) where T : ItemData
	{
		return ResourceManager.LoadItemFromId<T>( id );
	}

	public static ItemData GetById( string id )
	{
		return ResourceManager.LoadItemFromId<ItemData>( id );
	}

	public ItemData()
	{

	}

	public virtual CompressedTexture2D GetIcon()
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

	public virtual T CreateItem<T>() where T : PersistentItem
	{
		return PersistentItem.Create<T>( this ); // TODO: manually create the item
	}

	public virtual PersistentItem CreateItem()
	{
		return PersistentItem.Create( this ); // TODO: manually create the item
	}

	public Node3D CreateModelObject()
	{

		Node3D itemInstance;

		if ( ModelScene != null )
		{
			itemInstance = ModelScene.Instantiate<Node3D>();
		}
		else if ( PlaceScene != null )
		{
			itemInstance = PlaceScene.Instantiate<Node3D>();
		}
		else if ( DropScene != null )
		{
			itemInstance = DropScene.Instantiate<Node3D>();
		}
		else
		{
			itemInstance = DefaultTypeScene.Instantiate<Node3D>();
		}

		if ( itemInstance == null ) throw new Exception( $"Failed to instantiate item: {ResourcePath}" );

		if ( itemInstance is BaseItem baseItem )
		{
			var model = baseItem.Model;
			if ( model != null )
			{
				model.Owner = null;
				model.GetParent().RemoveChild( model );
				itemInstance.QueueFree();
				return model;
			}
			else
			{
				Logger.Warn( $"ShopDisplay", $"Item {Name} does not have a model" );
			}
		}
		else if ( itemInstance is Carriable.BaseCarriable baseCarriable )
		{
			var model = baseCarriable.Model;
			if ( model != null )
			{
				model.Owner = null;
				model.GetParent().RemoveChild( model );
				itemInstance.QueueFree();
				return model;
			}
			else
			{
				Logger.Warn( $"ShopDisplay", $"Item {Name} does not have a model" );
			}
		}

		Logger.Warn( $"ShopDisplay", $"Item {Name} does not have a model" );
		itemInstance.QueueFree();

		return null;

	}

	/// <summary>
	///   Returns a list of grid positions for this item.
	/// </summary>
	/// <param name="itemRotation">Amount of rotation to apply to the item.</param>
	/// <param name="originOffset">Offset to apply to the item.</param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public List<Vector2I> GetGridPositions( World.ItemRotation itemRotation, Vector2I originOffset = default )
	{
		var positions = new List<Vector2I>();
		if ( Width == 0 || Height == 0 ) throw new Exception( "Item has no size" );

		if ( Width == 1 && Height == 1 )
		{
			return [originOffset]; // if the item is 1x1, return the origin since it's the only position
		}

		if ( itemRotation == ItemRotation.North )
		{
			for ( var x = 0; x < Width; x++ )
			{
				for ( var y = 0; y < Height; y++ )
				{
					positions.Add( new Vector2I( originOffset.X + x, originOffset.Y + y ) );
				}
			}
		}
		else if ( itemRotation == ItemRotation.South )
		{
			for ( var x = 0; x < Width; x++ )
			{
				for ( var y = 0; y < Height; y++ )
				{
					positions.Add( new Vector2I( originOffset.X + x, originOffset.Y - y ) );
				}
			}
		}
		else if ( itemRotation == ItemRotation.East )
		{
			for ( var x = 0; x < Height; x++ )
			{
				for ( var y = 0; y < Width; y++ )
				{
					positions.Add( new Vector2I( originOffset.X + x, originOffset.Y + y ) );
				}
			}
		}
		else if ( itemRotation == ItemRotation.West )
		{
			for ( var x = 0; x < Height; x++ )
			{
				for ( var y = 0; y < Width; y++ )
				{
					positions.Add( new Vector2I( originOffset.X - x, originOffset.Y + y ) );
				}
			}
		}

		return positions;
	}

	public override string ToString()
	{
		return $"ItemData:{Name}:{Id}";
	}

}
