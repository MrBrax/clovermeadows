using System;
using vcrossing.Code.Components;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class ClothingData : ItemData, IEquipableData
{

	/// <summary>
	/// The slot this clothing item will be equipped to. Only one slot per item.
	/// </summary>
	[Export]
	public Equips.EquipSlot EquipSlot { get; set; } = Equips.EquipSlot.Hat;

	[Export] public Equips.EquipSlot HidesSlot { get; set; } = Equips.EquipSlot.None;

	[Export] public PackedScene EquipScene;

	public override CompressedTexture2D GetIcon()
	{
		return Icon ?? Loader.LoadResource<CompressedTexture2D>( "res://icons/default_clothing.png" );
	}

}
