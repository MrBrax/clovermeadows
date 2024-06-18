using System;
using vcrossing.Code.Components;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class ClothingData : ItemData
{

	[Export] public Equips.EquipSlot EquipSlot;

	[Export] public PackedScene EquipScene;

}
