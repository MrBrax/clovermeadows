using Godot;

namespace vcrossing;

public partial class WorldItem : Node3D
{
	
	// [Export] public string Name { get; set; }
	public Vector2I GridPosition { get; set; }
	public World.ItemRotation GridRotation { get; set; } = World.ItemRotation.North;
	// [Export] public ItemData ItemData { get; set; }
	public string ItemDataPath { get; set; }
	[Export] public NodePath Model { get; set; }
	public World.ItemPlacement Placement { get; set; } = World.ItemPlacement.Floor;
	
	public ItemData GetItemData()
	{
		return GD.Load<ItemData>( ItemDataPath );
	}
	
}
