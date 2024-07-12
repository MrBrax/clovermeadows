using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using vcrossing.Code.Data;
using YarnSpinnerGodot;

namespace vcrossing.Code;

public partial class MainGame : Node3D
{

	public Dictionary<string, ShopInventoryData> Shops = new();

	public override void _Ready()
	{
		base._Ready();

		// GenerateAllShopData();

		SetupDialogue();
	}

	private string _dialogueText;

	private void SetupDialogue()
	{
		var runner = GetNode<DialogueRunner>( "/root/Main/UserInterface/YarnSpinnerCanvasLayer/DialogueRunner" );
		var lineView = GetNode<Dialogue.LineView>( "/root/Main/UserInterface/YarnSpinnerCanvasLayer/LineView" );
		var speechPlayer = GetNode<AudioStreamPlayer>( "/root/Main/UserInterface/YarnSpinnerCanvasLayer/Speech" );

		lineView.onCharacterTyped += ( string currentLetter ) =>
		{

			if ( currentLetter == " " ) return;

			switch ( currentLetter )
			{
				case "1":
					currentLetter = "o";
					break;
				case "2":
					currentLetter = "t";
					break;
				case "3":
					currentLetter = "t";
					break;
				case "4":
					currentLetter = "f";
					break;
				case "5":
					currentLetter = "f";
					break;
				case "6":
					currentLetter = "s";
					break;
				case "7":
					currentLetter = "s";
					break;
				case "8":
					currentLetter = "e";
					break;
				case "9":
					currentLetter = "n";
					break;
				case "0":
					currentLetter = "z";
					break;
			}

			// only match letters
			if ( !Regex.IsMatch( currentLetter, @"[a-zA-Z]" ) )
			{
				// Logger.Info( $"YarnSpinner typed: {currentLetter} is not a letter" );
				return;
			}

			// Logger.Info( $"YarnSpinner say letter: {currentLetter}" );
			speechPlayer.Stream = Loader.LoadResource<AudioStream>( $"res://sound/speech/alphabet/{currentLetter.ToUpper()}.wav" );
			speechPlayer.Play();
			speechPlayer.PitchScale = (float)GD.RandRange( 1.8, 2.2 );

		};

		runner.onCommand += ( command ) =>
		{
			Logger.Info( $"YarnSpinner command: {command}" );
		};

		runner.onDialogueComplete += () =>
		{
			Logger.Info( "YarnSpinner dialogue complete" );
		};
	}

}
