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
	private int _width = 32;

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

			_height = texture.GetHeight();
			_width = texture.GetWidth();
			Sprite.PixelSize = 1f / _width;

			if ( !_isAnimated )
			{

				Sprite.Texture = texture;

				// var y = (1f / _width) / 32f;

				// Sprite.Position = new Vector3( 0, y, 0 );

				Logger.Info( "FloorSprite", $"Updated decal texture to {TexturePath}" );

			}
			else
			{

				Sprite.Texture = texture;

				/* Sprite.RegionEnabled = true;

				var region = new Rect2( 0, 0, texture.GetHeight(), texture.GetHeight() );
				Sprite.RegionRect = region; */

				Sprite.RegionEnabled = false;

				_frames = texture.GetHeight() / texture.GetWidth();

				Sprite.Vframes = _frames;

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
		if ( _isAnimated )
		{
			_animTimer += (float)delta;
			if ( _animTimer >= _animSpeed )
			{
				_animTimer = 0;
				Sprite.Frame = (Sprite.Frame + 1) % _frames;
			}
		}
	}

}
