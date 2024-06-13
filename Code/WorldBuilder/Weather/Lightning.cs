namespace vcrossing.Code.WorldBuilder.Weather;

public partial class Lightning : WeatherBase
{

	[Export] public AudioStreamPlayer3D ThunderSoundPlayer { get; set; }

	[Export] public DirectionalLight3D LightningLight { get; set; }

	// private const float _lightningMinDelay = 90000.0f;
	// private const float _lightningRandomDelay = 300000.0f;
	private const float _lightningMinDelay = 20000.0f;
	private const float _lightningRandomDelay = 10000.0f;

	public override void _Ready()
	{
		base._Ready();
		_nextLightningTime = Time.GetTicksMsec() + _lightningMinDelay + (float)GD.RandRange( 0.0, _lightningRandomDelay );
	}

	private float _nextLightningTime = 0.0f;

	public override void _Process( double delta )
	{
		base._Process( delta );

		if ( !_enabled ) return;

		// SunLight.ShadowBlur = 0.5f + (float)GD.RandRange( 0.0, 0.5 );

		if ( Time.GetTicksMsec() > _nextLightningTime )
		{
			StrikeLightning();
			_nextLightningTime = Time.GetTicksMsec() + _lightningMinDelay + (float)GD.RandRange( 0.0, _lightningRandomDelay );
		}

	}

	private void StrikeLightning()
	{

		if ( !IsInstanceValid( ThunderSoundPlayer ) ) throw new System.Exception( "Thunder sound is not valid" );

		if ( IsInstanceValid( LightningLight ) )
		{
			LightningLight.Visible = true;

			// hide lightning after a frame
			ToSignal( GetTree(), SceneTree.SignalName.ProcessFrame ).OnCompleted( () =>
			{
				LightningLight.Visible = false;
			} );
		}

		// simulate lightning distance
		ToSignal( GetTree().CreateTimer( 1f + GD.Randf() * 4f ), Timer.SignalName.Timeout ).OnCompleted( () =>
		{
			ThunderSoundPlayer.Play();
		} );
	}

}
