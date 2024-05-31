using System;
using Godot.Collections;

namespace vcrossing2.Code.WorldBuilder;

public partial class HouseInterior : Node3D
{

	[Export(PropertyHint.ResourceType, "Room")]
	public Array<Room> Rooms { get; set; }

	override public void _Ready()
	{
		foreach ( var room in Rooms )
		{
			LoadRoom( room );
		}
	}

	private void LoadRoom( Room room )
	{

		if ( GetNode(room.Wall) is not MeshInstance3D wallMesh )
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
		floorMesh.MaterialOverride = testMaterial;
		
		
	}
}
