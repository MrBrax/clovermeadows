using System;

namespace vcrossing.Code.Items;

public partial class FloorDecal : WorldItem
{
    public string TexturePath { get; set; }

    public void UpdateDecal()
    {
        var decal = GetNode<Decal>( "Decal" );
        if ( decal != null && !string.IsNullOrWhiteSpace( TexturePath ) )
        {
            decal.TextureAlbedo = Loader.LoadResource<Texture2D>( TexturePath );
        }
    }
}
