using System;
using System.Threading.Tasks;
using vcrossing.Code.Data;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Inventory;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;
using static vcrossing.Code.Data.ShopInventoryData;

namespace vcrossing.Code.Ui;

public partial class ItemPicker : Control, IStopInput
{

	[Export, Require] public GridContainer InventoryGrid;

	[Export, Require] public PackedScene InventorySlotButtonScene;

	[Export] public Button OkButton;
	[Export] public Button CancelButton;

	private int MaxItems = 1;

	// private List<InventorySlot<PersistentItem>> SelectedItems = new();
	private Godot.Collections.Array<int> SelectedItemIndexes = new();

	[Signal]
	public delegate void ItemsPickedEventHandler( Godot.Collections.Array<int> containerItemIndexes );

	// [Signal]
	// public delegate void ItemsCancelledEventHandler();

	public override void _Ready()
	{
		base._Ready();
		Hide();
	}

	public async Task<IList<InventorySlot<PersistentItem>>> PickItemAsync( InventoryContainer container, int maxItems = 1 )
	{

		MaxItems = maxItems;
		SelectedItemIndexes.Clear();

		InventoryGrid.QueueFreeAllChildren();

		OkButton.Disabled = SelectedItemIndexes.Count == 0;

		foreach ( var entry in container.GetEnumerator() )
		{

			var itemButton = InventorySlotButtonScene.Instantiate<Button>();

			// itemButton.Index = entry.Index;
			// itemButton.Slot = entry.HasSlot ? entry.Slot : null;
			itemButton.Name = $"InventorySlotButton{entry.Index}";

			Logger.Info( $"ItemPicker: {entry.Index} {entry.HasSlot} {entry.Slot?.GetName()}" );

			if ( entry.HasSlot )
			{
				// itemButton.Text = entry.Slot.GetName();
				itemButton.Icon = entry.Slot.GetIconTexture();

				itemButton.Pressed += () =>
				{

					var state = itemButton.ButtonPressed;

					if ( state )
					{

						if ( SelectedItemIndexes.Count >= MaxItems )
						{
							itemButton.SetPressedNoSignal( false );
							// TODO: play sound
							return;
						}

						SelectedItemIndexes.Add( entry.Index );

					}
					else
					{
						SelectedItemIndexes.Remove( entry.Index );
					}

					OkButton.Disabled = SelectedItemIndexes.Count == 0;

				};

			}
			else
			{
				itemButton.Disabled = true;
				itemButton.Icon = null;
			}

			InventoryGrid.AddChild( itemButton );

		}

		Show();

		var val = await ToSignal( this, SignalName.ItemsPicked );

		GD.Print( val );

		Hide();

		if ( SelectedItemIndexes.Count == 0 )
		{
			// EmitSignal( SignalName.ItemsCancelled );
			return null;
		}

		var selectedItems = new List<InventorySlot<PersistentItem>>();

		foreach ( var index in SelectedItemIndexes )
		{
			selectedItems.Add( container.GetSlotByIndex( index ) );
		}

		return selectedItems;

	}

	public void Cancel()
	{
		// EmitSignal( SignalName.ItemsCancelled );
		EmitSignal( SignalName.ItemsPicked, new Godot.Collections.Array<int>() );
	}

	public void Confirm()
	{
		EmitSignal( SignalName.ItemsPicked, SelectedItemIndexes );
	}

}
