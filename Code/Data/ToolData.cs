using System;
using vcrossing.Code.Components;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class ToolData : ItemData, IEquipableData
{

	[Export] public int MaxDurability = 100;

	public Equips.EquipSlot EquipSlot { get; set; } = Equips.EquipSlot.Tool;

	[Export] public CompressedTexture2D TouchUseIcon;

	public override CompressedTexture2D GetIcon()
	{
		return Icon ?? Loader.LoadResource<CompressedTexture2D>( "res://icons/default_tool.png" );
	}

}
