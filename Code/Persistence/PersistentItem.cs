using System;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code.Carriable;
using vcrossing2.Code.Items;

namespace vcrossing2.Code.Persistence;

[JsonDerivedType( typeof(PersistentItem), "base" )]
[JsonDerivedType( typeof(BaseCarriable), "carriable" )]
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
	public string ItemType { get; set; }

	[JsonIgnore] public virtual bool Stackable { get; set; } = false;
	[JsonIgnore] public virtual int MaxStack { get; set; } = 1;

	/// <summary>
	///  The path to the item data file. Used to load the scene and other data. Not allowed to be null.
	/// </summary>
	[JsonInclude]
	public string ItemDataPath { get; set; }

	// TODO: does really the base class need to know about placement type?
	[JsonInclude] public World.ItemPlacementType PlacementType { get; set; }

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

	private static Type GetNodeType( Node3D node )
	{
		return node.GetType();
	}

	public static PersistentItem Create( Node3D node )
	{
		var nodeType = GetNodeType( node );

		PersistentItem item = null;

		// var type = Type.GetType( nodeTypeString );

		if ( nodeType == null )
		{
			throw new Exception( $"Type not found for {node}" );
		}

		// item = (PersistentItem)Activator.CreateInstance( nodeType );
		item = CreateType( nodeType );

		if ( item == null )
		{
			GD.PushWarning( $"Item not found for {node}" );
			return null;
		}

		item.GetData( node );

		return item;
	}

	private static PersistentItem CreateType( Type type )
	{
		Type baseType = typeof(PersistentItem);

		// find the first type that extends the base type with the name of 'type', if none is found return base type
		Type derivedType = baseType.Assembly.GetTypes()
			.FirstOrDefault( t => t.IsSubclassOf( baseType ) && t.Name == type.Name );

		if ( derivedType == null )
		{
			GD.Print( $"Derived type not found for {type}, using default PersistentItem" );
			// return null;
			return new PersistentItem();
		}

		GD.Print( $"Creating derived type {derivedType}" );
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
		return GD.Load<ItemData>( ItemDataPath );
	}

	/*public virtual Node3D Spawn<T>( ItemSpawnType spawnType ) where T : Node3D
	{
		T scene;
		switch ( spawnType )
		{
			case ItemSpawnType.Dropped:
				scene = GetItemData().DropScene.Instantiate<T>();
			case ItemSpawnType.Placed:
				scene = GetItemData().PlaceScene.Instantiate<T>();
			case ItemSpawnType.Carry:
				scene = GetItemData().CarryScene.Instantiate<T>();
			default:
				return null;
		}
	}*/

	public virtual WorldItem CreateAuto()
	{
		/*switch ( PlacementType )
		{
			case World.ItemPlacementType.Dropped:
				return CreateDropped();
			case World.ItemPlacementType.Placed:
				return CreatePlaced();
			/*case World.ItemPlacementType.Carry:
				return CreateCarry();#1#
			default:
				return null;
		}*/
		
		var itemData = GetItemData();

		if ( PlacementType == World.ItemPlacementType.Placed && itemData.PlaceScene != null )
		{
			return CreatePlaced();
		}
		else if ( PlacementType == World.ItemPlacementType.Dropped && itemData.DropScene != null )
		{
			return CreateDropped();
		}
		else
		{
			// GD.PushWarning( $"{ItemType} PlaceScene: {itemData.PlaceScene}, DropScene: {itemData.DropScene}, PlacementType: {PlacementType}" );
			// throw new Exception( $"Placement type not found for {ItemDataPath}" );
			GD.PushWarning( $"Placement type not found for {ItemDataPath}, returning dropped item" );
			return CreateDropped();
		}
	}

	public virtual DroppedItem CreateDropped()
	{
		PackedScene packedScene = GetItemData().DropScene;
		if ( packedScene == null )
		{
			// throw new Exception( $"Drop scene not found for {ItemDataPath}" );
			GD.PushWarning( $"Drop scene not found for {ItemDataPath}, using default" );
			packedScene = GD.Load<PackedScene>( "res://items/misc/dropped_item.tscn" );
		}

		var scene = packedScene.Instantiate<DroppedItem>();
		scene.ItemDataPath = ItemDataPath;
		scene.PlacementType = PlacementType;
		return scene;
	}

	public virtual PlacedItem CreatePlaced()
	{
		if ( GetItemData().PlaceScene == null )
		{
			throw new Exception( $"Place scene not found for {ItemDataPath}" );
		}

		var scene = GetItemData().PlaceScene.Instantiate<PlacedItem>();
		scene.ItemDataPath = ItemDataPath;
		scene.PlacementType = PlacementType;
		return scene;
	}

	public virtual Carriable.BaseCarriable CreateCarry()
	{
		if ( GetItemData().CarryScene == null )
		{
			throw new Exception( $"Carry scene not found for {ItemDataPath}" );
		}

		var scene = GetItemData().CarryScene.Instantiate<Carriable.BaseCarriable>();
		scene.ItemDataPath = ItemDataPath;
		return scene;
	}

	public virtual void GetData( Node3D entity )
	{
		ItemType = GetNodeType( entity ).Name;

		if ( entity is WorldItem worldItem )
		{
			ItemDataPath = worldItem.ItemDataPath;
		}
		else if ( entity is Carriable.BaseCarriable carriable )
		{
			ItemDataPath = carriable.ItemDataPath;
		}
		else
		{
			GD.PushWarning( $"Item data path not found for {entity} (unsupported type {entity.GetType()})" );
		}
	}
}
