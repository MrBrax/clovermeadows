using System;
using System.Text.RegularExpressions;

namespace vcrossing.Code.Items;

public partial class FloorDecal : WorldItem
{

	// [Export] public Decal Decal { get; set; }
	[Export] public MeshInstance3D Mesh { get; set; }

	public string TexturePath { get; set; }

	private static Dictionary<string, StandardMaterial3D> _materials = new();

	private float _animTimer = 0;
	private float _animSpeed = 0.1f;
	private bool _isAnimated;
	private AtlasTexture _atlas;

	public void UpdateDecal()
	{
		_atlas?.Dispose();
		_atlas = null;

		if ( Mesh.Mesh == null ) throw new Exception( "Mesh is null" );

		if ( !string.IsNullOrWhiteSpace( TexturePath ) )
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

			_isAnimated = TexturePath.GetFile().StartsWith( "anim_" ) || Regex.IsMatch( TexturePath.GetFile(), @"^anim\d+_" );

			Logger.Info( "FloorDecal", $"TexturePath: {TexturePath}, isAnimated: {_isAnimated} ({TexturePath.GetFile()})" );

			if ( !_isAnimated )
			{

				StandardMaterial3D material;

				if ( _materials.ContainsKey( TexturePath ) )
				{
					Logger.Info( "FloorDecal", $"Reusing existing material for {TexturePath}: {_materials[TexturePath]}" );
					material = _materials[TexturePath];
				}
				else
				{
					material = new StandardMaterial3D
					{
						AlbedoTexture = texture,
						TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
						Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor,
					};

					Logger.Info( "FloorDecal", $"Created new material for {TexturePath}: {material}" );
					_materials.Add( TexturePath, material );
				}

				Mesh.Mesh = Mesh.Mesh.Duplicate() as QuadMesh; // Clone the mesh to avoid modifying the original material

				(Mesh.Mesh as QuadMesh).Material = material;

				Logger.Info( "FloorDecal", $"Updated decal texture to {TexturePath}" );

			}
			else
			{

				ShaderMaterial shader = new ShaderMaterial();

				shader.Shader = Loader.LoadResource<Shader>( "res://shaders/animated_floor_decal_new.tres" );

				shader.SetShaderParameter( "texture", texture );
				shader.SetShaderParameter( "frame_count", texture.GetWidth() / texture.GetHeight() );
				// shader.SetShaderParameter( "speed", 1f ); // TODO: implement speed control

				var regexSpeedMatch = Regex.Match( TexturePath.GetFile(), @"anim(\d+)_" );
				if ( regexSpeedMatch.Success )
				{
					_animSpeed = float.Parse( regexSpeedMatch.Groups[1].Value ) / 1000f;
				}

				Mesh.Mesh = Mesh.Mesh.Duplicate() as QuadMesh; // Clone the mesh to avoid modifying the original material

				(Mesh.Mesh as QuadMesh).Material = shader;

			}

		}
		else
		{
			Logger.Warn( "FloorDecal", "TexturePath is null" );
		}
	}

	/* public override void _Process( double delta )
	{

		if ( _isAnimated )
		{
			_animTimer += (float)delta;

			/* if ( _animTimer >= _animSpeed )
			{
				_animTimer = 0;

				var region = _atlas.Region;
				region.Position += new Vector2( 32, 0 );

				if ( region.Position.X >= 32 * 4 )
				{
					region.Position = new Vector2( 0, 0 );
				}

				_atlas.Region = region;

				GD.Print( $"_animTimer: {_animTimer}, _animSpeed: {_animSpeed}, region: {region}" );
			} */

}
