using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public interface IUsable
{
	
	public bool CanUse( PlayerController player );
	public void OnUse( PlayerController player );
	
}
