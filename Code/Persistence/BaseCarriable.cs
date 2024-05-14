using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Persistence;

public class BaseCarriable : PersistentItem, IPickupable
{
	
	[JsonInclude] public int Durability { get; set; }
	
	public override void GetLinkData( WorldNodeLink nodeLink )
	{
		base.GetLinkData( nodeLink );
		
		if ( nodeLink.Node is Carriable.BaseCarriable carriable )
		{
			Durability = carriable.Durability;
		}
	}

	public override Carriable.BaseCarriable CreateCarry()
	{
		var carriable = base.CreateCarry();
		carriable.Durability = Durability;
		return carriable;
	}

	public void OnPickup( PlayerController player )
	{
		throw new System.NotImplementedException();
	}
}
