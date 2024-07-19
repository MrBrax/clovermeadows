using System;

namespace vcrossing.Code.Items;

public partial class FloorDecal : WorldItem
{

	[Export] public Decal Decal { get; set; }

	public string TexturePath { get; set; }

	public void UpdateDecal()
	{
		if ( Decal != null && !string.IsNullOrWhiteSpace( TexturePath ) )
		{
			// decal.TextureAlbedo = Loader.LoadResource<Texture2D>( TexturePath );
			// decal.TextureAlbedo = GD.Load<CompressedTexture2D>( TexturePath );

			var image = new Godot.Image();

			if ( image.Load( TexturePath ) != Error.Ok )
			{
				Logger.LogError( "FloorDecal", $"Failed to load image from {TexturePath}" );
				return;
			}

			var texture = ImageTexture.CreateFromImage( image );

			Decal.TextureAlbedo = texture;

			Logger.Info( "FloorDecal", $"Updated decal texture to {TexturePath}" );
		}
		else
		{
			Logger.Warn( "FloorDecal", "Decal or TexturePath is null" );
		}
	}
}
