using System;
using Godot;
using vcrossing.Code.Data;

namespace vcrossing.Code;

public partial class BaseItem : Node3D
{

	[Export( PropertyHint.File, "*.tres" )]
	public string ItemDataPath { get; set; }
	[Export] public NodePath Model { get; set; }

	protected World World => GetNode<WorldManager>( "/root/Main/WorldManager" ).ActiveWorld;

	public ItemData ItemData;

	override public void _Ready()
	{
		LoadItemData();
	}

	protected void LoadItemData()
	{
		if ( string.IsNullOrEmpty( ItemDataPath ) ) throw new Exception( "ItemDataPath is null" );
		ItemData = Loader.LoadResource<ItemData>( ItemDataPath );
	}

}
