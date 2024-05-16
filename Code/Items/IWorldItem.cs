namespace vcrossing2.Code.Items;

public interface IWorldItem
{
	
	public bool IsPlacedInEditor { get; set; }
	public World.ItemPlacement Placement { get; set; }
	public string ItemDataPath { get; set; }
	
	/// <summary>
	///  Should this item be saved to the world file?
	/// </summary>
	/// <returns></returns>
	public bool ShouldBeSaved();
	
}
