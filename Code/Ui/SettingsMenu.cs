using System;
using vcrossing.Code.Save;
using static vcrossing.Code.Save.SettingsSaveData;

namespace vcrossing.Code.Ui;

public partial class SettingsMenu : Control
{

	[Export] public Control SettingsListContainer { get; set; }

	private SettingsSaveData SettingsSaveData => GetNode<SettingsSaveData>( "/root/SettingsSaveData" );

	public override void _Ready()
	{
		base._Ready();

		CreateCheckBox( "Fullscreen", SettingsSaveData.CurrentSettings.Fullscreen, ( bool value ) => SettingsSaveData.SetFullscreen( value, true ) );
		CreateCheckBox( "VSync", SettingsSaveData.CurrentSettings.VSync, ( bool value ) => SettingsSaveData.SetVSync( value, true ) );
		// CreateCheckBox( "Borderless", ( bool value ) => DisplayServer.WindowSetMode( DisplayServer.WindowMode.ExclusiveFullscreen)
		CreateCheckBox( "SSIL", SettingsSaveData.CurrentSettings.SSIL, ( bool value ) => SettingsSaveData.SetSSIL( value, true ) );
		CreateCheckBox( "SSAO", SettingsSaveData.CurrentSettings.SSAO, ( bool value ) => SettingsSaveData.SetSSAO( value, true ) );
		CreateCheckBox( "Player Mouse Control", SettingsSaveData.CurrentSettings.PlayerMouseControl, ( bool value ) => SettingsSaveData.SetPlayerMouseControl( value, true ) );
	}

	private CheckBox CreateCheckBox( string text, bool defaultValue, Action<bool> onToggle )
	{
		var control = new CheckBox();
		control.Text = text;
		control.ButtonPressed = defaultValue;
		control.Toggled += ( bool value ) => onToggle( value );
		SettingsListContainer.AddChild( control );
		return control;
	}

}
