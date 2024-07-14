using vcrossing.Code.Data;

namespace vcrossing.Code.Items;

[GlobalClass]
public partial class FurnitureData : ItemData
{

	[Export] public bool CanToggleLight { get; set; } = false;

}
