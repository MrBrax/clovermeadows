using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Player;

namespace vcrossing.Code.WorldBuilder;

public class WorldNodeLink
{
	[JsonIgnore] public Node3D Node;

	[JsonInclude, JsonConverter( typeof( Vector2IConverter ) )]
	public Vector2I GridPosition;

	[JsonInclude] public World.ItemRotation GridRotation;
	[JsonInclude] public World.ItemPlacement GridPlacement;
	[JsonInclude] public World.ItemPlacementType PlacementType;

	[JsonIgnore] public Vector2I GridSize;

	[JsonInclude] public string ItemDataPath;
	[JsonInclude] public string ItemScenePath;

	[JsonIgnore] public World World;

	public WorldNodeLink()
	{
	}

	public WorldNodeLink( World world, Node3D node )
	{
		World = world;
		Node = node;
		GetData( node );
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
			ItemDataPath = worldItem.ItemDataPath;
			PlacementType = worldItem.PlacementType;
		}
		else if ( node is Carriable.BaseCarriable carriable )
		{
			ItemDataPath = carriable.ItemDataPath;
			PlacementType = World.ItemPlacementType.Dropped;
		}
		else if ( node is IWorldItem worldItem2 )
		{
			ItemDataPath = worldItem2.ItemDataPath;
			PlacementType = World.ItemPlacementType.Dropped;
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
		// TODO: use IsInstanceValid instead but we don't have access to the scene tree here
		return Node != null;
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
		return $"[NL:{Node.Name} {GridPosition} {GridRotation} {GridPlacement}]";
	}

	public List<Vector2I> GetGridPositions( bool global = false )
	{
		var positions = new List<Vector2I>();

		var itemData = GetItemData();

		if ( itemData == null )
		{
			throw new Exception( $"Item data not found on {this}" );
		}

		// rotate the item based on the rotation
		var width = itemData.Width;
		var height = itemData.Height;
		var rotation = GridRotation;

		// GD.Print( $"Getting size of {itemData.Name}" );
		// GD.Print( $"Width: {width}, Height: {height}, Rotation: {rotation}" );

		if ( width == 0 || height == 0 ) throw new Exception( "Item has no size" );

		if ( width == 1 && height == 1 )
		{
			return new List<Vector2I> { global ? GridPosition : Vector2I.Zero };
		}

		if ( rotation == World.ItemRotation.North )
		{
			for ( var x = 0; x < width; x++ )
			{
				for ( var y = 0; y < height; y++ )
				{
					positions.Add( new Vector2I( x, y ) );
				}
			}
		}
		else if ( rotation == World.ItemRotation.South )
		{
			for ( var x = 0; x < width; x++ )
			{
				for ( var y = 0; y < height; y++ )
				{
					positions.Add( new Vector2I( x, y * -1 ) );
				}
			}
		}
		else if ( rotation == World.ItemRotation.East )
		{
			for ( var x = 0; x < height; x++ )
			{
				for ( var y = 0; y < width; y++ )
				{
					positions.Add( new Vector2I( x, y ) );
				}
			}
		}
		else if ( rotation == World.ItemRotation.West )
		{
			for ( var x = 0; x < height; x++ )
			{
				for ( var y = 0; y < width; y++ )
				{
					positions.Add( new Vector2I( x * -1, y ) );
				}
			}
		}

		if ( global )
		{
			positions = positions.Select( p => p + GridPosition ).ToList();
		}

		return positions;
	}

	public void OnPlayerPickUp( PlayerInteract playerInteract )
	{
		if ( !CanBePickedUp() )
		{
			Logger.Info( $"Cannot pick up {GetName()}" );
			return;
		}

		var playerInventory = playerInteract.GetNode<Player.Inventory>( "../PlayerInventory" );
		playerInventory.PickUpItem( this );
	}

	private bool CanBePickedUp()
	{
		return !GetItemData().DisablePickup;
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

	public ItemData GetItemData()
	{
		return Loader.LoadResource<ItemData>( ItemDataPath );
	}

	/// <summary>
	///  Proxy for World.RemoveItem
	/// </summary>
	public void QueueFree()
	{
		World.RemoveItem( this );
	}

	private List<T> GetNodesOfType<T>() where T : Node3D
	{
		var nodes = new List<T>();

		/*if ( Node is T tNode )
		{
			nodes.Add( tNode );
		}*/

		foreach ( var child in Node.FindChildren( "*" ) )
		{
			if ( child is T tChild )
			{
				nodes.Add( tChild );
			}
		}

		return nodes;
	}

	public List<SittableNode> GetSittableNodes() => GetNodesOfType<SittableNode>();
	public List<PlaceableNode> GetPlaceableNodes() => GetNodesOfType<PlaceableNode>();

	public PlaceableNode GetPlaceableNodeAtGridPosition( Vector2I position )
	{
		return GetPlaceableNodes().FirstOrDefault( n => GridPosition == World.WorldToItemGrid( n.GlobalPosition ) );
	}

}
