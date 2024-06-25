using System;
using Godot;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class ItemData : Resource
{

	[Export] public string Name;
	[Export] public string Description;
	[Export] public int Width = 1;
	[Export] public int Height = 1;
	[Export] public World.ItemPlacement Placements = World.ItemPlacement.Floor & World.ItemPlacement.Underground;

	[Export] public bool IsStackable = false;

	[Export] public bool CanDrop = true;
	[Export] public bool DisablePickup = false;
	[Export] public int StackSize = 1;

	[Export] public int BaseSellPrice = 0;

	[Export] public PackedScene CarryScene;
	[Export] public PackedScene DropScene;
	[Export] public PackedScene PlaceScene;
	[Export] public CompressedTexture2D Icon;

	public virtual PackedScene DefaultTypeScene => PlaceScene;

	// [Export] public string PersistentType;

	public ItemData()
	{

	}

	public CompressedTexture2D GetIcon()
	{
		return Icon ?? Loader.LoadResource<CompressedTexture2D>( "res://icons/default_item.png" );
	}

}
