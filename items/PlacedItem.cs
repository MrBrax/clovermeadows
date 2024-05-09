using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using vcrossing.items;
using vcrossing.Player;

namespace vcrossing;

public partial class PlacedItem : WorldItem
{

	public SittableNode[] SittableNodes => GetChildren().Where( x => x is SittableNode ).Cast<SittableNode>().ToArray();
	
	public bool IsSittable => SittableNodes.Length > 0;


	public override void OnPlayerUse( PlayerInteract playerInteract, Vector2I pos )
	{
		GD.Print( "Player used " + GetItemData().Name );
		foreach ( var testNode in FindChildren( "*", "SittableNode" ) )
		{
			GD.Print( testNode );
		}
		
		if ( IsSittable )
		{
			var sittableNode = SittableNodes.Where( x => !x.IsOccupied )
				.MinBy( x => x.GlobalPosition.DistanceTo( playerInteract.GlobalPosition ) );

			if ( sittableNode == null )
			{
				GD.Print( "No sittable nodes available" );
				return;
			}

			playerInteract.Sit( sittableNode );
			return;

		}
		
		GD.Print( $"{nameof(Player)} used " + GetItemData().Name + " at " + pos + " but no action was taken." );
	}
}
