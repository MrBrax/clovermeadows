using System;
using vcrossing.Code.Components;

namespace vcrossing.Code.Data;

[GlobalClass]
public partial class SurfaceData : Resource
{

	[Export( PropertyHint.ResourceType, "AudioStream" )]
	public Godot.Collections.Array<string> FootstepSounds { get; set; }

	[Export] public string FootstepSoundPlayer { get; set; }

}
