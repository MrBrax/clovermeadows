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

public partial class BaseFloorFauna : CharacterBody3D, INettable
{
	public void OnNetted( Net net )
	{
		QueueFree();
	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		GlobalPosition = new Vector3( GlobalPosition.X, 0f, GlobalPosition.Z + ((float)delta * 0.5f) );
	}
}
