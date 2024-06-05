using System;
using vcrossing.Code.Items;
using vcrossing.Code.Objects;

namespace vcrossing.Code.Items;

public partial class Fish : Node3D, IWorldItem
{
	public bool IsPlacedInEditor { get; set; }
	public World.ItemPlacement Placement { get; set; }


	[Export( PropertyHint.File, "*.tres" )]
	public string ItemDataPath { get; set; }

	[Export] public PathFollow3D Path { get; set; }
	[Export] public float Speed { get; set; } = 1.0f;

	public bool ShouldBeSaved()
	{
		return true;
	}

	public override void _Process( double delta )
	{
		if ( IsInstanceValid( Path ) )
		{
			Path.Progress += (float)delta * Speed;
		}
	}


}
