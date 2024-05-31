using System;
using Godot.Collections;
using vcrossing2.Code.Items;

namespace vcrossing2.Code.WorldBuilder;

public partial class HouseInterior : Node3D
{

	internal WorldManager WorldManager => GetNode<WorldManager>( "/root/Main/WorldContainer" );


	[Export(PropertyHint.ResourceType, "Room")]
	public Array<Room> Rooms { get; set; }

	override public void _Ready()
	{
		foreach ( var room in Rooms )
		{
			LoadRoom( room );
		}
	}

	public MeshInstance3D GetWall( int index )
	{
		return GetNode<MeshInstance3D>( Rooms[index].Wall );
	}

	public MeshInstance3D GetFloor( int index )
	{
		return GetNode<MeshInstance3D>( Rooms[index].Floor );
	}

	public void SetWallpaper( int index, WallpaperData wallpaperData )
	{

		if ( GetWall( index ) is not MeshInstance3D wallMesh )
		{
			throw new Exception( "Wall mesh not found." );
		}

		var material = new StandardMaterial3D();
		material.AlbedoTexture = wallpaperData.Texture;
		wallMesh.MaterialOverride = material;

		WorldManager.ActiveWorld.SaveData.Instances.FirstOrDefault().Value.Wallpapers[index] = wallpaperData.ResourcePath;
		WorldManager.ActiveWorld.Save();

	}

	private void LoadRoom( Room room )
	{

		/* if ( GetNode(room.Wall) is not MeshInstance3D wallMesh )
		{
			throw new Exception( "Wall mesh not found." );
		}

		if ( GetNode(room.Floor) is not MeshInstance3D floorMesh )
		{
			throw new Exception( "Floor mesh not found." );
		}

		var testMaterial = new StandardMaterial3D();
		testMaterial.AlbedoColor = new Color( 0.5f, 0.5f, 0.5f );

		wallMesh.MaterialOverride = testMaterial;
		floorMesh.MaterialOverride = testMaterial; */
		
		
	}
}
