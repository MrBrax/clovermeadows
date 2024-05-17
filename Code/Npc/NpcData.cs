namespace vcrossing2.Code.Npc;

[GlobalClass]
public partial class NpcData : Resource
{
	
	[Export] public string NpcName { get; set; }
	[Export] public string NpcId { get; set; }
	
	// TODO: use enum or resource or something
	[Export] public string Personality { get; set; }
	
	
	
}
