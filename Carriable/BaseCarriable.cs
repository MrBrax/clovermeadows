using Godot;
using vcrossing.Player;

namespace vcrossing.Carriable;

public partial class BaseCarriable : Node3D
{
	
	// [Signal] public delegate void Equip();
	// [Signal] public delegate void Unequip();
	// [Signal] public delegate void Use();
	
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
