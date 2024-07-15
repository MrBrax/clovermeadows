using System;
using vcrossing.Code.Data;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder;
using YarnSpinnerGodot;
using static vcrossing.Code.Data.ShopInventoryData;

namespace vcrossing.Code.Items;

public sealed partial class ShopDisplay : Node3D, IUsable
{

	[Export] public string ShopId { get; set; }

	// [Export, ExportGroup( "Item" )] public int ItemIndex { get; set; }
	[Export, ExportGroup( "Item" )] public bool StaticItem { get; set; }

	// [Export] public ItemCategoryData Category { get; set; }

	[Export] public Node3D ModelContainer { get; set; }

	[Export] public Node3D ShopSoldOutSign { get; set; }

	[Export] public int TileSize { get; set; } = 1;

	public StoreManager StoreManager;


	public ItemData CurrentItem => Item?.ItemData;

	// public bool IsBought { get; set; }


	public void SpawnModel()
	{

		Logger.Info( "ShopDisplay", $"Spawning model for shop display {Name}" );

		ModelContainer.GetChildren().FirstOrDefault()?.QueueFree();

		if ( !HasItem || !IsInStock )
		{
			Logger.Info( $"ShopDisplay", $"Display {Name} item {Item?.ItemDataName}: {HasItem}, Stock: {IsInStock}" );
			ShopSoldOutSign.Visible = true;
			return;
		}

		ShopSoldOutSign.Visible = false;

		// CurrentItem = ResourceLoader.Load<ItemData>( Item.ItemDataPath );

		if ( CurrentItem == null ) throw new Exception( "No item to spawn" );

		if ( CurrentItem.Width > TileSize || CurrentItem.Height > TileSize )
		{
			Logger.Warn( $"Item {CurrentItem.Name} is too big for shop display {Name}" );
		}

		Node3D itemInstance;

		if ( CurrentItem.PlaceScene != null )
		{
			itemInstance = CurrentItem.PlaceScene.Instantiate<Node3D>();
		}
		else if ( CurrentItem.DropScene != null )
		{
			itemInstance = CurrentItem.DropScene.Instantiate<Node3D>();
		}
		else
		{
			itemInstance = CurrentItem.DefaultTypeScene.Instantiate<Node3D>();
		}

		if ( itemInstance == null ) throw new Exception( $"Failed to instantiate item: {CurrentItem.ResourcePath}" );

		Logger.Info( $"ShopDisplay", $"Display {Name} spawned model node {itemInstance.Name} for item {CurrentItem.Name}" );

		if ( itemInstance is BaseItem baseItem )
		{
			var model = baseItem.Model;
			if ( model != null )
			{
				model.Owner = null;
				model.GetParent().RemoveChild( model );
				ModelContainer.AddChild( model );
				// model.Reparent( ModelContainer, true );
				model.Owner = ModelContainer;
				// model.Position = Vector3.Zero;
				itemInstance.QueueFree();
				Logger.Info( $"ShopDisplay", $"Added model {model.Name} to shop display {Name}" );
				return;
			}
			else
			{
				Logger.Warn( $"ShopDisplay", $"Item {CurrentItem.Name} does not have a model" );
			}
		}
		else if ( itemInstance is Carriable.BaseCarriable baseCarriable )
		{
			var model = baseCarriable.Model;
			if ( model != null )
			{
				model.Owner = null;
				model.GetParent().RemoveChild( model );
				ModelContainer.AddChild( model );
				model.Reparent( ModelContainer, true );
				model.Owner = ModelContainer;
				// model.Position = Vector3.Zero;
				itemInstance.QueueFree();
				Logger.Info( $"ShopDisplay", $"Added model {model.Name} to shop display {Name}" );
				return;
			}
			else
			{
				Logger.Warn( $"ShopDisplay", $"Item {CurrentItem.Name} does not have a model" );
			}
		}

		Logger.Warn( $"ShopDisplay", $"Item {CurrentItem.Name} does not have a model" );
		itemInstance.QueueFree();
	}

	public bool CanDisplayItem( ItemData item )
	{
		return item.Width <= TileSize && item.Height <= TileSize;
	}

	public bool HasItem
	{
		get
		{
			return Item != null;
		}
	}

	public ShopItem Item;

	/* private ShopItem Item
	{
		get
		{
			var main = GetNode<MainGame>( "/root/Main" );
			var shop = main.Shops[ShopId];

			if ( !StaticItem )
			{
				if ( ItemIndex >= shop.Items.Count )
				{
					Logger.Warn( $"Item index {ItemIndex} is out of range for shop {ShopId}" );
					return null;
				}
				return shop.Items[ItemIndex];
			}
			else
			{
				if ( ItemIndex >= shop.StaticItems.Count )
				{
					Logger.Warn( $"Static item index {ItemIndex} is out of range for shop {ShopId}" );
					return null;
				}
				return shop.StaticItems[ItemIndex];
			}
		}
	} */

	private bool IsInStock
	{
		get
		{
			return Item?.Stock > 0;
		}
	}

	public bool CanUse( PlayerController player )
	{
		return HasItem && IsInStock;
	}

	public void OnUse( PlayerController player )
	{
		Logger.Info( $"Used shop display with item {CurrentItem.Name}" );

		var runner = GetNode<DialogueRunner>( "/root/Main/UserInterface/YarnSpinnerCanvasLayer/DialogueRunner" );

		runner.VariableStorage.SetValue( "$NpcName", StoreManager.Shopkeeper.GetData()?.NpcName );
		runner.VariableStorage.SetValue( "$ItemName", CurrentItem.Name );
		runner.VariableStorage.SetValue( "$ItemPrice", Item.Price );
		runner.VariableStorage.SetValue( "$PlayerClovers", player.Clovers );

		runner.AddCommandHandler( "DoBuyItem", () => BuyItem( player ) );

		runner.StartDialogue( "BuyItem" );

		runner.onDialogueComplete += () =>
		{
			runner.RemoveCommandHandler( "DoBuyItem" );
		};

	}

	protected void BuyItem( PlayerController player )
	{
		if ( !player.CanAfford( Item.Price ) )
		{
			Logger.Info( $"Player cannot afford item {CurrentItem.Name}" );
			return;
		}

		Item.Stock--;

		var item = PersistentItem.Create( CurrentItem );
		player.Inventory.PickUpItem( item );

		player.SpendClovers( Item.Price );

		player.Save();

		GetNode<AudioStreamPlayer3D>( "ItemSold" ).Play();

		var poof = Loader.LoadResource<PackedScene>( "res://particles/poof.tscn" ).Instantiate<GpuParticles3D>();
		GetTree().CurrentScene.AddChild( poof );
		poof.GlobalPosition = ModelContainer.GlobalPosition;

		SpawnModel();

		// GetNode<MainGame>( "/root/Main" ).SaveShops();
	}
}
