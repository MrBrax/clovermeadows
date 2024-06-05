using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public interface IPickupable
{
	
	public void OnPickup( PlayerController player );
	// public void OnDrop( PlayerController player );
	
}
