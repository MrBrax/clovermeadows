using vcrossing.Code.Carriable;
using vcrossing.Code.Data;
using vcrossing.Code.Player;
using vcrossing.Code.Save;

namespace vcrossing.Code.Ui;

public partial class TouchInterface : Control
{

	private bool ShouldBeVisible => DisplayServer.IsTouchscreenAvailable() && NodeManager.SettingsSaveData.CurrentSettings.ShowTouchControls;

	public override void _Ready()
	{
		base._Ready();
		Visible = ShouldBeVisible;

		NodeManager.Player.Equips.EquippedItemChanged += OnEquippedItemChanged;
		NodeManager.Player.Equips.EquippedItemRemoved += OnEquippedItemRemoved;
	}

	private void OnEquippedItemChanged( Components.Equips.EquipSlot slot, Godot.Node3D item )
	{
		if ( !ShouldBeVisible ) return;
		if ( slot == Components.Equips.EquipSlot.Tool )
		{
			Visible = true;

			if ( item is BaseCarriable carriable && carriable.ItemData is ToolData toolData )
			{
				if ( toolData.TouchUseIcon != null )
				{
					GetNode<Button>( "UseTool" ).Icon = toolData.TouchUseIcon;
				}
				else
				{
					GetNode<Button>( "UseTool" ).Icon = Loader.LoadResource<CompressedTexture2D>( "res://icons/cursor/gauntlet_open.png" );
				}
			}
		}

	}

	private void OnEquippedItemRemoved( Components.Equips.EquipSlot slot )
	{
		if ( !ShouldBeVisible ) return;
		if ( slot == Components.Equips.EquipSlot.Tool )
		{
			Visible = false;
		}
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
