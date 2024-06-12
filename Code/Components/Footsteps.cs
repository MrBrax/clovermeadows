using System;

namespace vcrossing.Code.Components;

public partial class Footsteps : Node3D
{

	// [Export] public AudioStreamPlayer3D FootstepPlayer { get; set; }

	// [Export] public Godot.Collections.Array<AudioStream> FootstepSounds { get; set; }

	public void PlayFootstep()
	{
		// Stream = FootstepSounds.PickRandom();
		// Play();
		GetChild<AudioStreamPlayer3D>( 0 ).Play();
		GetChild<AudioStreamPlayer3D>( 0 ).PitchScale = 0.8f + GD.Randf() * 0.4f;
	}

	/* public void PlayFootstepRight()
	{
		FootstepPlayer.Play();
	} */

}
