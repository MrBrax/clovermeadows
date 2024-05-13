using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code.Persistence;

namespace vcrossing2.Code;

public class WorldProp<T> where T : PersistentItem
{
	
	[JsonInclude] public T Item { get; set; }

	[JsonInclude] public Vector2I GridPosition { get; set; }
	[JsonInclude] public World.ItemRotation GridRotation { get; set; }
	
	[JsonIgnore] public Node3D Node { get; set; }
	
}
