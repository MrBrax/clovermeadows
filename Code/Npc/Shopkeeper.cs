using System;
using vcrossing.Code.Items;
using vcrossing.Code.Player;
using vcrossing.Code.Save;
using YarnSpinnerGodot;

namespace vcrossing.Code.Npc;

public partial class Shopkeeper : BaseNpc
{


	public override void OnUse( PlayerController player )
	{
		TalkTo( player, "Shopkeeper" );
	}

	override protected void SetupDialogueRunner( PlayerController player, DialogueRunner runner )
	{
		runner.VariableStorage.SetValue( "$NpcName", GetData().NpcName );
		runner.VariableStorage.SetValue( "$PlayerName", player.PlayerName );

		runner.AddCommandHandler( "SellItemsToNpc", async () =>
		{

			var items = await player.Inventory.OpenItemPicker();

			runner.Stop();

			if ( items.Count == 0 )
			{
				runner.StartDialogue( "ShopkeeperNoItemsToSell" );
				return;
			}

			// Logger.Info( $"Player sold {items.Count} items to {GetData().NpcName}" );

			var totalValue = items.Sum( i => i.GetItem().ItemData.BaseSellPrice );

			runner.VariableStorage.SetValue( "$ItemsSold", items.Count );
			runner.VariableStorage.SetValue( "$ItemsSoldTotalClovers", totalValue );

			foreach ( var item in items )
			{
				item.Delete();
			}

			player.AddClovers( totalValue );

			runner.StartDialogue( "ShopkeeperItemsSold" );

		} );

	}


}
