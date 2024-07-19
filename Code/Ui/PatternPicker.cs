using System;
using System.IO;
using vcrossing.Code.Carriable;
using vcrossing.Code.Data;
using vcrossing.Code.Helpers.ACNL;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Ui;

public partial class PatternPicker : Window
{

	[Export] public FileDialog FileDialog;
	[Export] public string TexturePath;
	[Export] public Label TexturePathLabel;


	public override void _Ready()
	{
		base._Ready();
		DirAccess.MakeDirAbsolute( "user://designs" );
		DirAccess.MakeDirAbsolute( "user://designs/converted" );

		// var fileDialog = GetNode<FileDialog>( "FileDialog" );

		FileDialog.FileSelected += OnFileSelected;
	}

	private void OnFileSelected( string path )
	{

		Logger.Info( "DebugMenu", $"Importing floor decal from {path}" );

		var player = GetNode<Code.Player.PlayerController>( "/root/Main/Player" );

		var item = PersistentItem.Create( ResourceManager.Instance.LoadItemFromId<ItemData>( "floor_decal" ) ) as FloorDecal;
		if ( item == null ) throw new System.Exception( "Failed to create floor decal" );

		if ( path.GetExtension().ToLower() == "acnl" )
		{
			var bytes = Godot.FileAccess.GetFileAsBytes( path );
			var acnl = new ACNLFormat( bytes );

			var image = acnl.GetImage();
			image.SavePng( $"user://designs/converted/{Path.GetFileNameWithoutExtension( path )}.png" );

			TexturePath = $"user://designs/converted/{Path.GetFileNameWithoutExtension( path )}.png";
		}
		else
		{
			TexturePath = path;
		}

		if ( string.IsNullOrWhiteSpace( TexturePath ) )
		{
			// throw new System.Exception( "Failed to get texture path." );
			TexturePathLabel.Text = "Failed to get texture path.";
			return;
		}

		if ( !player.Equips.IsEquippedItemType<Paintbrush>( Components.Equips.EquipSlot.Tool ) )
		{
			// throw new System.Exception( "No paintbrush equipped." );
			TexturePathLabel.Text = "No paintbrush equipped.";
			return;
		}

		var paintbrush = player.Equips.GetEquippedItem<Paintbrush>( Components.Equips.EquipSlot.Tool );

		paintbrush.CurrentTexturePath = TexturePath;

		TexturePathLabel.Text = TexturePath;

		// var player = GetNode<Code.Player.PlayerController>( "/root/Main/Player" );
		//  player.Inventory.PickUpItem( item );

	}

	public void OpenFolder()
	{
		OS.ShellOpen( ProjectSettings.GlobalizePath( "user://designs" ) );
	}

	public void SelectPattern()
	{
		FileDialog.PopupCentered();
	}

	public void ClearPattern()
	{
		TexturePath = "";
		TexturePathLabel.Text = "";

		var player = GetNode<Code.Player.PlayerController>( "/root/Main/Player" );

		if ( !player.Equips.IsEquippedItemType<Paintbrush>( Components.Equips.EquipSlot.Tool ) )
		{
			// throw new System.Exception( "No paintbrush equipped." );
			TexturePathLabel.Text = "No paintbrush equipped.";
			return;
		}

		var paintbrush = player.Equips.GetEquippedItem<Paintbrush>( Components.Equips.EquipSlot.Tool );

		paintbrush.CurrentTexturePath = "";

		TexturePathLabel.Text = "No texture";

	}

	public void Open()
	{
		Show();
	}

	public void Close()
	{
		Hide();
	}


}
