namespace vcrossing.Code.Components;

[GlobalClass]
public sealed partial class EquipAttachNode : Resource
{

	[Export] public Equips.EquipSlot Slot;
	[Export] public NodePath Node;

}
