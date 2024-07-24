using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using vcrossing.Code.Data;
using vcrossing.Code.Interfaces;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Persistence;

[JsonDerivedType( typeof( Persistence.PersistentItem ), "base" )]
[JsonDerivedType( typeof( Persistence.BaseCarriable ), "carriable" )]
[JsonDerivedType( typeof( Persistence.WorldItem ), "worldItem" )]
[JsonDerivedType( typeof( Persistence.Plant ), "plant" )]
[JsonDerivedType( typeof( Persistence.ClothingItem ), "clothing" )]
// [JsonDerivedType( typeof( Persistence.Tool ), "tool" )]
[JsonDerivedType( typeof( Persistence.Tree ), "tree" )]
[JsonDerivedType( typeof( Persistence.Furniture ), "furniture" )]
[JsonDerivedType( typeof( Persistence.Seed ), "seed" )]
[JsonDerivedType( typeof( Persistence.FloorDecal ), "floorDecal" )]
[JsonDerivedType( typeof( Persistence.Fruit ), "fruit" )]
// [JsonPolymorphic( TypeDiscriminatorPropertyName = "$e" )]
public partial class PersistentItem
{
	public enum ItemSpawnType
	{
		Dropped,
		Placed,
		Carry
	}

	/// <summary>
	///   The type of the item, used for serialization. Holds the name of the class.
	/// </summary>
	[JsonInclude]
	public string NodeType { get; set; }

	// TODO: should this just copy from ItemData?
	[JsonIgnore] public virtual bool Stackable { get; set; } = false;

	[JsonIgnore] public virtual int MaxStack { get; set; } = 1;

	/// <summary>
	///  The path to the item data file. Used to load the scene and other data. Not allowed to be null.
	/// </summary>
	[JsonInclude, Obsolete( "Use ItemDataId instead." )]
	public string ItemDataPath { get; set; }

	/// <summary>
	/// The ID of the item data file. Used to load the scene and other data. Not allowed to be null.
	/// </summary>
	[JsonInclude]
	public string ItemDataId { get; set; }

	[JsonInclude] public string ItemScenePath { get; set; }

	// TODO: does really the base class need to know about placement type?
	[JsonInclude] public World.ItemPlacementType PlacementType { get; set; }

	// [JsonInclude] public Godot.Collections.Dictionary<string, Variant> CustomData { get; set; } = new();
	[JsonInclude] public Dictionary<string, object> CustomData { get; set; } = new();


	private ItemData _itemData;
	[JsonIgnore]
	public virtual ItemData ItemData
	{
		get
		{
			if ( _itemData == null )
			{
				LoadItemData();
			}
			return _itemData;
		}
		set => _itemData = value;
	}

	/* public PersistentItem()
	{
	} */

	/* public PersistentItem( string itemDataPath )
	{
		ItemDataPath = itemDataPath;
		// LoadItemData();
	} */

	/* public PersistentItem( ItemData itemData )
	{
		ItemDataPath = itemData.ResourcePath;
		// LoadItemData();
	} */

	private void LoadItemData()
	{

		if ( !string.IsNullOrWhiteSpace( ItemDataId ) )
		{
			_itemData = ResourceManager.Instance.LoadItemFromId<ItemData>( ItemDataId );
			if ( _itemData != null && _itemData.IsStackable ) Stackable = true;
			if ( _itemData != null && _itemData.StackSize > 0 ) MaxStack = _itemData.StackSize;
			return;
		}

		if ( string.IsNullOrWhiteSpace( ItemDataPath ) )
		{
			throw new Exception( "Item data path not set" );
		}

		_itemData = Loader.LoadResource<ItemData>( ItemDataPath );

		if ( _itemData != null && _itemData.IsStackable ) Stackable = true;
		if ( _itemData != null && _itemData.StackSize > 0 ) MaxStack = _itemData.StackSize;
	}

	public T GetItemData<T>() where T : ItemData
	{
		return (T)ItemData;
	}

	private static Type GetPersistentType( Node3D node )
	{
		if ( node is IPersistence iPersistence )
		{
			return Type.GetType( $"vcrossing.Code.Persistence.{iPersistence.PersistentItemType}" );
		}

		Logger.Warn( "PersistentItem", $"PersistentItemType not found for {node}" );

		return node.GetType();
	}

	public static PersistentItem Create( WorldNodeLink node )
	{
		var nodeType = GetPersistentType( node.Node );

		PersistentItem item = null;

		// var type = Type.GetType( nodeTypeString );

		if ( nodeType == null )
		{
			throw new Exception( $"Type not found for {node}" );
		}

		item = CreateType( nodeType );

		if ( item == null )
		{
			Logger.Warn( $"Item not found for {node}" );
			return null;
		}

		Logger.Debug( "PersistentItem", $"Creating item '{nodeType}' for '{node}'" );

		item.GetLinkData( node );

		return item;
	}

