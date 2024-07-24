#if TOOLS
using Godot;
using System;
using vcrossing.Code.Data;

namespace vcrossing.Code;

[Tool]
public partial class Plugin : EditorPlugin
{

	AreaExitGizmoPlugin _areaExitGizmoPlugin = new AreaExitGizmoPlugin();

	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		AddNode3DGizmoPlugin( _areaExitGizmoPlugin );

		CreateItems();
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveNode3DGizmoPlugin( _areaExitGizmoPlugin );
	}

	private void CreateItems()
	{
		/* GD.Print( "Creating items..." );

		var itemStubNames = new string[]
		{
			"Carrot",
			"Broccoli",
			"Tomato",
			"Potato",
			// "Apple",
			// "Banana",
			// "Orange",
			"Onion",
			"Garlic",
			"Flour",
			"Salt",
			"Sugar",
			"Pepper",
			// "OliveOil",
			// "Vinegar",
			"Cheese",
			"Rice",
			"Pasta",
			"Butter",
			"Milk",
			"Egg",
			"Avocado",
			"Chicken",
			"Beef",
			"Pork",
			// "Fish",
			// "Shrimp",
			"Eggplant",
			"Zucchini",
			"Spinach",
			"Peas",
			"Beans",
			"Chickpeas",
			"Lentils",
			"Quinoa",
			"Raisins",
			"Almonds",
			// "ChiaSeeds",
			// "SunflowerSeeds",
			// "PumpkinSeeds",
			"Walnuts",
			"Oats",
			"Pickles",
			"Cucumber",
		};

		foreach ( var itemStubName in itemStubNames )
		{

			var path = $"res://items/ingredients/{itemStubName.ToLower()}/{itemStubName.ToLower()}.tres";

			DirAccess.MakeDirAbsolute( $"res://items/ingredients/{itemStubName.ToLower()}" );

			var itemData = new IngredientData();
			itemData.Id = $"ingredient_{itemStubName.ToLower()}";
			itemData.Name = itemStubName;
			itemData.Icon = GD.Load<CompressedTexture2D>( $"res://icons/default_ingredient.png" );

			if ( ResourceSaver.Save( itemData, path ) != Error.Ok )
			{
				GD.PrintErr( $"Failed to save item data to {path}" );
			}

		} */
	}
}
#endif
