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

		SettingsSaveData.LoadSettings();

		CreateCheckBox( "Fullscreen", SettingsSaveData.CurrentSettings.Fullscreen, ( bool value ) => SettingsSaveData.SetFullscreen( value, true ) );
		CreateCheckBox( "VSync", SettingsSaveData.CurrentSettings.VSync, ( bool value ) => SettingsSaveData.SetVSync( value, true ) );
		// CreateCheckBox( "Borderless", ( bool value ) => DisplayServer.WindowSetMode( DisplayServer.WindowMode.ExclusiveFullscreen)
		CreateCheckBox( "SSIL", SettingsSaveData.CurrentSettings.SSIL, ( bool value ) => SettingsSaveData.SetSSIL( value, true ) );
		CreateCheckBox( "SSAO", SettingsSaveData.CurrentSettings.SSAO, ( bool value ) => SettingsSaveData.SetSSAO( value, true ) );
		CreateCheckBox( "Player Mouse Control", SettingsSaveData.CurrentSettings.PlayerMouseControl, ( bool value ) => SettingsSaveData.SetPlayerMouseControl( value, true ) );

		CreateVolumeSlider( "Master Volume", SettingsSaveData.CurrentSettings.VolumeMaster, ( float value ) => SettingsSaveData.SetVolume( "master", value, true ) );
		CreateVolumeSlider( "Effects Volume", SettingsSaveData.CurrentSettings.VolumeEffects, ( float value ) => SettingsSaveData.SetVolume( "effects", value, true ) );
		CreateVolumeSlider( "Music Volume", SettingsSaveData.CurrentSettings.VolumeMusic, ( float value ) => SettingsSaveData.SetVolume( "music", value, true ) );
		CreateVolumeSlider( "Ambient Volume", SettingsSaveData.CurrentSettings.VolumeAmbience, ( float value ) => SettingsSaveData.SetVolume( "ambience", value, true ) );
		CreateVolumeSlider( "Eating Volume", SettingsSaveData.CurrentSettings.VolumeEating, ( float value ) => SettingsSaveData.SetVolume( "eating", value, true ) );
		CreateVolumeSlider( "UI Volume", SettingsSaveData.CurrentSettings.VolumeUI, ( float value ) => SettingsSaveData.SetVolume( "ui", value, true ) );
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

	private Control CreateVolumeSlider( string text, float defaultValue, Action<float> onValueChanged )
	{
		var container = new VBoxContainer();
		SettingsListContainer.AddChild( container );

		var label = new Label();
		label.Text = text;
		container.AddChild( label );

		var control = new HSlider
		{
			// Value = defaultValue,
			MinValue = -50d,
			MaxValue = 0d,
			TickCount = 10,
			Step = 0.05d,
			// ExpEdit = true
		};
		control.SetValueNoSignal( defaultValue );
		control.ValueChanged += ( double value ) => onValueChanged( (float)value );
		container.AddChild( control );

		return control;
	}

}
