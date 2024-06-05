namespace vcrossing.Code.Items;

public partial class Tree : WorldItem
{
	public override bool CanBePickedUp()
	{
		return false;
	}
	
	public override bool ShouldBeSaved()
	{
		return false;
	}
}
