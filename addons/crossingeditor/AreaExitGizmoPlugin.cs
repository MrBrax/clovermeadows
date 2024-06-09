#if TOOLS
using Godot;
using System;
using vcrossing.Code.Items;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code;

public partial class AreaExitGizmoPlugin : EditorNode3DGizmoPlugin
{

	public override string _GetGizmoName()
	{
		return "AreaExit";
	}

	public override bool _HasGizmo( Node3D forNode3D )
	{
		var script = forNode3D.GetScript().As<CSharpScript>();
		if ( script != null )
		{
			var filename = script.ResourcePath.GetFile().GetBaseName();
			return filename == "AreaExit" || filename == "AreaTrigger";
		}

		return forNode3D is AreaExit || forNode3D is AreaTrigger;
	}

	public AreaExitGizmoPlugin()
	{
		GD.Print( "Creating AreaExitGizmoPlugin." );
		CreateMaterial( "main", new Color( 1f, 1f, 1f, 1f ) );
	}

	public override void _Redraw( EditorNode3DGizmo gizmo )
	{
		gizmo.Clear();
		var areaExit = gizmo.GetNode3D();
		var arrowStart = Vector3.Zero;
		var arrowEnd = new Vector3( 0, 0, 10 );
		gizmo.AddLines( new Vector3[] { arrowStart, arrowEnd }, GetMaterial( "main" ) );
	}
}
#endif
