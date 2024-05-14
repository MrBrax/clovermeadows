using Godot;

namespace vcrossing2.Code;

public partial class BaseItem : Node3D
{
	
	[Export] public string ItemDataPath { get; set; }
	[Export] public NodePath Model { get; set; }
	
	protected World World => GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;
	
}
