using Godot;
using vcrossing2.Code.Carriable;

namespace vcrossing2.Code.Objects;

public partial class FishingBobber : Node3D
{

	public FishingRod Rod { get; set; }

	public override void _Ready()
	{
		AddToGroup( "fishing_bobber" );

		GetNode<AudioStreamPlayer3D>( "BobberWater" ).Play();
	}

}
