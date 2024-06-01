#if TOOLS
using Godot;
using System;

[Tool]
public partial class Plugin : EditorPlugin
{

	AreaExitGizmoPlugin _areaExitGizmoPlugin = new AreaExitGizmoPlugin();

	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		AddNode3DGizmoPlugin( _areaExitGizmoPlugin );
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveNode3DGizmoPlugin( _areaExitGizmoPlugin );
	}
}
#endif
