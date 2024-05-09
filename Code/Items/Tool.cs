using Godot;

namespace vcrossing2.Code.Items;

public partial class Tool : ItemData
{
	
	[Export] public int Durability = 1;
	[Export] public float UseTime = 1;
	[Export] public float UseRange = 1;
	
	
}
