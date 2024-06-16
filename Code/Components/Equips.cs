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
	public Godot.Collections.Dictionary<EquipSlot, Node3D> AttachNodes { get; set; } = new();

	public Dictionary<EquipSlot, Node3D> EquippedItems { get; set; } = new();

	public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
	{
		var list = new Godot.Collections.Array<Godot.Collections.Dictionary>();

		foreach ( var slot in AttachNodes.Keys )
		{
			list.Add( new Godot.Collections.Dictionary
			{
				{ "name", slot.ToString() },
				{ "type", "NodePath" },
				{ "hint", "node" },
				{ "usage", "editor" },
				{ "default_value", AttachNodes[slot] }
			} );
		}

		return list;
	}

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
		if ( EquippedItems.ContainsKey( tool ) )
		{
			EquippedItems[tool] = item;
		}
		else
		{
			EquippedItems.Add( tool, item );
		}

		if ( AttachNodes.ContainsKey( tool ) )
		{
			item.GetParent().RemoveChild( item );
			item.GlobalTransform = AttachNodes[tool].GlobalTransform;
			AttachNodes[tool].AddChild( item );
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


}
