using System;
using vcrossing.Code.Components;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class ToolData : ItemData, IEquipableData
{

	[Export] public int MaxDurability = 100;

	public Equips.EquipSlot EquipSlot { get; set; } = Equips.EquipSlot.Tool;
}
