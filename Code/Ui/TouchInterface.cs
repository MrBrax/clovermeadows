using vcrossing.Code.Save;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code.Ui;

public partial class TouchInterface : Control
{

	public override void _Ready()
	{
		base._Ready();
		Visible = DisplayServer.IsTouchscreenAvailable() && GetNode<SettingsSaveData>( "/root/SettingsSaveData" ).CurrentSettings.ShowTouchControls;
	}

	public void OnButtonInteractDown()
	{
		Input.ParseInputEvent( new InputEventAction() { Action = "Interact", Pressed = true } );
	}

	public void OnButtonInteractUp()
	{
		Input.ParseInputEvent( new InputEventAction() { Action = "Interact", Pressed = false } );
	}

	public void OnButtonInventory()
	{
		Input.ParseInputEvent( new InputEventAction() { Action = "Inventory", Pressed = true } );
	}

	public void OnButtonPause()
	{
		Input.ParseInputEvent( new InputEventAction() { Action = "ui_cancel", Pressed = true } );
	}

	public void OnButtonPickUp()
	{
		Input.ParseInputEvent( new InputEventAction() { Action = "PickUp", Pressed = true } );
	}

	public void OnButtonUseToolDown()
	{
		Input.ParseInputEvent( new InputEventAction() { Action = "UseTool", Pressed = true } );
	}

	public void OnButtonUseToolUp()
	{
		Input.ParseInputEvent( new InputEventAction() { Action = "UseTool", Pressed = false } );
	}

	public void OnButtonDebug()
	{
		Input.ParseInputEvent( new InputEventAction() { Action = "Debug", Pressed = true } );
	}

}
