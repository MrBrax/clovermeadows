using System;
using Godot;
using vcrossing.Code.Data;
using vcrossing.Code.Interfaces;
using vcrossing.Code.WorldBuilder;

namespace vcrossing.Code;

public partial class BaseItem : Node3D, IDataPath
{

	[Export( PropertyHint.File, "*.tres" )]
	public string ItemDataPath { get; set; }
	public string ItemDataId { get; set; }

	/// <summary>
	///  The model used for the item.
	/// </summary>
	[Export] public Node3D Model { get; set; }

	protected World World => GetNode<WorldManager>( "/root/Main/WorldManager" ).ActiveWorld;

	protected DateTime TimeNow => GetNode<TimeManager>( "/root/Main/TimeManager" ).Time;

	public ItemData ItemData;

	override public void _Ready()
	{
		LoadItemData();
	}

	protected void LoadItemData()
	{
		if ( string.IsNullOrWhiteSpace( ItemDataPath ) ) throw new Exception( "ItemDataPath is null" );
		ItemData = Loader.LoadResource<ItemData>( ItemDataPath );
	}

}
