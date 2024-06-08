using System.Linq;
using Godot;
using vcrossing.Code.Helpers;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public partial class PlacedItem : WorldItem, IUsable
{
	public SittableNode[] SittableNodes => GetChildren().Where( x => x is SittableNode ).Cast<SittableNode>().ToArray();
	public bool IsSittable => SittableNodes.Length > 0;

	public LyingNode[] LyingNodes => GetChildren().Where( x => x is LyingNode ).Cast<LyingNode>().ToArray();

	// TODO: rename
	public bool IsLying => LyingNodes.Length > 0;

	public override bool CanBePickedUp()
	{
		return !SittableNodes.Any( x => x.IsOccupied ) && !LyingNodes.Any( x => x.IsOccupied ) && base.CanBePickedUp();
	}


	public void OnUse( PlayerController player )
	{
		Logger.Info( $"Player used {ItemData.Name}" );
		foreach ( var testNode in FindChildren( "*", "SittableNode" ) )
		{
			Logger.Info( testNode );
		}

		if ( IsSittable )
		{
			var sittableNode = SittableNodes.Where( x => !x.IsOccupied )
				.MinBy( x => x.GlobalPosition.DistanceTo( player.GlobalPosition ) );

			if ( !IsInstanceValid( sittableNode ) )
			{
				Logger.Info( "No sittable nodes available" );
				return;
			}

			player.Interact.Sit( sittableNode );
			return;
		}
		else if ( IsLying )
		{
			var lyingNode = LyingNodes.Where( x => !x.IsOccupied )
				.MinBy( x => x.GlobalPosition.DistanceTo( player.GlobalPosition ) );

			if ( !IsInstanceValid( lyingNode ) )
			{
				Logger.Info( "No lying nodes available" );
				return;
			}

			player.Interact.Lie( lyingNode );
			return;
		}

		Logger.Info( $"{player} used {ItemData.Name} at {GlobalPosition} but no action was taken." );
	}

	public bool CanUse( PlayerController player )
	{
		return true;
	}
}
