using vcrossing2.Code.Player;

namespace vcrossing2.Code.Items;

public interface IUsable
{
	
	public bool CanUse( PlayerController player );
	public void OnUse( PlayerController player );
	
}
