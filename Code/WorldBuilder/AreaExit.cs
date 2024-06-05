using Godot;
using vcrossing.Code.Helpers;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.WorldBuilder;

// [GlobalClass, Tool]
public partial class AreaExit : Node3D
{

	[Export] public Door Door { get; set; }

	public void OnExited()
	{
		Logger.Info( "AreaExit", "Player exited." );
		if ( Door != null )
		{
			Logger.Info( "AreaExit", "Closing door." );
			Door.SetState( true );
			// Door.Close();
			ToSignal( GetTree().CreateTimer( 1f ), Timer.SignalName.Timeout ).OnCompleted( () =>
			{
				Door.Close();
			} );
		}
		else
		{
			Logger.Info( "AreaExit", "No door to close." );
		}
	}

}
