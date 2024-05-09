using Godot;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Carriable;

public partial class BaseCarriable : Node3D
{
	
	// [Signal] public delegate void Equip();
	// [Signal] public delegate void Unequip();
	// [Signal] public delegate void Use();
	
	protected World World => GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;
	
	public ItemData ItemData { get; set; }
	
	public virtual void OnEquip( PlayerController player )
	{
		
	}
	
	public virtual void OnUnequip( PlayerController player )
	{
		
	}
	
	public virtual void OnUse( PlayerController player )
	{
		
	}
	
}
