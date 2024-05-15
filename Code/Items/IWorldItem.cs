namespace vcrossing2.Code.Items;

public interface IWorldItem
{
	
	/// <summary>
	///  Should this item be saved to the world file?
	/// </summary>
	/// <returns></returns>
	public bool ShouldBeSaved();
	
}
