using Godot;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class WorldData : Resource
{

	[Export] public string WorldId { get; set; }
	[Export] public string WorldName { get; set; }
	[Export] public PackedScene WorldScene { get; set; }

	[Export] public bool PlacementDisabled { get; set; }

	[Export] public int Width { get; set; } = 16;
	[Export] public int Height { get; set; } = 16;
	[Export] public bool UseAcres { get; set; } = false;

}
