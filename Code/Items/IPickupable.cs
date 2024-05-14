using vcrossing2.Code.Player;

namespace vcrossing2.Code.Items;

public interface IPickupable
{
	
	public void OnPickup( PlayerController player );
	// public void OnDrop( PlayerController player );
	
}
