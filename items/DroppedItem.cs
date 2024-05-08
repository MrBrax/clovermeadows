using Godot;

namespace vcrossing;

public class DroppedItem : Node3D
{
	[Export] public string ItemDataPath;

	public ItemData ItemData => ResourceLoader.Load<ItemData>( ItemDataPath );
	
}
