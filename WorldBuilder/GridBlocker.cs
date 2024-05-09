using Godot;

namespace vcrossing.WorldBuilder;

[Tool]
public partial class GridBlocker : Node3D
{
	[Export] public Shape3D Shape { get; set; }
}
