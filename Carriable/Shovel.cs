using Godot;
using vcrossing.Player;

namespace vcrossing.Carriable;

public partial class Shovel : BaseCarriable
{
	
	public override void OnEquip( PlayerController player )
	{
		GD.Print( "Equipped shovel." );
	}
	
	public override void OnUnequip( PlayerController player )
	{
		GD.Print( "Unequipped shovel." );
	}
	
	public override void OnUse( PlayerController player )
	{
		GD.Print( "Used shovel." );
	}
	
}
