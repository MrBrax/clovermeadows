using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public interface IUsable
{

	public bool CanUse( PlayerController player );

	/// <summary>
	/// Called when the player uses this node
	/// </summary>
	/// <param name="player"></param>
	public void OnUse( PlayerController player );

}
