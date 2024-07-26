namespace vcrossing.Code.Ui;

public partial class UiSounds : Node
{

	private AudioStreamPlayer _audioStreamPlayer;

	private static UiSounds _instance;

	public override void _Ready()
	{
		_instance = this;
		// _audioStreamPlayer3D = GetNode<AudioStreamPlayer3D>( "AudioStreamPlayer3D" );
		_instance._audioStreamPlayer = new AudioStreamPlayer();
		_instance._audioStreamPlayer.Bus = "UserInterface";
		AddChild( _audioStreamPlayer );
	}

	public static void ButtonDown()
	{
		_instance._audioStreamPlayer.Stream = Loader.LoadResource<AudioStream>( "res://sound/ui/btn_down.ogg" );
		_instance._audioStreamPlayer.Play();
	}

	public static void ButtonUp()
	{
		_instance._audioStreamPlayer.Stream = Loader.LoadResource<AudioStream>( "res://sound/ui/btn_up.ogg" );
		_instance._audioStreamPlayer.Play();
	}

	public static void PlaySound( string path )
	{
		_instance._audioStreamPlayer.Stream = Loader.LoadResource<AudioStream>( path );
		_instance._audioStreamPlayer.Play();
	}

	public static void PlaySound( AudioStream stream )
	{
		_instance._audioStreamPlayer.Stream = stream;
		_instance._audioStreamPlayer.Play();
	}

}
