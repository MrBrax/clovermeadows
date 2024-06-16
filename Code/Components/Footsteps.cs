using System;

namespace vcrossing.Code.Components;

public partial class Footsteps : Node3D
{

	// [Export] public AudioStreamPlayer3D FootstepPlayer { get; set; }

	// [Export] public Godot.Collections.Array<AudioStream> FootstepSounds { get; set; }

	public void PlayFootstep()
	{

		// GetChild<AudioStreamPlayer3D>( 0 ).Play();
		// GetChild<AudioStreamPlayer3D>( 0 ).PitchScale = 0.8f + GD.Randf() * 0.4f;

		var state = GetWorld3D().DirectSpaceState;
		var query = new Trace( state ).CastRay( PhysicsRayQueryParameters3D.Create( GlobalTransform.Origin,
			GlobalTransform.Origin + Vector3.Down ) );

		if ( query == null ) return;

		var parent = query.Collider.GetParent();

		var groups = parent.GetGroups();

		var surface_group = groups.FirstOrDefault( g => g.ToString().StartsWith( "surface_" ) ).ToString();

		if ( string.IsNullOrEmpty( surface_group ) )
		{
			surface_group = "surface_grass";
		}

		surface_group = surface_group.Substring( 8 ); // remove "surface_"

		surface_group = surface_group.Capitalize();

		var playerNode = GetNodeOrNull<AudioStreamPlayer3D>( surface_group );

		if ( playerNode == null )
		{
			Logger.Warn( $"No AudioStreamPlayer3D found for surface group {surface_group}" );
			playerNode = GetNode<AudioStreamPlayer3D>( "Grass" );
		}

		playerNode.Play();
		playerNode.PitchScale = 0.8f + GD.Randf() * 0.4f;

		/* if ( query.Collider is StaticBody3D staticBody )
		{
			staticBody.
		} */
	}

	/* public void PlayFootstepRight()
	{
		FootstepPlayer.Play();
	} */

}
