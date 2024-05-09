using vcrossing2.Code.Items;

namespace vcrossing2.Inventory.Items;

public class BaseItemDTO
{
	
	public virtual string GetName()
	{
		return "Base Item";
	}
	
	public virtual string GetDescription()
	{
		return "This is a base item.";
	}

	public virtual ItemData ItemData { get; set; }

}
