using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public partial class Fruit : WorldItem, IPickupable
{
	public bool CanPickup( PlayerController player )
	{
		return true;
	}

	// public void OnPickup( PlayerController player ) { }

}
