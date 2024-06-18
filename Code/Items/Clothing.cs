using System;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Objects;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Items;

public partial class Clothing : Node3D, IWorldItem, IPersistence
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

	public Type PersistentType => typeof( ClothingItem );

	public bool ShouldBeSaved()
	{
		return !IsPlacedInEditor;
	}

	public Dictionary<string, object> GetNodeData()
	{
		return new Dictionary<string, object>();
	}

	public void SetNodeData( Dictionary<string, object> data )
	{

	}
}
