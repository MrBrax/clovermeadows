using System;
using Godot.Collections;
using vcrossing.Code.Data;
using vcrossing.Code.Items;

namespace vcrossing.Code.WorldBuilder;

public partial class InteriorManager : Node3D
{

	internal WorldManager WorldManager => GetNode<WorldManager>( "/root/Main/WorldManager" );

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
		if ( Rooms == null || Rooms.Count == 0 )
		{

			Logger.Warn( "HouseInterior", "No rooms found, setting all to collision layer." );

			foreach ( var node in GetChildren() )
			{
				if ( node is not MeshInstance3D mesh ) continue;

				var collider = mesh.GetNodeOrNull<StaticBody3D>( "StaticBody3D" );

				if ( collider == null )
				{
					Logger.Warn( "HouseInterior", $"No collider found for mesh '{mesh.Name}'." );
					continue;
				}

				if ( collider.CollisionLayer != World.TerrainLayer )
				{
					Logger.Warn( "HouseInterior", $"Missing/wrong collision layer for mesh '{mesh.Name}' ({collider.CollisionLayer})." );
				}

				collider.CollisionLayer = World.TerrainLayer;
				Logger.Info( "HouseInterior", $"Collision layer set to {World.TerrainLayer} for mesh '{mesh.Name}'." );
			}

			return;
		}

		foreach ( var room in Rooms )
		{
			var floor = GetFloor( room.Id );
			var wall = GetWall( room.Id );

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

			if ( floorCollider.CollisionLayer != World.TerrainLayer || wallCollider.CollisionLayer != World.TerrainLayer )
			{
				Logger.Warn( "HouseInterior", $"Missing/wrong collision layer for room '{room}' ({floorCollider.CollisionLayer}, {wallCollider.CollisionLayer})." );
			}

			floorCollider.CollisionLayer = World.TerrainLayer;
			wallCollider.CollisionLayer = World.TerrainLayer;

		}
	}

	public Room GetRoom( string roomId )
	{
		return Rooms.FirstOrDefault( room => room.Id == roomId );
	}

	public MeshInstance3D GetWall( string roomId )
	{
		return GetNode<MeshInstance3D>( GetRoom( roomId ).Wall );
	}

	public MeshInstance3D GetFloor( string roomId )
	{
		return GetNode<MeshInstance3D>( GetRoom( roomId ).Floor );
	}

	public void SetWallpaper( string roomId, WallpaperData wallpaperData )
	{

		if ( GetWall( roomId ) is not MeshInstance3D wallMesh )
		{
			throw new Exception( "Wall mesh not found." );
		}

		if ( wallpaperData == null )
		{
			Logger.Info( "Removing wallpaper." );
			wallMesh.MaterialOverride = null;
			WorldManager.ActiveWorld.SaveData.Wallpapers[roomId] = null;
			WorldManager.ActiveWorld.Save();
			return;
		}

		if ( WorldManager.ActiveWorld.SaveData == null ) throw new Exception( "World save data is null." );
		if ( WorldManager.ActiveWorld.SaveData.Wallpapers == null ) throw new Exception( "Wallpapers array is null." );

		var material = new StandardMaterial3D();
		material.AlbedoTexture = wallpaperData.Texture;
		wallMesh.MaterialOverride = material;

		WorldManager.ActiveWorld.SaveData.Wallpapers[roomId] = wallpaperData.ResourcePath;
		WorldManager.ActiveWorld.Save();

		Logger.Info( "Wallpaper set." );

	}

	public void SetFloor( string roomId, FlooringData floorData )
	{

		if ( GetFloor( roomId ) is not MeshInstance3D floorMesh )
		{
			throw new Exception( "Floor mesh not found." );
		}

		if ( floorData == null )
		{
			Logger.Info( "Removing floor." );
			floorMesh.MaterialOverride = null;
			WorldManager.ActiveWorld.SaveData.Floors[roomId] = null;
			WorldManager.ActiveWorld.Save();
			return;
		}

		if ( WorldManager.ActiveWorld.SaveData == null ) throw new Exception( "World save data is null." );
		if ( WorldManager.ActiveWorld.SaveData.Floors == null ) throw new Exception( "Floors array is null." );

		var material = new StandardMaterial3D();
		material.AlbedoTexture = floorData.Texture;
		floorMesh.MaterialOverride = material;

		WorldManager.ActiveWorld.SaveData.Floors[roomId] = floorData.ResourcePath;
		WorldManager.ActiveWorld.Save();

		Logger.Info( "Floor set." );

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

		foreach ( var roomWallpaperData in WorldManager.ActiveWorld.SaveData.Wallpapers )
		{

			var wallpaperData = Loader.LoadResource<WallpaperData>( roomWallpaperData.Value );

			if ( wallpaperData == null )
			{
				Logger.Warn( "HouseInterior", $"Wallpaper data '{roomWallpaperData.Value}' not found." );
				continue;
			}

			SetWallpaper( roomWallpaperData.Key, wallpaperData );
		}

		foreach ( var roomFloorData in WorldManager.ActiveWorld.SaveData.Floors )
		{

			var floorData = Loader.LoadResource<FlooringData>( roomFloorData.Value );

			if ( floorData == null )
			{
				Logger.Warn( "HouseInterior", $"Floor data '{roomFloorData.Value}' not found." );
				continue;
			}

			SetFloor( roomFloorData.Key, floorData );
		}

	}


}
