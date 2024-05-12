using Godot;
using vcrossing2.Code.DTO;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Carriable;

public partial class BaseCarriable : Node3D
{
	
	// [Signal] public delegate void Equip();
	// [Signal] public delegate void Unequip();
	// [Signal] public delegate void Use();
	
	public Player.Inventory Inventory { get; set; }
	
	public BaseCarriableDTO DTO { get; set; } = new();
	
	protected World World => GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;
	
	[Export] public string ItemDataPath { get; set; }
	
	public ItemData GetItemData()
	{
		return GD.Load<ItemData>( ItemDataPath );
	}
	
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
