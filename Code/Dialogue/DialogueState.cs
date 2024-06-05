using vcrossing.Code.Npc;
using vcrossing.Code.Player;

namespace vcrossing.Code.Dialogue;

public partial class DialogueState : GodotObject
{
	
	public PlayerController Player { get; set; }
	
	public List<BaseNpc> Npcs { get; set; }
	
	public BaseNpc MainNpc => Npcs.FirstOrDefault();
	
	public bool IsSingleNpc => Npcs.Count == 1;
	
	public string NpcName => MainNpc.GetData()?.NpcName;
	
	public string PlayerName => Player.PlayerName;
	// public string PlayerNickname => MainNpc.GetNickname( Player );
	
	public DialogueState()
	{
		Player = null;
		Npcs = new List<BaseNpc>();
	}
	
	public DialogueState( PlayerController player, List<BaseNpc> npcs )
	{
		Player = player;
		Npcs = npcs;
	}
	
	public DialogueState( PlayerController player, BaseNpc npc )
	{
		Player = player;
		Npcs = new List<BaseNpc> { npc };
	}
	
	public void TestFunction()
	{
		GD.Print( "Test function called" );
	}
	
	public void AddReputation( int amount )
	{
		MainNpc.SaveData.AddPlayerReputation( Player.PlayerId, amount );
	}

	public void StartFollowing( Node3D node )
	{
		// MainNpc.FollowTarget = node;
		MainNpc.SetFollowTarget( node );
	}
	
	public void StopFollowing()
	{
		// MainNpc.FollowTarget = null;
		MainNpc.SetFollowTarget( null );
	}
	
}
