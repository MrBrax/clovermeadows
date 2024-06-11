using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Persistence;

[JsonDerivedType( typeof( PersistentItem ), "base" )]
[JsonDerivedType( typeof( BaseCarriable ), "carriable" )]
[JsonDerivedType( typeof( WorldItem ), "worldItem" )]
[JsonDerivedType( typeof( Plant ), "plant" )]
// [JsonPolymorphic( TypeDiscriminatorPropertyName = "$e" )]
public class PersistentItem
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

	[JsonIgnore] public virtual bool Stackable { get; set; } = false;

	[JsonIgnore] public virtual int MaxStack { get; set; } = 1;

	/// <summary>
	///  The path to the item data file. Used to load the scene and other data. Not allowed to be null.
	/// </summary>
	[JsonInclude]
	public string ItemDataPath { get; set; }

	[JsonInclude] public string ItemScenePath { get; set; }

	// TODO: does really the base class need to know about placement type?
	[JsonInclude] public World.ItemPlacementType PlacementType { get; set; }

	[JsonInclude] public Dictionary<string, Variant> CustomData { get; set; } = new();

	public PersistentItem()
	{
	}

	public PersistentItem( string itemDataPath )
	{
		ItemDataPath = itemDataPath;
	}

	public PersistentItem( ItemData itemData )
	{
		ItemDataPath = itemData.ResourcePath;
	}

	private static Type GetPersistentType( Node3D node )
	{
		if ( node is IPersistence iPersistence )
		{
			return iPersistence.PersistentType;
		}
		else if ( node is Items.WorldItem worldItem )
		{
			/*if ( !string.IsNullOrEmpty( worldItem.GetItemData().PersistentType ) )
			{
				return Type.GetType( worldItem.GetItemData().PersistentType );
			}*/

			return worldItem.PersistentType;
		}
		else if ( node is Carriable.BaseCarriable carriable )
		{
			/*if ( !string.IsNullOrEmpty( carriable.GetItemData().PersistentType ) )
			{
				return Type.GetType( carriable.GetItemData().PersistentType );
			}*/

			return carriable.PersistentType;
		}

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

		Logger.Info( "PersistentItem", $"Creating item '{nodeType}' for '{node}'" );

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

		Logger.Info( "PersistentItem", $"Creating item '{nodeType}' for '{node}'" );

		item.GetNodeData( node );

		return item;
	}

	public static PersistentItem Create( ItemData itemData )
	{

		var nodeType = Type.GetType( itemData.PersistentType );

		PersistentItem item = null;

		if ( nodeType == null )
		{
			throw new Exception( $"Type not found for {itemData}" );
		}

		item = CreateType( nodeType );

		if ( item == null )
		{
			Logger.Warn( "PersistentItem", $"Item not found for {itemData}" );
			return null;
		}

		Logger.Info( "PersistentItem", $"Creating item '{nodeType}' for '{itemData}'" );

		item.ItemDataPath = itemData.ResourcePath;

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
			// return null;
			return new PersistentItem();
		}

		Logger.Info( "PersistentItem", $"Creating derived type {derivedType}" );
		return (PersistentItem)Activator.CreateInstance( derivedType );
	}

	public virtual bool IsValid()
	{
		return true;
	}

	public virtual string GetName()
	{
		return GetItemData()?.Name ?? GetType().Name;
	}

	public virtual string GetDescription()
	{
		return GetItemData()?.Description ?? "";
	}

	public virtual string GetTooltip()
	{
		return "";
	}

	public virtual string GetImage()
	{
		// var modelHash = Crc32.FromString( GetModel() );
		// return $"/ui/thumbnails/{modelHash}.png";
		return "";
	}

	public virtual string GetIcon()
	{
		return "category";
	}

	public ItemData GetItemData()
	{
		return Loader.LoadResource<ItemData>( ItemDataPath );
	}

	public virtual T Create<T>() where T : Node3D
	{
		if ( string.IsNullOrEmpty( ItemScenePath ) )
		{
			throw new Exception( $"Item scene path not found for {ItemDataPath}" );
		}

		var node = Loader.LoadResource<PackedScene>( ItemScenePath ).Instantiate<T>();
		SetNodeData( node );
		return node;
	}

	public virtual Carriable.BaseCarriable CreateCarry()
	{
		if ( GetItemData().CarryScene == null )
		{
			throw new Exception( $"Carry scene not found for {ItemDataPath}" );
		}

		var scene = GetItemData().CarryScene.Instantiate<Carriable.BaseCarriable>();
		scene.ItemDataPath = ItemDataPath;
		scene.SceneFilePath = GetItemData().CarryScene.ResourcePath;
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
		else if ( node is IWorldItem iWorldItem )
		{
			ItemDataPath = iWorldItem.ItemDataPath;
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

		if ( node is IPersistence iPersistence )
		{
			iPersistence.SetNodeData( CustomData );
		}
	}
}
