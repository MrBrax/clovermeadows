using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

[GlobalClass]
public partial class Furniture : WorldItem, IUsable
{

	public bool LightState { get; set; } = false;

	[Export] public Godot.Collections.Array<Light3D> Lights { get; set; } = new();

	[Export] public AudioStreamPlayer3D UseSoundPlayer { get; set; }

	[Export] public AnimationPlayer AnimationPlayer { get; set; }

	public override void _Ready()
	{
		base._Ready();
		SetLightState( LightState );
	}

	public void SetLightState( bool state )
	{
		LightState = state;
		foreach ( var light in Lights )
		{
			light.Visible = state;
		}
	}

	public bool CanUse( PlayerController player )
	{
		return true;
	}

	public void OnUse( PlayerController player )
	{
		if ( ItemData is not FurnitureData furnitureData ) return;

		if ( furnitureData.CanToggleLight )
		{
			SetLightState( !LightState );
			UseSoundPlayer?.Play();
		}
		else if ( furnitureData.CanPlayAnimation )
		{
			AnimationPlayer.Play( furnitureData.AnimationName );
			UseSoundPlayer?.Play();
		}
	}
}
