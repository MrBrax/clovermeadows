namespace vcrossing.Code.WorldBuilder;

[GlobalClass]
public partial class Room : Resource
{
	[Export] public string Id { get; set; }
	[Export] public NodePath Wall { get; set; }
	[Export] public NodePath Floor { get; set; }

}
