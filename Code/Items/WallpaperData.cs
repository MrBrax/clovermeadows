using System;
using Godot;

namespace vcrossing2.Code.Items;

[GlobalClass]
public partial class WallpaperData : ItemData
{

	// [Export] public string Name;
	// [Export] public string Description;
	[Export] public CompressedTexture2D Texture;

}
