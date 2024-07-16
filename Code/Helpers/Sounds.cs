namespace vcrossing.Code.Helpers;

public partial class Sounds : Node
{

	private static Sounds _instance;

	public override void _Ready()
	{
		_instance = this;
	}

	public AudioStreamPlayer3D Play( string path, Vector3 position )
	{
		var audioStreamPlayer3D = new AudioStreamPlayer3D();
		audioStreamPlayer3D.Stream = Loader.LoadResource<AudioStream>( path );
		audioStreamPlayer3D.Play();
		audioStreamPlayer3D.GlobalPosition = position;
		_instance.AddChild( audioStreamPlayer3D );

		audioStreamPlayer3D.Finished += () =>
		{
			audioStreamPlayer3D.QueueFree();
		};

		return audioStreamPlayer3D;
	}

	public AudioStreamPlayer Play2D( string path )
	{
		var audioStreamPlayer = new AudioStreamPlayer();
		audioStreamPlayer.Stream = Loader.LoadResource<AudioStream>( path );
		audioStreamPlayer.Play();
		_instance.AddChild( audioStreamPlayer );

		audioStreamPlayer.Finished += () =>
		{
			audioStreamPlayer.QueueFree();
		};

		return audioStreamPlayer;
	}

}
