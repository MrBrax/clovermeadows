using System;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Objects;

namespace vcrossing.Code.Items;

public partial class Clothing : Node3D, IWorldItem
{

	public bool IsPlacedInEditor { get; set; }
	public World.ItemPlacement Placement { get; set; }
	[Export] public string ItemDataPath { get; set; }

	private ClothingData _itemData;
	public ClothingData ItemData
	{
		get
		{
			if ( _itemData == null )
			{
				if ( string.IsNullOrEmpty( ItemDataPath ) ) throw new Exception( "ItemDataPath is null" );
				_itemData = Loader.LoadResource<ClothingData>( ItemDataPath );
				if ( _itemData == null ) throw new Exception( $"Failed to load item data from {ItemDataPath}" );
			}
			return _itemData;
		}
		set => _itemData = value;
	}

	public bool ShouldBeSaved()
	{
		return !IsPlacedInEditor;
	}
}
