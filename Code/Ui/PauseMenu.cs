using System;
using vcrossing.Code.Player;
using vcrossing.Code.Save;
using static vcrossing.Code.Save.SettingsSaveData;

namespace vcrossing.Code.Ui;

public partial class PauseMenu : Control, IStopInput
{

	public void OnResumeButtonPressed()
	{
		GetNode<UserInterface>( "/root/Main/UserInterface" ).IsPaused = false;
	}

	public void OnSettingsButtonPressed()
	{

	}

	public void OnQuitButtonPressed()
	{
		GetNode<WorldManager>( "/root/Main/WorldManager" ).ActiveWorld.Save();
		GetNode<PlayerController>( "/root/Main/Player" ).Save();

		if ( FileAccess.FileExists( "user://playing.dat" ) )
		{
			DirAccess.RemoveAbsolute( "user://playing.dat" );
		}

		GetTree().ChangeSceneToFile( "res://scenes/menu.tscn" );
	}


}
