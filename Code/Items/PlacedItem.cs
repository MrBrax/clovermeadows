using System.Linq;
using Godot;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Items;

public partial class PlacedItem : WorldItem
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


	public override void OnPlayerUse( PlayerInteract playerInteract, Vector2I pos )
	{
		Logger.Info( $"Player used {GetItemData().Name}" );
		foreach ( var testNode in FindChildren( "*", "SittableNode" ) )
		{
			Logger.Info( testNode );
		}
		
		if ( IsSittable )
		{
			var sittableNode = SittableNodes.Where( x => !x.IsOccupied )
				.MinBy( x => x.GlobalPosition.DistanceTo( playerInteract.GlobalPosition ) );

			if ( sittableNode == null )
			{
				Logger.Info( "No sittable nodes available" );
				return;
			}

			playerInteract.Sit( sittableNode );
			return;

		} else if ( IsLying )
		{
			var lyingNode = LyingNodes.Where( x => !x.IsOccupied )
				.MinBy( x => x.GlobalPosition.DistanceTo( playerInteract.GlobalPosition ) );

			if ( lyingNode == null )
			{
				Logger.Info( "No lying nodes available" );
				return;
			}

			playerInteract.Lie( lyingNode );
			return;
		}
		
		Logger.Info( $"{playerInteract} used " + GetItemData().Name + " at " + pos + " but no action was taken." );
	}
}
