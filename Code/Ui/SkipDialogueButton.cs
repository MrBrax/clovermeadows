using System;
using vcrossing.Code.Save;
using YarnSpinnerGodot;
using static vcrossing.Code.Save.SettingsSaveData;

namespace vcrossing.Code.Ui;

/// <summary>
///  A button that skips the current dialogue line. YarnSpinner does not provide a built-in way to skip lines, so this button is used to advance the dialogue view.
///  It also skips the typewriter effect.
/// </summary>
public partial class SkipDialogueButton : Button
{

	[Export] public Dialogue.LineView LineView { get; set; }

	public override void _Ready()
	{
		base._Ready();
	}

	private void Skip()
	{
		LineView.UserRequestedViewAdvancement();
	}

	public override void _Input( InputEvent @event )
	{
		base._Input( @event );

		if ( @event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left )
		{
			Skip();
		}

	}

}
