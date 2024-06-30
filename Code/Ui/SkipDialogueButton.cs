using System;
using vcrossing.Code.Save;
using YarnSpinnerGodot;
using static vcrossing.Code.Save.SettingsSaveData;

namespace vcrossing.Code.Ui;

public partial class SkipDialogueButton : Button
{

	[Export] public LineView LineView { get; set; }

	public override void _Ready()
	{
		base._Ready();
	}

	private void Skip()
	{
		LineView.UserRequestedViewAdvancement();
	}

	/* public override void _Pressed()
	{
		base._Pressed();
		Logger.Info( "SkipDialogueButton._Pressed" );
	} */

	/* public override void _GuiInput( InputEvent @event )
	{
		base._GuiInput( @event );

		if ( @event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left )
		{
			Skip();
			Logger.Info( "SkipDialogueButton._GuiInput" );
		}
	} */

	public override void _Input( InputEvent @event )
	{
		base._Input( @event );

		if ( @event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left )
		{
			Skip();
			Logger.Info( "SkipDialogueButton._Input" );
		}

	}

}
