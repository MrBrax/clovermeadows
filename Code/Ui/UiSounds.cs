namespace vcrossing.Code.Ui;

public partial class UiSounds : Node
{

	private AudioStreamPlayer _audioStreamPlayer;

	public override void _Ready()
	{
		// _audioStreamPlayer3D = GetNode<AudioStreamPlayer3D>( "AudioStreamPlayer3D" );
		_audioStreamPlayer = new AudioStreamPlayer();
		_audioStreamPlayer.Bus = "UserInterface";
		AddChild( _audioStreamPlayer );
	}

	public void ButtonDown()
	{
		_audioStreamPlayer.Stream = Loader.LoadResource<AudioStream>( "res://sound/ui/btn_down.ogg" );
		_audioStreamPlayer.Play();
	}

	public void ButtonUp()
	{
		_audioStreamPlayer.Stream = Loader.LoadResource<AudioStream>( "res://sound/ui/btn_up.ogg" );
		_audioStreamPlayer.Play();
	}

	public void PlaySound( string path )
	{
		_audioStreamPlayer.Stream = Loader.LoadResource<AudioStream>( path );
		_audioStreamPlayer.Play();
	}

}
