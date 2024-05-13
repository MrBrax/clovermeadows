using System.Linq;
using Godot;
using vcrossing2.Code.Items;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Carriable;

public partial class Shovel : BaseCarriable
{
	public override void OnEquip( PlayerController player )
	{
		GD.Print( "Equipped shovel." );
	}

	public override void OnUnequip( PlayerController player )
	{
		GD.Print( "Unequipped shovel." );
	}

	public override void OnUse( PlayerController player )
	{
		base.OnUse( player );
		
		var pos = player.Interact.GetAimingGridPosition();

		var worldItems = player.World.GetItems( pos ).ToList();

		if ( worldItems.Count == 0 )
		{
			DigHole( pos );
			return;
		}
		else
		{
			var undergroundItem = worldItems.FirstOrDefault( x => x.Placement == World.ItemPlacement.Underground );
			if ( undergroundItem != null )
			{
				DigUpItem( pos, undergroundItem );
				return;
			}
			
			var floorItem = worldItems.FirstOrDefault( x => x.Placement == World.ItemPlacement.Floor );
			if ( floorItem != null )
			{
				if ( floorItem is Hole hole )
				{
					FillHole( pos );
					return;
				}
				else
				{
					HitItem( pos, floorItem );
					return;
				}
			}
		}
		
		GD.PushWarning( "No action taken." );
	}

	private void HitItem( Vector2I pos, WorldItem floorItem )
	{
		GD.Print( $"Hit {floorItem.GetItemData().Name} at {pos}" );
	}

	private void DigHole( Vector2I pos )
	{
		GD.Print( $"Dug hole at {pos}" );

		var holeData = GD.Load<ItemData>( "res://items/misc/hole.tres" );
		var hole = Inventory.World.SpawnPlacedItem<Hole>( holeData, pos, World.ItemPlacement.Floor,
			World.ItemRotation.North );

		Inventory.World.Save();
	}
	
	private void FillHole( Vector2I pos )
	{
		GD.Print( $"Filled hole at {pos}" );
		
		var hole = Inventory.World.GetItem( pos, World.ItemPlacement.Floor );
		if ( hole == null )
		{
			GD.Print( "No hole found." );
			return;
		}
		
		if ( hole is Hole holeItem )
		{
			Inventory.World.RemoveItem( holeItem );
			Inventory.World.Save();
		}
		else
		{
			GD.PushWarning( "Not a hole." );
		}
		
		// TODO: check if hole has item in it
	}
	
	private void DigUpItem( Vector2I pos, WorldItem item )
	{
		GD.Print( $"Dug up {item.GetItemData().Name} at {pos}" );
		// Inventory.World.RemoveItem( item );
		
		// TODO: check if there are items in ground
	}
}
