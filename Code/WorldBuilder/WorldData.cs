using Godot;

namespace vcrossing2.Code.WorldBuilder;

[GlobalClass]
public partial class WorldData : Resource
{
	
	[Export] public string WorldId { get; set; }
	[Export] public string WorldName { get; set; }
	[Export] public PackedScene WorldScene { get; set; }

	[Export] public int Width { get; set; } = 16;
	[Export] public int Height { get; set; } = 16;

}
