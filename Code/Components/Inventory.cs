using System;
using System.Collections.Immutable;
using vcrossing.Code.Helpers;
using vcrossing.Code.Inventory;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Components;

public partial class Inventory : Node3D
{

	/// <summary>
	///  The player controller that owns this inventory, if any
	/// </summary>
	internal PlayerController Player => GetNode<PlayerController>( "../" );

	/* internal World World => GetNode<WorldManager>( "/root/Main/WorldManager" ).ActiveWorld;
	internal Node3D PlayerModel => GetNode<Node3D>( "../PlayerModel" );
	internal PlayerInteract PlayerInteract => GetNode<PlayerInteract>( "../PlayerInteract" ); */

	public InventoryContainer Container { get; private set; } = new InventoryContainer();

	/// <summary>
	///  the exact same as AddItem, but with a sound effect
	/// </summary>
	public void PickUpItem( PersistentItem item )
	{
		Container.AddItem( item );
		// PlayPickupSound();
	}

	// TODO: move most of this into container
	public void PickUpItem( WorldNodeLink nodeLink )
	{
		if ( string.IsNullOrEmpty( nodeLink.ItemDataPath ) ) throw new System.Exception( "Item data path is null" );

		Logger.Info( $"Picking up item {nodeLink.ItemDataPath}" );

		var inventoryItem = PersistentItem.Create( nodeLink );
		// worldItem.UpdateDTO();

		if ( inventoryItem == null )
		{
			throw new System.Exception( "Failed to create inventory item" );
		}

		inventoryItem.ItemDataPath = nodeLink.ItemDataPath;

		var index = Container.GetFirstFreeEmptyIndex();
		if ( index == -1 )
		{
			throw new InventoryFullException( "Inventory is full." );
		}

		var slot = new InventorySlot<PersistentItem>( Container )
		{
			Index = index
		};

		slot.SetItem( inventoryItem );

		Logger.Info( $"Picked up item {nodeLink.ItemDataPath}" );

		NodeExtensions.SetCollisionState( nodeLink.Node, false );

		var player = Owner as PlayerController;

		player.InCutscene = true;
		player.CutsceneTarget = Vector3.Zero;
		player.Velocity = Vector3.Zero;

		// TODO: needs dupe protection
		var tween = player.GetTree().CreateTween();
		var t = tween.Parallel().TweenProperty( nodeLink.Node, "global_position", player.GlobalPosition + Vector3.Up * 0.5f, 0.2f );
		tween.Parallel().TweenProperty( nodeLink.Node, "scale", Vector3.One * 0.1f, 0.3f ).SetTrans( Tween.TransitionType.Cubic ).SetEase( Tween.EaseType.Out );
		tween.TweenCallback( Callable.From( () =>
		{
			player.World.RemoveItem( nodeLink );
			player.InCutscene = false;

			// PlayPickupSound();

		} ) );

	}

	public void MakeInventory( int count )
	{
		Container = new InventoryContainer( count );
		Container.Owner = this.Player;
	}
}
