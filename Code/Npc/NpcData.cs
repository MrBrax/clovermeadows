namespace vcrossing.Code.Npc;

[GlobalClass]
public partial class NpcData : Resource
{
	
	[Export] public string NpcName { get; set; }
	[Export] public string NpcId { get; set; }
	[Export] public string HomeHouseId { get; set; }
	
	// TODO: use enum or resource or something
	[Export] public string Personality { get; set; }
	
	[Export] public PackedScene NpcScene { get; set; }
	
	
	
}
