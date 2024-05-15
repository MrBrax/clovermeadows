using System.IO;
using System.Text.RegularExpressions;
using Godot.Collections;
using vcrossing2.Code.Dependencies;
using vcrossing2.Code.Helpers;
using vcrossing2.Code.Items;
using vcrossing2.Code.Persistence;

namespace vcrossing2.Code.Ui;

public partial class DebugMenu : PanelContainer
{
	[Export, Require] public GridContainer ItemContainer;

	private Array<ItemData> Items { get; set; } = new();

	private List<string> FilePaths( string path, string pattern = ".*" )
	{
		var files = new List<string>();
		var dir = DirAccess.Open( path );
		dir.ListDirBegin();
		while ( true )
		{
			var file = dir.GetNext();

			// break out of loop if no more files
			if ( file == "" )
			{
				break;
			}

			var fullPath = $"{path}/{file}";

			// recursively search directories
			if ( dir.CurrentIsDir() )
			{
				files.AddRange( FilePaths( fullPath, pattern ) );
			}
			else
			{
				// skip files that don't match pattern
				if ( Regex.IsMatch( file, pattern ) == false )
				{
					// Logger.Info( $"Skipping {fullPath}, didn't match {pattern}" );
					continue;
				}

				// Logger.Info( $"Adding {fullPath}, matched {pattern}" );
				files.Add( fullPath );
			}
		}

		dir.ListDirEnd();
		return files;
	}

	private void LoadAllItems()
	{
		var paths = FilePaths( "res://items", ".*\\.tres" );
		foreach ( var path in paths )
		{
			var item = GD.Load<Resource>( path );

			if ( item is not ItemData itemData )
			{
				Logger.Warn( $"Failed to load item from {path}" );
				continue;
			}

			if ( itemData.DisablePickup ) continue;

			Items.Add( itemData );
		}
	}


	public override void _Ready()
	{
		base._Ready();

		LoadAllItems();

		foreach ( var itemData in Items )
		{
			
			var button = new Button
			{
				Text = $"{itemData.Name}",
			};
			// button.Connect( "pressed", this, nameof( OnItemButtonPressed ), new Godot.Collections.Array { item } );
			button.Pressed += () => GiveItem( itemData );
			ItemContainer.AddChild( button );
			
		}
	}

	private void GiveItem( ItemData item )
	{
		var player = GetNode<Player.PlayerController>( "/root/Main/Player" );
		player.Inventory.AddItem( new PersistentItem( item ) );
	}
}
