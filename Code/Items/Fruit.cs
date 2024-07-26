using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public partial class Fruit : WorldItem, IPickupable
{
	public override bool CanPickup( PlayerController player )
	{
		return true;
	}

}
