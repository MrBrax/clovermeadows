using System;
using vcrossing.Code.Carriable;
using vcrossing.Code.Player;

namespace vcrossing.Code.Components;

public partial class Equips : Node3D
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
		// TODO: add more later?
	}

	[Export]
	public Godot.Collections.Array<EquipAttachNode> AttachNodes { get; set; } = new();

	public Dictionary<EquipSlot, Node3D> EquippedItems { get; set; } = new();

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

	public bool HasEquippedItem( EquipSlot slot )
	{
		return EquippedItems.ContainsKey( slot ) && IsInstanceValid( EquippedItems[slot] );
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
			EquippedItems[slot] = item;
		}
		else
		{
			EquippedItems.Add( slot, item );
		}

		var attachNodeData = AttachNodes.FirstOrDefault( x => x.Slot == slot );

		if ( attachNodeData != null )
		{
			var node = GetNode<Node3D>( attachNodeData.Node );
			if ( !IsInstanceValid( node ) ) throw new Exception( $"Attach node {attachNodeData.Node} is not valid" );
			if ( IsInstanceValid( item.GetParent() ) ) item.GetParent().RemoveChild( item );
			node.AddChild( item );
			item.GlobalTransform = node.GlobalTransform;
			Logger.Info( "Equips", $"Equipped {item} to {slot}" );
		}
		else
		{
			// Logger.LogError( "Equips", $"No attach node for slot {slot}" );
			throw new Exception( $"No attach node for slot {slot} on {this}" );
		}
	}

	public void RemoveEquippedItem( EquipSlot slot, bool free = false )
	{
		if ( EquippedItems.ContainsKey( slot ) )
		{
			if ( free ) EquippedItems[slot].QueueFree();
			EquippedItems.Remove( slot );
		}
	}

	public override void _Input( InputEvent @event )
	{

		if ( !IsInstanceValid( GetParent<PlayerController>() ) ) return;

		if ( @event is InputEventKey keyEvent )
		{
			if ( keyEvent.IsActionPressed( "UseTool" ) )
			{
				if ( HasEquippedItem( EquipSlot.Tool ) )
				{
					var tool = GetEquippedItem<BaseCarriable>( EquipSlot.Tool );
					if ( tool.CanUse() )
					{
						tool.OnUse( GetParent<PlayerController>() );
					}
				}
			}

		}

	}


}
