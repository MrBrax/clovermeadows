namespace vcrossing.Code.Ui;

public partial class Menu : Control
{
	public void StartGame()
	{
		if ( FileAccess.FileExists( "user://playing.dat" ) )
		{
			Logger.Warn( "Menu", "User exited game without saving." );
			// TODO: scold the player for not saving
		}

		var f = FileAccess.Open( "user://playing.dat", FileAccess.ModeFlags.Write );
		f.Store8( 1 );
		f.Close();
		GetTree().ChangeSceneToFile( "res://scenes/game.tscn" );
	}

}
