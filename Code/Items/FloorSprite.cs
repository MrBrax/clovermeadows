using System;
using System.Text.RegularExpressions;

namespace vcrossing.Code.Items;

public partial class FloorSprite : WorldItem
{

	[Export] public Sprite3D Sprite { get; set; }

	public string TexturePath { get; set; }

	// private static Dictionary<string, StandardMaterial3D> _materials = new();

	private float _animTimer = 0;
	private float _animSpeed = 0.1f;
	private bool _isAnimated;

	private int _frames = 4;
	private int _height = 32;

	public void UpdateDecal()
	{

		if ( !string.IsNullOrWhiteSpace( TexturePath ) )
		{
			// decal.TextureAlbedo = Loader.LoadResource<Texture2D>( TexturePath );
			// decal.TextureAlbedo = GD.Load<CompressedTexture2D>( TexturePath );

			var image = new Godot.Image();

			if ( image.Load( TexturePath ) != Error.Ok )
			{
				Logger.LogError( "FloorSprite", $"Failed to load image from {TexturePath}" );
				return;
			}

			var texture = ImageTexture.CreateFromImage( image );

			_isAnimated = TexturePath.GetFile().StartsWith( "anim_" ) || Regex.IsMatch( TexturePath.GetFile(), @"^anim\d+_" );

			Logger.Info( "FloorSprite", $"TexturePath: {TexturePath}, isAnimated: {_isAnimated} ({TexturePath.GetFile()})" );

			if ( !_isAnimated )
			{

				Sprite.Texture = texture;

				Logger.Info( "FloorSprite", $"Updated decal texture to {TexturePath}" );

			}
			else
			{

				Sprite.Texture = texture;

				Sprite.RegionEnabled = true;

				var region = new Rect2( 0, 0, texture.GetHeight(), texture.GetHeight() );
				Sprite.RegionRect = region;

				_frames = texture.GetWidth() / texture.GetHeight();
				_height = texture.GetHeight();

				Logger.Info( "FloorSprite", $"Updated decal texture to {TexturePath}, frames: {_frames}, height: {_height}" );

			}

		}
		else
		{
			Logger.Warn( "FloorSprite", "TexturePath is null" );
		}
	}

	public override void _Process( double delta )
	{

		// TODO: use sprite animation instead

		if ( _isAnimated )
		{
			_animTimer += (float)delta;

			if ( _animTimer >= _animSpeed )
			{
				_animTimer = 0;

				var region = Sprite.RegionRect;
				region.Position += new Vector2( _height, 0 );

				if ( region.Position.X >= _height * _frames )
				{
					region.Position = new Vector2( 0, 0 );
				}

				Sprite.RegionRect = region;
			}
		}
	}

}
