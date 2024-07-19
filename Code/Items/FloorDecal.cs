using System;

namespace vcrossing.Code.Items;

public partial class FloorDecal : WorldItem
{

	[Export] public Decal Decal { get; set; }
	[Export] public MeshInstance3D Mesh { get; set; }

	public string TexturePath { get; set; }

	private static Dictionary<string, StandardMaterial3D> _materials = new();

	public void UpdateDecal()
	{
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

			// Decal.TextureAlbedo = texture;

			/* var material = new StandardMaterial3D
			{
				AlbedoTexture = texture,
				TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
				Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor
			}; */

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
			Logger.Warn( "FloorDecal", "TexturePath is null" );
		}
	}
}
