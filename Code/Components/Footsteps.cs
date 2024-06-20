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

		if ( parent == null )
		{
			Logger.Warn( $"No parent found for {query.Collider}" );
			return;
		}

		if ( parent is WorldMesh worldMesh )
		{
			var surface = worldMesh.Surface;
			if ( surface == null )
			{
				Logger.Warn( $"No SurfaceData found for {worldMesh}" );
				return;
			}

			if ( !string.IsNullOrEmpty( surface.FootstepSoundPlayer ) )
			{
				var playerNode1 = GetNodeOrNull<AudioStreamPlayer3D>( surface.FootstepSoundPlayer );

				if ( playerNode1 == null )
				{
					Logger.Warn( $"No AudioStreamPlayer3D found for {surface.FootstepSoundPlayer}" );
					return;
				}

				playerNode1.Play();
				playerNode1.PitchScale = 0.8f + GD.Randf() * 0.4f;
				return;
			}

			if ( surface.FootstepSounds.Count == 0 )
			{
				Logger.Warn( $"No FootstepSounds found for {surface}" );
				return;
			}

			Logger.Warn( $"No FootstepSoundPlayer found for {surface}" );

			return;
		}

		var groups = parent.GetGroups();

		if ( groups.Count == 0 )
		{
			Logger.Warn( $"No groups found for {parent}" );
			return;
		}

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
