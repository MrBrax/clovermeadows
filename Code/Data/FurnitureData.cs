using vcrossing.Code.Data;

namespace vcrossing.Code.Items;

[GlobalClass]
public sealed partial class FurnitureData : ItemData
{

	[Export] public bool CanToggleLight { get; set; } = false;

}
