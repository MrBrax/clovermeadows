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
		var pos = player.Interact.GetAimingGridPosition();

		var worldItems = player.World.GetItems( pos ).ToList();

		if ( worldItems.Count == 0 )
		{
			DigHole( pos );
			return;
		}
		else
		{
			var floorItem = worldItems.FirstOrDefault( x => x.Placement == World.ItemPlacement.Floor );
			if ( floorItem != null )
			{
				DigUpItem( pos, floorItem );
				return;
			}
		}
		
		GD.Print( "No action taken." );
	}
	
	private void DigHole( Vector2I pos )
	{
		GD.Print( $"Dug hole at {pos}" );

		var holeData = GD.Load<ItemData>( "res://items/misc/hole.tres" );
		var hole = Inventory.World.SpawnPlacedItem<Hole>( holeData, pos, World.ItemPlacement.Floor,
			World.ItemRotation.North );
	}
	
	private void DigUpItem( Vector2I pos, WorldItem item )
	{
		GD.Print( $"Dug up {item.GetItemData().Name} at {pos}" );
		// Inventory.World.RemoveItem( item );
	}
}
