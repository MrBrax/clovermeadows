using System;
using vcrossing.Code.Components;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class ClothingData : ItemData, IEquipableData
{

	[Export] public Equips.EquipSlot EquipSlot { get; set; }

	[Export] public PackedScene EquipScene;

}
