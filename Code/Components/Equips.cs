using System;
using vcrossing.Code.Carriable;
using vcrossing.Code.Player;

namespace vcrossing.Code.Components;

public partial class Equips : Node3D
{

	public enum EquipSlot
	{
		Hat = 1,
		Top = 2,
		Bottom = 3,
		Shoes = 4,
		Tool = 5,
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

	public void SetEquippedItem( EquipSlot tool, Node3D item )
	{

		if ( !IsInstanceValid( item ) )
		{
			throw new Exception( "Item is not valid" );
		}

		if ( EquippedItems.ContainsKey( tool ) )
		{
			EquippedItems[tool] = item;
		}
		else
		{
			EquippedItems.Add( tool, item );
		}

		var attachNodeData = AttachNodes.FirstOrDefault( x => x.Slot == tool );

		if ( attachNodeData != null )
		{
			var node = GetNode<Node3D>( attachNodeData.Node );
			if ( !IsInstanceValid( node ) ) throw new Exception( $"Attach node {attachNodeData.Node} is not valid" );
			if ( IsInstanceValid( item.GetParent() ) ) item.GetParent().RemoveChild( item );
			item.GlobalTransform = node.GlobalTransform;
			node.AddChild( item );
		}
		else
		{
			Logger.LogError( $"No attach node for {tool}" );
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
