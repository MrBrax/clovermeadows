using System;

namespace vcrossing.Code.Components;

public partial class Footsteps : AudioStreamPlayer3D
{

    // [Export] public AudioStreamPlayer3D FootstepPlayer { get; set; }

    [Export] public Godot.Collections.Array<AudioStream> FootstepSounds { get; set; }

    public void PlayFootstep()
    {
        Stream = FootstepSounds.PickRandom();
        Play();
    }

    /* public void PlayFootstepRight()
	{
		FootstepPlayer.Play();
	} */

}
