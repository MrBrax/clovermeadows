using System;
using System.Text.Json.Serialization;
using vcrossing.Code.Persistence;

namespace vcrossing.Code.Persistence;

public class FloorDecal : PersistentItem
{

	[JsonInclude] public string TexturePath { get; set; }

	public override void GetNodeData( Node3D node )
	{
		base.GetNodeData( node );
		if ( node is Code.Items.FloorDecal decal )
		{
			TexturePath = decal.TexturePath;
		}
	}

	public override void SetNodeData( Node3D node )
	{
		base.SetNodeData( node );
		if ( node is Code.Items.FloorDecal decal )
		{
			decal.TexturePath = TexturePath;
			decal.UpdateDecal();
		}
	}

	private Texture2D _iconTexture;
	public override Texture2D GetIconTexture()
	{
		if ( _iconTexture != null )
		{
			return _iconTexture;
		}

		var image = new Godot.Image();

		if ( image.Load( TexturePath ) != Error.Ok )
		{
			Logger.LogError( "FloorDecal", $"Failed to load image from {TexturePath}" );
			return null;
		}

		_iconTexture = ImageTexture.CreateFromImage( image );

		return _iconTexture;
	}

}
