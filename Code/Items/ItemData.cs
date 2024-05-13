using System;
using Godot;

namespace vcrossing2.Code.Items;

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
	[Export] public bool CanEquip = false;
	[Export] public bool DisablePickup = false;
	[Export] public int StackSize = 1;
	
	[Export] public PackedScene CarryScene;
	[Export] public PackedScene DropScene;
	[Export] public PackedScene PlaceScene;
	[Export] public CompressedTexture2D Icon;
	
	[Export] public string PersistentType;
	
	public ItemData()
	{
		
	}
	
}
