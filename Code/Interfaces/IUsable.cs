using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public interface IUsable
{

	/// <summary>
	/// Determines whether the item can be used by the specified player.
	/// </summary>
	/// <param name="player">The player controller.</param>
	/// <returns><c>true</c> if the item can be used by the player; otherwise, <c>false</c>.</returns>
	public bool CanUse( PlayerController player );

	/// <summary>
	/// Called when the player uses this node
	/// </summary>
	/// <param name="player"></param>
	public void OnUse( PlayerController player );

	/// <summary>
	///  Returns the text to display when the player can use this node
	/// </summary>
	/// <returns></returns>
	public string GetUseText()
	{
		return "Interact";
	}

}
