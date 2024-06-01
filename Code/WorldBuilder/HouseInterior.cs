using System;
using Godot.Collections;
using vcrossing2.Code.Items;

namespace vcrossing2.Code.WorldBuilder;

public partial class HouseInterior : Node3D
{

	internal WorldManager WorldManager => GetNode<WorldManager>( "/root/Main/WorldContainer" );


	[Export( PropertyHint.ResourceType, "Room" )]
	public Array<Room> Rooms { get; set; }

	public void LoadRooms()
	{
		if ( Rooms == null )
		{
			Logger.Warn( "HouseInterior", "Rooms array is null." );
			return;
		}

		foreach ( var room in Rooms )
		{
			LoadRoom( room );
		}
	}

	public void SetupCollisions()
	{
		if ( Rooms == null )
		{
			Logger.Warn( "HouseInterior", "Rooms array is null." );
			return;
		}

		foreach ( var room in Rooms )
		{
			var floor = GetFloor( Rooms.IndexOf( room ) );
			var wall = GetWall( Rooms.IndexOf( room ) );

			if ( floor == null )
			{
				Logger.LogError( "HouseInterior", $"Floor mesh not found for room '{room}'." );
				return;
			}

			if ( wall == null )
			{
				Logger.LogError( "HouseInterior", $"Wall mesh not found for room '{room}'." );
				return;
			}

			var floorCollider = floor.GetNode<StaticBody3D>( "StaticBody3D" );
			var wallCollider = wall.GetNode<StaticBody3D>( "StaticBody3D" );

			if ( floorCollider.CollisionLayer != 1010u || wallCollider.CollisionLayer != 1010u )
			{
				Logger.Warn( "HouseInterior", $"Missing collision layer for room '{room}' ({floorCollider.CollisionLayer}, {wallCollider.CollisionLayer})." );
			}

			floorCollider.CollisionLayer = 1010u;
			wallCollider.CollisionLayer = 1010u;

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

		if ( wallpaperData == null )
		{
			Logger.Info( "Removing wallpaper." );
			wallMesh.MaterialOverride = null;
			WorldManager.ActiveWorld.SaveData.Wallpapers[index] = null;
			WorldManager.ActiveWorld.Save();
			return;
		}

		if ( WorldManager.ActiveWorld.SaveData == null ) throw new Exception( "World save data is null." );
		if ( WorldManager.ActiveWorld.SaveData.Wallpapers == null ) throw new Exception( "Wallpapers array is null." );

		var material = new StandardMaterial3D();
		material.AlbedoTexture = wallpaperData.Texture;
		wallMesh.MaterialOverride = material;

		WorldManager.ActiveWorld.SaveData.Wallpapers[index] = wallpaperData.ResourcePath;
		WorldManager.ActiveWorld.Save();

		Logger.Info( "Wallpaper set." );

	}

	private void LoadRoom( Room room )
	{

		if ( WorldManager.ActiveWorld.SaveData == null )
		{
			Logger.Warn( "HouseInterior", "World save data is null." );
			return;
		}

		if ( WorldManager.ActiveWorld.SaveData.Wallpapers == null )
		{
			Logger.Warn( "HouseInterior", "Wallpapers array is null." );
			return;
		}
		if ( WorldManager.ActiveWorld.SaveData.Wallpapers.Count <= 0 )
		{
			Logger.Info( "HouseInterior", "No wallpapers found." );
			return;
		}

		for ( var i = 0; i < WorldManager.ActiveWorld.SaveData.Wallpapers.Count; i++ )
		{
			if ( WorldManager.ActiveWorld.SaveData.Wallpapers[i] == null )
			{
				Logger.Info( "HouseInterior", $"Wallpaper data at index {i} is null." );
				continue;
			}

			var wallpaperData = Loader.LoadResource<WallpaperData>( WorldManager.ActiveWorld.SaveData.Wallpapers[i] );

			if ( wallpaperData == null )
			{
				Logger.Warn( "HouseInterior", $"Wallpaper data '{WorldManager.ActiveWorld.SaveData.Wallpapers[i]}' not found." );
				continue;
			}

			SetWallpaper( i, wallpaperData );
		}

	}
}