	public static PersistentItem Create( Node3D node )
	{
		var nodeType = GetPersistentType( node );

		PersistentItem item = null;

		// var type = Type.GetType( nodeTypeString );

		if ( nodeType == null )
		{
			throw new Exception( $"Type not found for {node}" );
		}

		item = CreateType( nodeType );

		if ( item == null )
		{
			Logger.Warn( "PersistentItem", $"Item not found for {node}" );
			return null;
		}

		Logger.Debug( "PersistentItem", $"Creating item '{nodeType}' for '{node}'" );

		item.GetNodeData( node );

		return item;
	}

	/// <summary>
	///  Get the PersistentItemType from the scene properties or script. Returns null if not found.
	/// </summary>
	/// <param name="scene"></param>
	/// <returns></returns>
	public static string GetScenePersistentItemType( PackedScene scene )
	{
		var state = scene.GetState();

		// first loop through properties for the PersistentItemType
		for ( var i = 0; i < state.GetNodePropertyCount( 0 ); i++ )
		{
			var propertyName = state.GetNodePropertyName( 0, i );
			if ( propertyName == "PersistentItemType" )
			{
				var propertyNameString = state.GetNodePropertyValue( 0, i ).ToString();
				if ( !string.IsNullOrWhiteSpace( propertyNameString ) ) return propertyNameString;
			}
		}

		// if not found, check the script for the PersistentItemType
		for ( var i = 0; i < state.GetNodePropertyCount( 0 ); i++ )
		{
			var propertyName = state.GetNodePropertyName( 0, i );
			if ( propertyName == "script" )
			{
				var script = state.GetNodePropertyValue( 0, i ).As<CSharpScript>();
				if ( script != null )
				{
					var defaultValue = script.GetPropertyDefaultValue( "PersistentItemType" ).AsString();
					if ( !string.IsNullOrWhiteSpace( defaultValue ) ) return defaultValue;
				}
			}
		}

		Logger.Warn( "PersistentItem.GSPIT", $"PersistentItemType not found for {scene.ResourcePath}" );

		return null;
	}

	/// <summary>
	///  Create a PersistentItem DTO from an ItemData object. This is used to create internal items from the item data.
	/// </summary>
	/// <param name="itemData"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static PersistentItem Create( ItemData itemData )
	{

		// var typeName = !string.IsNullOrEmpty( itemData.PersistentType ) ? itemData.PersistentType : "PersistentItem";

		string typeName = "";

		// first, check if the PlaceScene has a PersistentItemType property
		if ( itemData.PlaceScene != null )
		{
			typeName = GetScenePersistentItemType( itemData.PlaceScene );
		}
		else if ( itemData.DropScene != null )
		{
			typeName = GetScenePersistentItemType( itemData.DropScene );
		}
		else if ( itemData.CarryScene != null )
		{
			typeName = GetScenePersistentItemType( itemData.CarryScene );
		}
		else if ( itemData.DefaultTypeScene != null )
		{
			typeName = GetScenePersistentItemType( itemData.DefaultTypeScene );
		}
		else
		{
			Logger.Warn( "PersistentItem", $"No scene found for {itemData.ResourcePath}" );
		}

		if ( string.IsNullOrWhiteSpace( typeName ) )
		{
			typeName = "PersistentItem";
			Logger.Warn( "PersistentItem", $"PersistentItemType not found for {itemData.ResourcePath}" );
		}

		var nodeType = Type.GetType( $"vcrossing.Code.Persistence.{typeName}" );

		PersistentItem item = null;

		if ( nodeType == null )
		{
			throw new Exception( $"Type {typeName} not found for {itemData.ResourcePath}" );
		}

		item = CreateType( nodeType );

		if ( item == null )
		{
			Logger.Warn( "PersistentItem", $"Item not found for {itemData}" );
			return null;
		}

		Logger.Debug( "PersistentItem", $"Creating item '{nodeType}' for '{itemData}'" );

		item.ItemDataPath = itemData.ResourcePath;
		item.ItemDataId = itemData.Id;

		return item;
	}

	private static PersistentItem CreateType( Type type )
	{
		Type baseType = typeof( PersistentItem );

		// find the first type that extends the base type with the name of 'type', if none is found return base type
		Type derivedType = baseType.Assembly.GetTypes()
			.FirstOrDefault( t => t.IsSubclassOf( baseType ) && t.Name == type.Name );

		if ( derivedType == null )
		{
			Logger.Info( "PersistentItem", $"Derived type not found for {type}, using default PersistentItem" );
			return new PersistentItem();
		}

		Logger.Debug( "PersistentItem", $"Creating derived type {derivedType}" );
		return (PersistentItem)Activator.CreateInstance( derivedType );
	}

	public virtual bool IsValid()
	{
		return true;
	}

	public virtual string GetName()
	{
		return ItemData?.Name ?? GetType().Name;
	}

	public virtual string GetDescription()
	{
		return ItemData?.Description ?? "";
	}

	public virtual string GetTooltip()
	{
		var tooltipText = GetName();
		if ( !string.IsNullOrWhiteSpace( GetDescription() ) )
		{
			tooltipText += $"\n{GetDescription()}";
		}
		return tooltipText + $"\n\nPath: {ItemDataPath}\nId: {ItemDataId}\nType: {GetType().Name}\nItemData Type: {ItemData?.GetType().Name}";
	}

