using System.Threading.Tasks;
using vcrossing2.Code.Helpers;

namespace vcrossing2.Code.Ui;

public partial class Fader : ColorRect
{
	[Export] public float FadeTime = 0.5f;

	private bool _isFading = false;
	private bool _targetState = false;
	private float _fadeStartTime = 0;

	[Signal] public delegate void FadeOutCompleteEventHandler();
	[Signal] public delegate void FadeInCompleteEventHandler();

	public override void _Ready()
	{
		_targetState = false;
	}

	public async Task FadeOut()
	{
		FixResolution();
		_targetState = false;
		_fadeStartTime = Time.GetTicksMsec();
		_isFading = true;
		Logger.Info( "Fader", "Fading out." );
		await ToSignal( this, SignalName.FadeOutComplete );
	}

	public async Task FadeIn()
	{
		FixResolution();
		_targetState = true;
		_fadeStartTime = Time.GetTicksMsec();
		_isFading = true;
		Logger.Info( "Fader", "Fading in." );
		await ToSignal( this, SignalName.FadeInComplete );
	}

	private void FixResolution()
	{
		if ( Material is not ShaderMaterial material ) return;
		material.SetShaderParameter( "screen_width", GetViewportRect().Size.X );
		material.SetShaderParameter( "screen_height", GetViewportRect().Size.Y );
	}

	public override void _Process( double delta )
	{
		if ( !_isFading ) return;
		var time = Time.GetTicksMsec() - _fadeStartTime;
		var progress = time / (FadeTime * 1000);
		if ( Material is not ShaderMaterial material ) return;
		material.SetShaderParameter( "progress", !_targetState ? progress : 1 - progress );

		if ( time >= FadeTime * 1000 )
		{
			_isFading = false;
			if ( !_targetState )
			{
				EmitSignal( SignalName.FadeOutComplete );
			}
			else
			{
				EmitSignal( SignalName.FadeInComplete );
			}
		}
	}
}
