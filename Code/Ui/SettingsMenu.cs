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

		CreateCheckBox( "SSIL", CurrentSettings.SSIL, ( bool value ) =>
		{
			CurrentSettings.SSIL = value;
			SettingsSaveData.SetSSIL( value );
		} );

		CreateCheckBox( "SSAO", CurrentSettings.SSAO, ( bool value ) =>
		{
			CurrentSettings.SSAO = value;
			SettingsSaveData.SetSSAO( value );
		} );

		/* CreateCheckBox( "Player Mouse Control", CurrentSettings.PlayerMouseControl, ( bool value ) =>
		{
			CurrentSettings.PlayerMouseControl = value;
		} ); */

		CreateCheckBox( "Show Touch Controls", CurrentSettings.ShowTouchControls, ( bool value ) =>
		{
			CurrentSettings.ShowTouchControls = value;
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
		var control = new CheckBox();
		control.Text = text;
		control.SetPressedNoSignal( defaultValue );
		control.Toggled += ( bool value ) => onToggle( value );
		control.SizeFlagsHorizontal = SizeFlags.ExpandFill;
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
		control.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		container.AddChild( control );

		return control;
	}

	private Control CreateSlider( string text, float defaultValue, float minValue, float maxValue, float step, Action<float> onValueChanged )
	{
		var container = new VBoxContainer();
		SettingsListContainer.AddChild( container );

		var label = new Label();
		label.Text = text;
		container.AddChild( label );

		var control = new HSlider
		{
			MinValue = minValue,
			MaxValue = maxValue,
			TickCount = (int)((maxValue - minValue) / step),
			Step = step,
		};
		control.SetValueNoSignal( defaultValue );
		control.ValueChanged += ( double value ) => onValueChanged( (float)value );
		control.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		container.AddChild( control );

		return control;
	}

	/* public override void _Ready()
	{
		base._Ready();

		SettingsSaveData.LoadSettings();

		CreateCheckBox( "Fullscreen", SettingsSaveData.CurrentSettings.Fullscreen, ( bool value ) => SettingsSaveData.SetFullscreen( value, true ) );
		CreateCheckBox( "VSync", SettingsSaveData.CurrentSettings.VSync, ( bool value ) => SettingsSaveData.SetVSync( value, true ) );
		// CreateCheckBox( "Borderless", ( bool value ) => DisplayServer.WindowSetMode( DisplayServer.WindowMode.ExclusiveFullscreen)
		CreateCheckBox( "SSIL", SettingsSaveData.CurrentSettings.SSIL, ( bool value ) => SettingsSaveData.SetSSIL( value, true ) );
		CreateCheckBox( "SSAO", SettingsSaveData.CurrentSettings.SSAO, ( bool value ) => SettingsSaveData.SetSSAO( value, true ) );
		CreateCheckBox( "Player Mouse Control", SettingsSaveData.CurrentSettings.PlayerMouseControl, ( bool value ) => SettingsSaveData.SetPlayerMouseControl( value, true ) );
		CreateCheckBox( "Show Touch Controls", SettingsSaveData.CurrentSettings.ShowTouchControls, ( bool value ) => SettingsSaveData.SetShowTouchControls( value, true ) );

		CreateVolumeSlider( "Master Volume", SettingsSaveData.CurrentSettings.VolumeMaster, ( float value ) => SettingsSaveData.SetVolume( "master", value, true ) );
		CreateVolumeSlider( "Effects Volume", SettingsSaveData.CurrentSettings.VolumeEffects, ( float value ) => SettingsSaveData.SetVolume( "effects", value, true ) );
		CreateVolumeSlider( "Music Volume", SettingsSaveData.CurrentSettings.VolumeMusic, ( float value ) => SettingsSaveData.SetVolume( "music", value, true ) );
		CreateVolumeSlider( "Ambient Volume", SettingsSaveData.CurrentSettings.VolumeAmbience, ( float value ) => SettingsSaveData.SetVolume( "ambience", value, true ) );
		CreateVolumeSlider( "Eating Volume", SettingsSaveData.CurrentSettings.VolumeEating, ( float value ) => SettingsSaveData.SetVolume( "eating", value, true ) );
		CreateVolumeSlider( "UI Volume", SettingsSaveData.CurrentSettings.VolumeUI, ( float value ) => SettingsSaveData.SetVolume( "ui", value, true ) );

		CreateSlider( "Render Scale", SettingsSaveData.CurrentSettings.RenderScale, 0.1f, 2f, 0.1f, ( float value ) => SettingsSaveData.SetRenderScale( value, true ) );

		var scalingModeContainer = new VBoxContainer();
		SettingsListContainer.AddChild( scalingModeContainer );
		scalingModeContainer.AddChild( new Label { Text = "Scaling Mode" } );

		var scalingModeDropdown = new OptionButton();
		foreach ( var val in Enum.GetValues( typeof( Viewport.Scaling3DModeEnum ) ) )
		{
			var value = (Viewport.Scaling3DModeEnum)val;
			scalingModeDropdown.AddItem( value.ToString(), (int)value );
		}
		scalingModeDropdown.ItemSelected += ( long index ) =>
		{
			SettingsSaveData.SetRenderMode( (Viewport.Scaling3DModeEnum)index, true );
		};
		scalingModeDropdown.Select( (int)SettingsSaveData.CurrentSettings.Scaling3DMode );
		scalingModeContainer.AddChild( scalingModeDropdown );

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

	private Control CreateSlider( string text, float defaultValue, float minValue, float maxValue, float step, Action<float> onValueChanged )
	{
		var container = new VBoxContainer();
		SettingsListContainer.AddChild( container );

		var label = new Label();
		label.Text = text;
		container.AddChild( label );

		var control = new HSlider
		{
			MinValue = minValue,
			MaxValue = maxValue,
			TickCount = (int)((maxValue - minValue) / step),
			Step = step,
		};
		control.SetValueNoSignal( defaultValue );
		control.ValueChanged += ( double value ) => onValueChanged( (float)value );
		container.AddChild( control );

		return control;
	} */

}
