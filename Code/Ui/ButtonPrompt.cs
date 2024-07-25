using Godot;
using System;
using static Godot.GD;
using System.Collections.Generic;

public partial class ButtonPrompt : Control
{

	[Export] public StringName Action { get; set; }
	[Export] public string Prompt { get; set; }

	[Export] public Label InputLabel { get; set; }
	[Export] public Label PromptLabel { get; set; }

	public override void _Ready()
	{
		if ( Action != null )
		{
			SetAction( Action, Prompt );
		}
	}

	public void SetAction( StringName action, string text )
	{
		PromptLabel.Text = text;

		var actions = InputMap.ActionGetEvents( action );
		if ( actions.Count > 0 )
		{
			var actionEvent = actions[0];
			var eventString = actionEvent.AsText();
			var eventParts = eventString.Split( " " );
			InputLabel.Text = eventParts[0];
		}
		else
		{
			InputLabel.Text = "Not Set";
		}

	}

	public void SetLabel( string v )
	{
		PromptLabel.Text = v;
	}
}
