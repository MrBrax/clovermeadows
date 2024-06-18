using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Components;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Player;

namespace vcrossing.Code.Persistence;

public partial class ClothingItem : PersistentItem
{

	[JsonIgnore] public Equips.EquipSlot EquipSlot => (ItemData as ClothingData).EquipSlot;

	public override Clothing Create()
	{
		var itemData = ItemData as ClothingData;

		if ( itemData.EquipScene == null )
		{
			throw new Exception( $"Carry scene not found for {ItemDataPath}" );
		}

		var scene = itemData.EquipScene.Instantiate<Clothing>();
		scene.ItemDataPath = ItemDataPath;
		scene.SceneFilePath = itemData.EquipScene.ResourcePath;
		SetNodeData( scene );
		return scene;
	}

}
