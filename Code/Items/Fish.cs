using System;
using vcrossing2.Code.Items;
using vcrossing2.Code.Objects;

namespace vcrossing2.Code.Items;

public partial class Fish : Node3D, IWorldItem
{
	public bool IsPlacedInEditor { get; set; }
	public World.ItemPlacement Placement { get; set; }


	[Export( PropertyHint.File, "*.tres" )]
	public string ItemDataPath { get; set; }

	public bool ShouldBeSaved()
	{
		return true;
	}
}
