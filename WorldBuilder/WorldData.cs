using Godot;

namespace vcrossing.WorldBuilder;

[GlobalClass]
public partial class WorldData : Resource
{
	
	[Export] public string WorldId { get; set; }
	[Export] public string WorldName { get; set; }
	[Export] public PackedScene WorldScene { get; set; }
	
}
