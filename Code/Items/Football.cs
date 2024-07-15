using vcrossing.Code.Helpers;

namespace vcrossing.Code.Items;

public sealed partial class Football : RigidBody3D, IPushable
{
	public float PushForce { get; set; } = 200f;
	public bool PushOnce { get; set; } = true;

	public void OnPushed( Node3D byNode )
	{
		GetNode<AudioStreamPlayer3D>( "KickSound" ).Play();
	}

	/*public void OnBodyEntered( Node body )
	{
		GetNode<AudioStreamPlayer3D>( "KickSound" ).Play();
	}*/
}
