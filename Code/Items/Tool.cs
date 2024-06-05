using Godot;
using vcrossing.Code.Data;

namespace vcrossing.Code.Items;

public partial class Tool : ItemData
{

	[Export] public int Durability = 1;
	[Export] public float UseTime = 1;
	[Export] public float UseRange = 1;


}
