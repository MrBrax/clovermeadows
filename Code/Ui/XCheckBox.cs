namespace vcrossing.Code.Ui;

public partial class XCheckBox : CheckBox
{

	public override void _Ready()
	{
		// Connect( "pressed", this, nameof( OnPressed ) );
		// Pressed += _OnPressedDown;
		ButtonDown += OnPressedDown;
		ButtonUp += OnPressedUp;
	}

	protected virtual void OnPressedDown()
	{
		UiSounds.ButtonDown();
	}

	protected virtual void OnPressedUp()
	{
		UiSounds.ButtonUp();
	}
}
