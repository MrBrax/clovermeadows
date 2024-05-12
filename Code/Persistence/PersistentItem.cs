using System;
using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code.Carriable;
using vcrossing2.Code.Items;

namespace vcrossing2.Code.Persistence;

[JsonPolymorphic( TypeDiscriminatorPropertyName = "$e" )]
public class PersistentItem
{
	public enum ItemSpawnType
	{
		Dropped,
		Placed,
		Carry
	}

	public string ItemType { get; set; }

	[JsonIgnore] public virtual bool Stackable { get; set; } = false;
	[JsonIgnore] public virtual int MaxStack { get; set; } = 1;

	public string ItemDataPath { get; set; }

	private static string GetNodeType( Node3D node )
	{
		return node.GetType().Name;
	}

	public PersistentItem Create( Node3D node )
	{
		var type = GetNodeType( node );
		PersistentItem item = null;

		// item = TypeLibrary.Create<PersistentItem>( type );
		item = (PersistentItem)Activator.CreateInstance( Type.GetType( type ) );

		if ( item == null )
		{
			return null;
		}

		item.GetData( node );

		return item;
	}

	public virtual bool IsValid()
	{
		return true;
	}

	public virtual string GetName()
	{
		return "Item";
	}

	public virtual string GetDescription()
	{
		return "";
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
	
	public virtual DroppedItem SpawnDropped()
	{
		var scene = GetItemData().DropScene.Instantiate<DroppedItem>();
		scene.ItemDataPath = ItemDataPath;
		return scene;
	}
	
	public virtual PlacedItem SpawnPlaced()
	{
		var scene = GetItemData().PlaceScene.Instantiate<PlacedItem>();
		scene.ItemDataPath = ItemDataPath;
		return scene;
	}
	
	public virtual BaseCarriable SpawnCarry()
	{
		var scene = GetItemData().CarryScene.Instantiate<BaseCarriable>();
		scene.ItemDataPath = ItemDataPath;
		return scene;
	}
	
	public virtual void GetData( Node3D entity )
	{
		ItemType = GetNodeType( entity );
	}
}
