using System;
using Godot;

namespace vcrossing.Code.Data;

[GlobalClass]
public sealed partial class WallpaperData : ItemData
{

	// [Export] public string Name;
	// [Export] public string Description;
	[Export] public CompressedTexture2D Texture;

}
