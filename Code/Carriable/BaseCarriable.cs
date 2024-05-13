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
	
	public delegate void Used( PlayerController player );
	public event Used OnUsed;
	
	public delegate void Equipped( PlayerController player );
	public event Equipped OnEquipped;
	
	public delegate void Unequipped( PlayerController player );
	public event Unequipped OnUnequipped;
	
	
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
		OnEquipped?.Invoke( player );
	}
	
	public virtual void OnUnequip( PlayerController player )
	{
		OnUnequipped?.Invoke( player );
	}
	
	public virtual void OnUse( PlayerController player )
	{
		OnUsed?.Invoke( player );
	}
	
}
