using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Player;

namespace vcrossing.Code.WorldBuilder;

/// <summary>
///  This is a link between a <see cref="Node3D"/> object and the world grid. It is used to store information about the object's position, rotation, etc for saving and loading.
///  Absolutely DO NOT place nodes in the world grid without using this class, as it will break saving and loading.
/// </summary>
public class WorldNodeLink
{

	/// <summary>
	/// The Node3D associated with this WorldNodeLink. It can be anything, like a WorldItem, Carriable, etc.
	/// If it is contained within the world grid, it should be here.
	/// </summary>
	[JsonIgnore] public Node3D Node;

	[JsonInclude, JsonConverter( typeof( Vector2IConverter ) )]
	public Vector2I GridPosition;

	[JsonInclude] public World.ItemRotation GridRotation;
	[JsonInclude] public World.ItemPlacement GridPlacement;
	[JsonInclude] public World.ItemPlacementType PlacementType;

	[JsonIgnore] public Vector2I GridSize;

	[JsonInclude, Obsolete( "Use ItemDataId instead." )] public string ItemDataPath;
	[JsonInclude] public string ItemDataId;

	/// <summary>
	///  A static path to the scene file of the item. It's hard to keep track of what scene in the item data is used when loading, so this is used to load the scene directly.
	/// </summary>
	[JsonInclude] public string ItemScenePath;
	[JsonIgnore] public ItemData ItemData;

	/// <summary>
	///  Helper accessor for the active world
	/// </summary>
	[JsonIgnore] public World World;

	public WorldNodeLink()
	{
		// LoadItemData();
	}

	public WorldNodeLink( World world, Node3D node )
	{
		World = world;
		Node = node;
		GetData( node );
		LoadItemData();
	}

	/*public WorldNodeLink( Node3D node )
	{
		Node = node;
		GetData( node );
	}*/

	public T GetNode<T>() where T : Node3D
	{
		return Node as T;
	}

	/// <summary>
	/// Retrieves data from the specified <see cref="Node3D"/> object.
	/// </summary>
	/// /// <param name="node">The <see cref="Node3D"/> object to retrieve data from.</param>
	private void GetData( Node3D node )
	{
		if ( node is WorldItem worldItem )
		{
			// ItemDataPath = worldItem.ItemDataPath;
			PlacementType = worldItem.PlacementType;
		}
		else if ( node is Carriable.BaseCarriable carriable )
		{
			// ItemDataPath = carriable.ItemDataPath;
			PlacementType = World.ItemPlacementType.Dropped;
		}
		else if ( node is IWorldItem worldItem2 )
		{
			// ItemDataPath = worldItem2.ItemDataPath;
			PlacementType = World.ItemPlacementType.Dropped;
		}

		if ( node is IDataPath dataPath )
		{
			ItemDataPath = dataPath.ItemDataPath;
			ItemDataId = dataPath.ItemDataId;
		}
		else
		{
			Logger.Warn( $"Item data path not found for {node} (unsupported type {node.GetType()})" );
		}
	}

	public bool ShouldBeSaved()
	{
		// return true;
		if ( Node is IWorldItem worldItem )
		{
			return worldItem.ShouldBeSaved();
		}

		return true;
	}

	public bool IsValid()
	{
		return Node != null && GodotObject.IsInstanceValid( Node );
	}

	public string GetName()
	{
		return Node.Name;
	}

	public void DestroyNode()
	{
		Node.QueueFree();
	}

	public override string ToString()
	{
		if ( Node == null ) return "[NL:NULL]";
		return $"[NL:{Node.Name} {GridPosition} {GridRotation} {GridPlacement}]";
	}

	public List<Vector2I> GetGridPositions( bool global = false )
	{
		var itemData = ItemData;

		if ( itemData == null )
		{
			throw new Exception( $"Item data not found on {this}" );
		}

		return itemData.GetGridPositions( GridRotation, GridPosition );
	}

	public List<Vector2I> RotateGridPositionsAroundPoint( World.ItemRotation newRotation, Vector2I rotateAround = default )
	{
		var positions = GetGridPositions();

		/* if ( rotateAround == default )
		{
			rotateAround = GridPosition;
		} */

		var rotatedPositions = new List<Vector2I>();

		foreach ( var position in positions )
		{
			var relativePosition = position - rotateAround;
			var rotatedPosition = (Vector2I)((Vector2)relativePosition).Rotated( World.GetRotationAngle( newRotation ) );
			rotatedPositions.Add( rotateAround + rotatedPosition );
		}

		return rotatedPositions;
	}

	public void UpdateTransform()
	{
		World.UpdateTransform( GridPosition, GridPlacement );
	}

	public void OnPlayerPickUp( PlayerInteract playerInteract )
	{
		if ( !CanBePickedUp() )
		{
			Logger.Info( $"Cannot pick up {GetName()}" );
			return;
		}

		var playerInventory = playerInteract.GetNode<Components.Inventory>( "../PlayerInventory" );
		playerInventory.PickUpItem( this );
	}

	private bool CanBePickedUp()
	{
		return !ItemData.DisablePickup;
	}

	/*public void OnPlayerCarriableUse( PlayerController player, BaseCarriable carriable )
	{
		if ( carriable is IUsable usable )
		{
			usable.OnUse( player );
		}
	}*/

	public void OnPlayerUse( PlayerController player )
	{
		if ( Node is IUsable usable )
		{
			usable.OnUse( player );
		}
	}

	/* [Obsolete]
	public ItemData GetItemData()
	{
		return Loader.LoadResource<ItemData>( ItemDataPath );
	} */

	public void LoadItemData()
	{
		if ( string.IsNullOrWhiteSpace( ItemDataPath ) ) throw new Exception( "ItemDataPath is null" );
		ItemData = Loader.LoadResource<ItemData>( ItemDataPath );
	}

	/// <summary>
	///  Proxy for World.RemoveItem
	/// </summary>
	public void QueueFree()
	{
		World.RemoveItem( this );
	}

	public List<SittableNode> GetSittableNodes() => Node.GetNodesOfType<SittableNode>();
	public List<PlaceableNode> GetPlaceableNodes() => Node.GetNodesOfType<PlaceableNode>();

	public PlaceableNode GetPlaceableNodeAtGridPosition( Vector2I position )
	{
		return GetPlaceableNodes().FirstOrDefault( n => GridPosition == World.WorldToItemGrid( n.GlobalPosition ) );
	}

	public void Remove()
	{
		World.RemoveItem( this );
	}
}
