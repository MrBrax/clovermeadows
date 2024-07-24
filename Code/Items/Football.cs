using vcrossing.Code.Interfaces;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public sealed partial class Football : RigidBody3D, IPushable, IPickupable
{
	public float PushForce { get; set; } = 200f;
	public bool PushOnce { get; set; } = true;

	public bool CanPickup( PlayerController player )
	{
		return true;
	}

	public void OnPickup( PlayerController player )
	{

		var ball = PersistentItem.Create( this );

		player.Inventory.PickUpItem( ball );

		QueueFree();

	}


	public void OnPushed( Node3D byNode )
	{
		GetNode<AudioStreamPlayer3D>( "KickSound" ).Play();
	}

	/*public void OnBodyEntered( Node body )
	{
		GetNode<AudioStreamPlayer3D>( "KickSound" ).Play();
	}*/
}
