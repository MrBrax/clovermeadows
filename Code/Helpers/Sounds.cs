namespace vcrossing.Code.Helpers;

public partial class Sounds : Node
{

	private static Sounds _instance;

	public override void _Ready()
	{
		_instance = this;
	}

	public static AudioStreamPlayer3D Play( string path, Vector3 position )
	{
		if ( !_instance.IsInsideTree() ) throw new System.Exception( "Sounds instance not in tree" );
		var audioStreamPlayer3D = new AudioStreamPlayer3D();
		_instance.AddChild( audioStreamPlayer3D );
		audioStreamPlayer3D.Stream = Loader.LoadResource<AudioStream>( path );
		audioStreamPlayer3D.GlobalPosition = position;

		audioStreamPlayer3D.Finished += () =>
		{
			audioStreamPlayer3D.QueueFree();
		};

		audioStreamPlayer3D.Play();

		return audioStreamPlayer3D;
	}

	public static AudioStreamPlayer Play2D( string path )
	{
		if ( !_instance.IsInsideTree() ) throw new System.Exception( "Sounds instance not in tree" );
		var audioStreamPlayer = new AudioStreamPlayer();
		_instance.AddChild( audioStreamPlayer );
		audioStreamPlayer.Stream = Loader.LoadResource<AudioStream>( path );

		audioStreamPlayer.Finished += () =>
		{
			audioStreamPlayer.QueueFree();
		};

		audioStreamPlayer.Play();

		return audioStreamPlayer;
	}

}
