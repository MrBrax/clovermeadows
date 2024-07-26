using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public interface IPickupable
{

	public bool CanPickup( PlayerController player );

	/// <summary>
	///  Called when the player tries to pick up the item. By default it does nothing, but <see cref="WorldItem"/> has an implementation.
	///  Not every item is a 
	/// </summary>
	/// <param name="player"></param>
	public void OnPickup( PlayerController player );

}
