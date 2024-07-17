using System;
using vcrossing.Code.Save;
using static vcrossing.Code.Save.SettingsSaveData;

namespace vcrossing.Code.Ui;

public partial class SettingsMenu : Control
{

	[Export] public Control SettingsListContainer { get; set; }

	private SettingsSaveData SettingsSaveData => GetNode<SettingsSaveData>( "/root/SettingsSaveData" );

	private GameSettings CurrentSettings;
	private bool DangerousSettingChanged; // TODO: revert settings after a few seconds if not accepted

	public void Revert()
	{
		SettingsSaveData.LoadSettings();
		CurrentSettings = SettingsSaveData.CurrentSettings;
	}

	public override void _Ready()
	{
		base._Ready();
		Revert();
		AddControls();
	}

	private void AddControls()
	{

		SettingsListContainer.QueueFreeAllChildren();

		CreateCheckBox( "Fullscreen", CurrentSettings.Fullscreen, ( bool value ) =>
		{
			CurrentSettings.Fullscreen = value;
			DangerousSettingChanged = true;
		} );

		CreateCheckBox( "VSync", CurrentSettings.VSync, ( bool value ) =>
		{
			CurrentSettings.VSync = value;
			DangerousSettingChanged = true;
		} );

		CreateCheckBox( "Screen space indirect lighting", CurrentSettings.SSIL, ( bool value ) =>
		{
			CurrentSettings.SSIL = value;
			SettingsSaveData.SetSSIL( value );
		} );

		CreateCheckBox( "Signed distance field global illumination", CurrentSettings.SDFGI, ( bool value ) =>
		{
			CurrentSettings.SDFGI = value;
			SettingsSaveData.SetSDFGI( value );
		} );

		CreateCheckBox( "Screen space ambient occlusion", CurrentSettings.SSAO, ( bool value ) =>
		{
			CurrentSettings.SSAO = value;
			SettingsSaveData.SetSSAO( value );
		} );

		CreateCheckBox( "Screen space reflections", CurrentSettings.SSR, ( bool value ) =>
		{
			CurrentSettings.SSR = value;
			SettingsSaveData.SetSSR( value );
		} );

		/* CreateCheckBox( "Player Mouse Control", CurrentSettings.PlayerMouseControl, ( bool value ) =>
		{
			CurrentSettings.PlayerMouseControl = value;
		} ); */

		CreateCheckBox( "Show Touch Controls", CurrentSettings.ShowTouchControls, ( bool value ) =>
		{
			CurrentSettings.ShowTouchControls = value;
		} );

		CreateCheckBox( "Show crosshair", CurrentSettings.ShowCrosshair, ( bool value ) =>
		{
			CurrentSettings.ShowCrosshair = value;
		} );

		CreateVolumeSlider( "Master Volume", CurrentSettings.VolumeMaster, ( float value ) =>
		{
			CurrentSettings.VolumeMaster = value;
			SettingsSaveData.SetVolume( "master", value );
		} );

		CreateVolumeSlider( "Effects Volume", CurrentSettings.VolumeEffects, ( float value ) =>
		{
			CurrentSettings.VolumeEffects = value;
			SettingsSaveData.SetVolume( "effects", value );
		} );

		CreateVolumeSlider( "Music Volume", CurrentSettings.VolumeMusic, ( float value ) =>
		{
			CurrentSettings.VolumeMusic = value;
			SettingsSaveData.SetVolume( "music", value );
		} );

		CreateVolumeSlider( "Ambient Volume", CurrentSettings.VolumeAmbience, ( float value ) =>
		{
			CurrentSettings.VolumeAmbience = value;
			SettingsSaveData.SetVolume( "ambience", value );
		} );

		CreateVolumeSlider( "Eating Volume", CurrentSettings.VolumeEating, ( float value ) =>
		{
			CurrentSettings.VolumeEating = value;
			SettingsSaveData.SetVolume( "eating", value );
		} );

		CreateVolumeSlider( "UI Volume", CurrentSettings.VolumeUI, ( float value ) =>
		{
			CurrentSettings.VolumeUI = value;
			SettingsSaveData.SetVolume( "ui", value );
		} );

		CreateSlider( "Render Scale", CurrentSettings.RenderScale, 0.1f, 2f, 0.1f, ( float value ) =>
		{
			CurrentSettings.RenderScale = value;
			SettingsSaveData.SetRenderScale( value );
		} );


		var applyButton = new Button();
		applyButton.Text = "Apply & Save";
		applyButton.Pressed += () =>
		{
			SettingsSaveData.CurrentSettings = CurrentSettings;
			SettingsSaveData.ApplySettings();
			SettingsSaveData.SaveSettings();
		};
		SettingsListContainer.AddChild( applyButton );

		var revertButton = new Button();
		revertButton.Text = "Revert";
		revertButton.Pressed += () =>
		{
			Revert();
			SettingsListContainer.QueueFreeAllChildren();
			AddControls();
		};
		SettingsListContainer.AddChild( revertButton );
	}

	private CheckBox CreateCheckBox( string text, bool defaultValue, Action<bool> onToggle )
	{
		Logger.Info( $"Creating checkbox for {text} with default value {defaultValue}" );
		var control = Loader.LoadResource<PackedScene>( "res://ui/settings/checkbox.tscn" ).Instantiate<CheckBox>();
		control.Text = text;
		control.SetPressedNoSignal( defaultValue );
		control.Toggled += ( bool value ) => onToggle( value );
		control.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		SettingsListContainer.AddChild( control );
		return control;
	}

	private Control CreateVolumeSlider( string text, float defaultValue, Action<float> onValueChanged )
	{
		var control = Loader.LoadResource<PackedScene>( "res://ui/settings/slider.tscn" ).Instantiate<Control>();

		var textLabel = control.GetNode<Label>( "%TextLabel" );
		var slider = control.GetNode<HSlider>( "%HSlider" );
		var valueLabel = control.GetNode<Label>( "%ValueLabel" );

		textLabel.Text = text;
		slider.MinValue = -50d;
		slider.MaxValue = 0d;
		slider.TickCount = 10;
		slider.Step = 0.05d;
		slider.SetValueNoSignal( defaultValue );

		valueLabel.Text = $"{defaultValue:0.00}";

		slider.ValueChanged += ( double value ) =>
		{
			onValueChanged( (float)value );
			valueLabel.Text = $"{value:0.00}";
		};

		SettingsListContainer.AddChild( control );

		return control;
	}

	private Control CreateSlider( string text, float defaultValue, float minValue, float maxValue, float step, Action<float> onValueChanged )
	{
		var control = Loader.LoadResource<PackedScene>( "res://ui/settings/slider.tscn" ).Instantiate<Control>();

		var textLabel = control.GetNode<Label>( "%TextLabel" );
		var slider = control.GetNode<HSlider>( "%HSlider" );
		var valueLabel = control.GetNode<Label>( "%ValueLabel" );

		textLabel.Text = text;
		slider.MinValue = minValue;
		slider.MaxValue = maxValue;
		slider.TickCount = (int)((maxValue - minValue) / step);
		slider.Step = step;
		slider.SetValueNoSignal( defaultValue );

		valueLabel.Text = $"{defaultValue:0.00}";

		slider.ValueChanged += ( double value ) =>
		{
			onValueChanged( (float)value );
			valueLabel.Text = $"{value:0.00}";
		};

		SettingsListContainer.AddChild( control );

		return control;
	}

}
