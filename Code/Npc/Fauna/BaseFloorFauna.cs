using System;
using DialogueManagerRuntime;
using Godot;
using Godot.Collections;
using vcrossing.Code.Carriable;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Dialogue;
using vcrossing.Code.Helpers;
using vcrossing.Code.Items;
using vcrossing.Code.Player;
using vcrossing.Code.Save;

namespace vcrossing.Code.Npc.Fauna;

public partial class BaseFloorFauna : BaseFauna, INettable
{

	public void OnNetted( Net net )
	{
		QueueFree();
	}

}
