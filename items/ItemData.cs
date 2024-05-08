using System.Collections.Generic;
using Godot;

namespace vcrossing;

[GlobalClass]
public partial class ItemData : Resource
{

	[Export] public string Name;
	[Export] public string Description;
	[Export] public int Width = 1;
	[Export] public int Height  = 1;
	[Export] public World.ItemPlacement Placements = new();
	// [Export] public Dictionary<string, string> Properties = new();
	[Export] public bool IsStackable = false;
	[Export] public int StackSize = 1;
	[Export] public PackedScene Prefab;
	
	public ItemData()
	{
		
	}
	
}
