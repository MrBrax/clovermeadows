namespace vcrossing.Code.Ui;

public partial class Menu : Control
{
	public void StartGame()
	{
		GetTree().ChangeSceneToFile( "res://scenes/game.tscn" );
	}

}
