using Godot;
using vcrossing2.Code.Items;

namespace vcrossing2.Code.DTO;

public class BaseDTO
{
	
	// public Vector2I GridPosition { get; set; }
	public World.ItemPlacementType PlacementType { get; set; } = World.ItemPlacementType.Placed;
	public World.ItemRotation GridRotation { get; set; } = World.ItemRotation.North;
	public string ItemDataPath { get; set; }
	
	protected ItemData GetItemData()
	{
		return GD.Load<ItemData>( ItemDataPath );
	}
	
	public virtual string GetName()
	{
		return GetItemData().Name;
	}
	
}
