using System.IO;
using System.Text.RegularExpressions;
using Godot.Collections;
using vcrossing.Code.Data;
using vcrossing.Code.Dependencies;
using vcrossing.Code.Helpers.ACNL;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Ui;

public partial class DebugMenu : PanelContainer
{
	[Export, Require] public Control ItemContainer;

	private Array<ItemData> Items { get; set; } = new();

	private IList<string> FilePaths( string path, string pattern = ".*" )
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
			fullPath = fullPath.Replace( ".import", "" ).Replace( ".remap", "" );

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
			var item = Loader.LoadResource<Resource>( path );

			if ( item is not ItemData itemData )
			{
				Logger.Warn( "DebugMenu", $"Failed to load item from {path}" );
				continue;
			}

			/* if ( itemData.DisablePickup )
			{
				Logger.Info( "DebugMenu", $"Skipping item {itemData.Name} because it's disabled" );
				continue;
			} */

			Items.Add( itemData );
		}
	}


	public override void _Ready()
	{
		base._Ready();

		LoadAllItems();

		foreach ( var itemData in Items.OrderBy( x => x.GetType().ToString() ).ThenBy( x => x.Name ) )
		{

			Logger.Info( "DebugMenu", $"Adding item {itemData.Name}" );

			var button = new Button
			{
				Text = !string.IsNullOrWhiteSpace( itemData.Name ) ? $"{itemData.Name}" : itemData.ResourcePath,
				Icon = itemData.GetIcon(),
				ExpandIcon = true,
				Alignment = Godot.HorizontalAlignment.Left
			};
			// button.Connect( "pressed", this, nameof( OnItemButtonPressed ), new Godot.Collections.Array { item } );
			button.Pressed += () => GiveItem( itemData );
			ItemContainer.AddChild( button );

		}
	}

	private void GiveItem( ItemData item )
	{
		Logger.Verbose( "DebugMenu", $"Giving item {item.Name} to player from debug menu" );

		var player = NodeManager.Player;
		// player.Inventory.AddItem( new PersistentItem( item ) );

		var persistentItem = PersistentItem.Create( item );
		persistentItem.Initialize();

		if ( persistentItem == null || persistentItem.ItemData == null ) throw new System.Exception( "Failed to create persistent item" );

		player.Inventory.PickUpItem( persistentItem );
	}

	public void Save()
	{
		var player = NodeManager.Player;
		player.Save();

		var world = NodeManager.WorldManager.ActiveWorld;
		world.Save();
	}

	/* public override void _Process( double delta )
	{
		if ( Input.IsActionJustPressed( "Debug" ) )
		{
			Visible = !Visible;
		}
	} */

	public override void _Input( InputEvent @event )
	{
		if ( @event.IsActionPressed( "Debug" ) )
		{
			Visible = !Visible;
		}
	}

	public void ImportFloorDecal()
	{
		DirAccess.MakeDirAbsolute( "user://designs" );
		DirAccess.MakeDirAbsolute( "user://designs/converted" );

		var fileDialog = new Godot.FileDialog();
		AddChild( fileDialog );
		fileDialog.Filters = ["*.png", "*.jpg", "*.jpeg", "*.acnl"];
		fileDialog.Access = FileDialog.AccessEnum.Userdata;
		fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
		fileDialog.RootSubfolder = "designs";
		fileDialog.Size = new Vector2I( 800, 600 );
		fileDialog.PopupCentered();

		fileDialog.FileSelected += ( string path ) =>
		{
			Logger.Info( "DebugMenu", $"Importing floor decal from {path}" );

			// var item = PersistentItem.Create( ResourceManager.Instance.LoadItemFromId<ItemData>( "floor_decal" ) ) as FloorDecal;
			var item = PersistentItem.Create<FloorDecal>( ItemData.GetById( "floor_sprite" ) );
			if ( item == null ) throw new System.Exception( "Failed to create floor decal" );

			if ( path.GetExtension().ToLower() == "acnl" )
			{
				var bytes = Godot.FileAccess.GetFileAsBytes( path );
				var acnl = new ACNLFormat( bytes );

				var image = acnl.GetImage();
				image.SavePng( $"user://designs/converted/{Path.GetFileNameWithoutExtension( path )}.png" );

				item.TexturePath = $"user://designs/converted/{Path.GetFileNameWithoutExtension( path )}.png";
			}
			else
			{
				item.TexturePath = path;
			}


			var player = GetNode<Player.PlayerController>( "/root/Main/Player" );
			player.Inventory.PickUpItem( item );
		};

	}
}
