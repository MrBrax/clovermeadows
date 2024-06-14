using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Ui;

public partial class UserInterface : Control
{

	[Export] public Label DateLabel;

	public override void _Process( double delta )
	{
		base._Process( delta );

		var timeManager = GetNode<TimeManager>( "/root/Main/TimeManager" );
		DateLabel.Text = timeManager.GetDate();
	}

}
