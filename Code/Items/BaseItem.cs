using Godot;

namespace vcrossing.Code;

public partial class BaseItem : Node3D
{
	
	[Export(PropertyHint.File, "*.tres")] 
	public string ItemDataPath { get; set; }
	[Export] public NodePath Model { get; set; }
	
	protected World World => GetNode<WorldManager>( "/root/Main/WorldContainer" ).ActiveWorld;
	
}