	// public virtual string GetImage()
	// {
	// 	// var modelHash = Crc32.FromString( GetModel() );
	// 	// return $"/ui/thumbnails/{modelHash}.png";
	// 	return "";
	// }

	private static Texture2D DefaultIcon => Loader.LoadResource<CompressedTexture2D>( "res://icons/default_item.png" );

	public virtual string GetIcon()
	{
		return ItemData?.Icon?.ResourcePath ?? DefaultIcon.ResourcePath;
	}

	public virtual Texture2D GetIconTexture()
	{
		return ItemData?.Icon ?? DefaultIcon;
	}

	/* [Obsolete]
	public ItemData GetItemData()
	{
		return Loader.LoadResource<ItemData>( ItemDataPath );
	} */

	public virtual T Create<T>() where T : Node3D
	{
		if ( string.IsNullOrWhiteSpace( ItemScenePath ) )
		{
			throw new Exception( $"Item scene path not found for {ItemDataPath}" );
		}

		var node = Loader.LoadResource<PackedScene>( ItemScenePath ).Instantiate<T>();
		SetNodeData( node );
		return node;
	}

	public virtual Node3D Create()
	{
		if ( ItemData.PlaceScene == null )
		{
			throw new Exception( $"Place scene not found for {ItemDataPath}" );
		}

		var scene = ItemData.PlaceScene.Instantiate<Node3D>();
		scene.SceneFilePath = ItemData.PlaceScene.ResourcePath;
		SetNodeData( scene );
		return scene;
	}

	public virtual void GetLinkData( WorldNodeLink nodeLink )
	{
		// ItemType = GetPersistentType( entity ).Name;
		GetNodeData( nodeLink.Node );
	}

	public virtual void GetNodeData( Node3D node )
	{
		if ( node is Items.WorldItem worldItem )
		{
			ItemDataPath = worldItem.ItemDataPath;
			PlacementType = worldItem.PlacementType;
		}
		else if ( node is Carriable.BaseCarriable carriable )
		{
			ItemDataPath = carriable.ItemDataPath;
			PlacementType = World.ItemPlacementType.Dropped;
		}

		if ( node is IDataPath dataPath )
		{
			ItemDataPath = dataPath.ItemDataPath;
			ItemDataId = dataPath.ItemDataId;

			// TODO: uncomment
			// if ( string.IsNullOrEmpty( ItemDataId ) )
			// {
			Logger.Warn( $"Item data id not found for {node}" );
			ItemDataId = ItemData.Id;
			// }
		}
		else
		{
			Logger.Warn( $"Item data path not found for {node} (unsupported type {node.GetType()})" );
		}

		if ( node is IPersistence iPersistence )
		{
			CustomData = iPersistence.GetNodeData();
		}

		ItemScenePath = node.SceneFilePath;

		NodeType = node.GetType().Name;
	}

	public virtual void SetNodeData( Node3D node )
	{
		if ( node is Items.WorldItem worldItem )
		{
			Logger.Info( "PersistentItem", $"Load worldItem PersistentItem {ItemDataPath}" );
			worldItem.ItemDataPath = ItemDataPath;
			worldItem.PlacementType = PlacementType;
		}
		else if ( node is Carriable.BaseCarriable carriable )
		{
			Logger.Info( "PersistentItem", $"Load carriable PersistentItem {ItemDataPath}" );
			carriable.ItemDataPath = ItemDataPath;
		}
		else
		{
			Logger.Warn( $"Item data path not found for {node} (unsupported type {node.GetType()})" );
		}

		if ( node is IDataPath dataPath )
		{
			dataPath.ItemDataPath = ItemDataPath;
			dataPath.ItemDataId = ItemDataId;
		}

		if ( node is IPersistence iPersistence )
		{
			iPersistence.SetNodeData( this, CustomData );
		}
	}


	public bool TryGetCustomProperty<T>( string key, out T value )
	{
		value = default;

		if ( CustomData.TryGetValue( key, out var obj ) )
		{
			if ( obj is T t )
			{
				value = t;
				return true;
			}
			else if ( obj is JsonElement jsonElement )
			{
				value = jsonElement.Deserialize<T>();
				return true;
			}
		}

		return false;
	}

	/// <summary>
	///		 Returns true if this item can be merged with the other item. Throws an exception if it can't.
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public virtual bool CanMergeWith( PersistentItem other )
	{
		return true;
	}

	public virtual void MergeWith( PersistentItem other )
	{
		return;
	}

	/// <summary>
	///   Called when the item is created for the first time. Set things like durability and other properties here that are otherwise not set on the item.
	/// </summary>
	public virtual void Initialize()
	{

	}

	public PersistentItem Clone()
	{
		// TODO: DON'T DO THIS KIDS
		return JsonSerializer.Deserialize<PersistentItem>( JsonSerializer.Serialize( this, MainGame.JsonOptions ), MainGame.JsonOptions );
	}
}
