using vcrossing2.Code.Helpers;

namespace vcrossing2.Code.Ui;

public partial class Fader : CanvasLayer
{
	[Export] public float FadeTime = 0.5f;

	private ColorRect _fadeRect;

	private bool _isFading = false;
	private bool _targetState = false;
	private float _fadeStartTime = 0;

	public override void _Ready()
	{
		_fadeRect = GetNode<ColorRect>( "FadeRect" );
		_fadeRect.Visible = false;
		_targetState = false;
	}

	public void FadeOut()
	{
		/*Logger.Info( "Fader", "Fading out." );
		// await ToSignal( GetTree(), "idle_frame" );
		_fadeRect.Visible = true;
		_fadeRect.Modulate = new Color( 0, 0, 0, 1 );
		var tween = CreateTween();
		tween.TweenProperty( _fadeRect, "modulate", new Color( 0, 0, 0, 0 ), FadeTime );
		await ToSignal( tween, Tween.SignalName.Finished );
		Logger.Info( "Fader", "Faded out." );*/
		_targetState = false;
		_fadeStartTime = Time.GetTicksMsec();
		_isFading = true;
		_fadeRect.Visible = true;
		_fadeRect.Modulate = new Color( 0, 0, 0, 1 );
		Logger.Info( "Fader", "Fading out." );
	}

	public void FadeIn()
	{
		/*Logger.Info( "Fader", "Fading in." );
		// await ToSignal( GetTree(), "idle_frame" );
		_fadeRect.Visible = true;
		_fadeRect.Modulate = new Color( 0, 0, 0, 0 );
		var tween = CreateTween();
		tween.TweenProperty( _fadeRect, "modulate", new Color( 0, 0, 0, 1 ), FadeTime );
		await ToSignal( tween, Tween.SignalName.Finished );
		_fadeRect.Visible = false;
		Logger.Info( "Fader", "Faded in." );*/
		_targetState = true;
		_fadeStartTime = Time.GetTicksMsec();
		_isFading = true;
		_fadeRect.Visible = true;
		_fadeRect.Modulate = new Color( 0, 0, 0, 0 );
		Logger.Info( "Fader", "Fading in." );
	}

	public override void _Process( double delta )
	{
		if ( !_isFading ) return;
		var time = Time.GetTicksMsec() - _fadeStartTime;
		var alpha = time / ( FadeTime * 1000 );
		if ( _targetState )
		{
			_fadeRect.Modulate = new Color( 0, 0, 0, alpha );
			if ( alpha >= 1 )
			{
				_fadeRect.Visible = true;
				_isFading = false;
				Logger.Info( "Fader", "Faded in." );
			}
		}
		else
		{
			_fadeRect.Modulate = new Color( 0, 0, 0, 1 - alpha );
			if ( alpha >= 1 )
			{
				_fadeRect.Visible = false;
				_isFading = false;
				Logger.Info( "Fader", "Faded out." );
			}
		}
	}
}
