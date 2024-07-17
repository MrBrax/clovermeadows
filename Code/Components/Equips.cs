using System;
using vcrossing.Code.Carriable;
using vcrossing.Code.Player;

namespace vcrossing.Code.Components;

public sealed partial class Equips : Node3D
{

	[Flags]
	public enum EquipSlot
	{
		None = 1 << 0, // Not a valid slot
		Hat = 1 << 1,
		Top = 1 << 2,
		Bottom = 1 << 3,
		Shoes = 1 << 4,
		Tool = 1 << 5,

		Hair = 1 << 6,
		// TODO: add more later?
	}

	[Export]
	public Godot.Collections.Array<EquipAttachNode> AttachNodes { get; set; } = new();

	public Dictionary<EquipSlot, Node3D> EquippedItems { get; set; } = new();

	[Signal]
	public delegate void EquippedItemChangedEventHandler( EquipSlot slot, Node3D item );

	[Signal]
	public delegate void EquippedItemRemovedEventHandler( EquipSlot slot );

	public Node3D GetEquippedItem( EquipSlot slot )
	{
		if ( EquippedItems.ContainsKey( slot ) )
		{
			return EquippedItems[slot];
		}
		return null;
	}

	public T GetEquippedItem<T>( EquipSlot slot ) where T : Node3D
	{
		if ( EquippedItems.ContainsKey( slot ) )
		{
			return EquippedItems[slot] as T;
		}
		return null;
	}

	public bool IsEquippedItemType<T>( EquipSlot slot )
	{
		if ( EquippedItems.ContainsKey( slot ) )
		{
			return EquippedItems[slot] is T;
		}
		return false;
	}

	public bool HasEquippedItem( EquipSlot slot )
	{
		return EquippedItems.ContainsKey( slot ) && IsInstanceValid( EquippedItems[slot] );
	}

	public bool TryGetEquippedItem( EquipSlot slot, out Node3D item )
	{
		if ( EquippedItems.ContainsKey( slot ) )
		{
			item = EquippedItems[slot];
			return true;
		}
		item = null;
		return false;
	}

	public bool TryGetEquippedItem<T>( EquipSlot slot, out T item ) where T : Node3D
	{
		if ( EquippedItems.ContainsKey( slot ) )
		{
			item = EquippedItems[slot] as T;
			return true;
		}
		item = null;
		return false;
	}

	public void SetEquippedItem( EquipSlot slot, Node3D item )
	{

		if ( slot == 0 ) throw new Exception( "Slot 0 is not allowed" );

		if ( !IsInstanceValid( item ) )
		{
			throw new Exception( "Item is not valid" );
		}

		if ( EquippedItems.ContainsKey( slot ) )
		{
			Logger.Info( "Equips", $"Removing already equipped {EquippedItems[slot]} from {slot}" );
			RemoveEquippedItem( slot, true );
		}

		EquippedItems.Add( slot, item );

		var attachNodeData = AttachNodes.FirstOrDefault( x => x.Slot == slot );

		if ( attachNodeData == null )
		{
			// Logger.LogError( "Equips", $"No attach node for slot {slot}" );
			throw new Exception( $"No attach node for slot {slot} on {this}" );
		}

		var node = GetNode<Node3D>( attachNodeData.Node );
		if ( !IsInstanceValid( node ) ) throw new Exception( $"Attach node {attachNodeData.Node} is not valid" );
		if ( IsInstanceValid( item.GetParent() ) ) item.GetParent().RemoveChild( item );
		node.AddChild( item );
		item.GlobalTransform = node.GlobalTransform;

		if ( item is Carriable.BaseCarriable carriable )
		{
			carriable.SetHolder( GetParent<PlayerController>() );
			carriable.OnEquip( GetParent<PlayerController>() ); // TODO: check if player or npc
		}

		Logger.Info( "Equips", $"Equipped {item} to {slot}" );

		EmitSignal( SignalName.EquippedItemChanged, (int)slot, item );

	}

	public void RemoveEquippedItem( EquipSlot slot, bool free = false )
	{
		if ( EquippedItems.ContainsKey( slot ) )
		{
			if ( free ) EquippedItems[slot].QueueFree();
			EquippedItems.Remove( slot );
			EmitSignal( SignalName.EquippedItemRemoved, (int)slot );
		}
	}

	public void SetEquippableVisibility( EquipSlot slot, bool visible )
	{
		if ( EquippedItems.ContainsKey( slot ) )
		{
			EquippedItems[slot].Visible = visible;
			return;
		}
		throw new Exception( $"No item equipped in slot {slot}" );
	}

	public override void _Input( InputEvent @event )
	{

		if ( !IsInstanceValid( GetParent<PlayerController>() ) ) return;

		if ( @event.IsActionPressed( "UseTool" ) )
		{
			if ( HasEquippedItem( EquipSlot.Tool ) )
			{
				var tool = GetEquippedItem<BaseCarriable>( EquipSlot.Tool );
				if ( tool.CanUse() )
				{
					tool.OnUse( GetParent<PlayerController>() );
					tool.OnUseDown( GetParent<PlayerController>() );
				}
			}
		}
		else if ( @event.IsActionReleased( "UseTool" ) )
		{
			if ( HasEquippedItem( EquipSlot.Tool ) )
			{
				var tool = GetEquippedItem<BaseCarriable>( EquipSlot.Tool );
				tool.OnUseUp( GetParent<PlayerController>() );
			}
		}

	}


}
