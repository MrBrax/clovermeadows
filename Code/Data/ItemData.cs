using System;
using Godot;

namespace vcrossing2.Code.Data;

[GlobalClass]
public partial class ItemData : Resource
{

	[Export] public string Name;
	[Export] public string Description;
	[Export] public int Width = 1;
	[Export] public int Height = 1;
	[Export] public World.ItemPlacement Placements = World.ItemPlacement.Floor & World.ItemPlacement.Underground;

	[Export] public bool IsStackable = false;
	[Export] public bool CanEquip = false;
	[Export] public bool CanDrop = true;
	[Export] public bool DisablePickup = false;
	[Export] public int StackSize = 1;

	// TODO: separate durability from item data somehow?
	[Export] public int MaxDurability = 100;

	[Export] public int BaseSellPrice = 0;

	[Export] public PackedScene CarryScene;
	[Export] public PackedScene DropScene;
	[Export] public PackedScene PlaceScene;
	[Export] public CompressedTexture2D Icon;

	[Export] public string PersistentType;

	public ItemData()
	{

	}

}
