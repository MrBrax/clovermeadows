using System;
using vcrossing.Code.Items;
using vcrossing.Code.Player;
using vcrossing.Code.Save;

namespace vcrossing.Code.Npc;

public partial class Shopkeeper : BaseNpc
{


	public override void OnUse( PlayerController player )
	{
		TalkTo( player, "Shopkeeper" );
	}


}
