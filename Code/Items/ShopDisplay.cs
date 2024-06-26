using System;
using vcrossing.Code.Data;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public partial class ShopDisplay : Node3D, IUsable
{

	[Export] public ItemCategoryData Category { get; set; }

	[Export] public Node3D ModelContainer { get; set; }

	public ItemData CurrentItem { get; set; }

	public override void _Ready()
	{
		base._Ready();

		var item = Category.Items.PickRandom();

		CurrentItem = item;

		var itemInstance = item.PlaceScene.Instantiate<Node3D>();

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

		Logger.Warn( "Item does not have a model" );
	}


	public bool CanUse( PlayerController player )
	{
		return true;
	}

	public void OnUse( PlayerController player )
	{
		Logger.Info( $"Used shop display with item {CurrentItem.Name}" );
	}
}
