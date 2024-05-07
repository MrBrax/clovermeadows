using Godot;
using System;
using Godot.Collections;

namespace vcrossing;

public partial class World : Node3D
{

	public enum ItemPlacement
	{
		Wall,
		OnTop,
		Floor
	}
	
	public const int GridSize = 1;
	public const int GridWidth = 16;
	public const int GridHeight = 16;
	
	public Dictionary<Vector2I, Dictionary<ItemPlacement, WorldItem>> Items = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
