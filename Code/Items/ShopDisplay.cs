using System;
using vcrossing.Code.Data;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public partial class ShopDisplay : Node3D, IUsable
{

	[Export] public ItemCategoryData Category { get; set; }

	[Export] public Node3D ModelContainer { get; set; }

	public ItemData CurrentItem { get; set; }

	public bool IsBought { get; set; }

	public override void _Ready()
	{
		base._Ready();

		var item = Category.Items.PickRandom();

		CurrentItem = item;

		SpawnModel();

		Logger.Warn( "Item does not have a model" );
	}

	private void SpawnModel()
	{

		if ( CurrentItem == null ) throw new Exception( "No item to spawn" );

		var itemInstance = CurrentItem.PlaceScene.Instantiate<Node3D>();

		if ( itemInstance is BaseItem baseItem )
		{
			var model = baseItem.Model;
			if ( model != null )
			{
				var modelNode = itemInstance.GetNode<Node3D>( model );
				modelNode.GetParent().RemoveChild( modelNode );
				ModelContainer.AddChild( modelNode );
				itemInstance.QueueFree();
				Logger.Info( $"Added model {modelNode.Name} to shop display" );
				return;
			}
		}
	}


	public bool CanUse( PlayerController player )
	{
		return true;
	}

	public void OnUse( PlayerController player )
	{
		Logger.Info( $"Used shop display with item {CurrentItem.Name}" );

		if ( IsBought ) return;
		IsBought = true;

		var item = PersistentItem.Create( CurrentItem );
		player.Inventory.PickUpItem( item );

		ModelContainer.GetChildren().FirstOrDefault()?.QueueFree();
	}
}
