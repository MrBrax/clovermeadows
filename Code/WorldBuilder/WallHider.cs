using Godot;
using Godot.Collections;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.WorldBuilder;

public partial class WallHider : Area3D
{
	
	[Export] public Node3D Mesh { get; set; }
	[Export] public string WallName { get; set; }
	
	// private float _wishedOpacity = 1;
	
	public override void _Ready()
	{
		base._Ready();
		
		if ( Mesh == null )
		{
			throw new System.Exception( "Mesh not set for wall hider." );
		}
		if ( WallName == null )
		{
			throw new System.Exception( "Wall name not set for wall hider." );
		}
		
		// Connect( "body_entered", this, nameof( OnAreaEntered ) );
		BodyEntered += OnAreaEntered;
		BodyExited += OnAreaExited;
		
	}
	
	public void OnAreaEntered( Node3D node )
	{
		if ( node is not PlayerController player )
		{
			// throw new System.Exception( "Area trigger entered by non-player." );
			GD.Print( "Area trigger entered by non-player." );
			return;
		}
		
		HideWall();
	}
	
	public void OnAreaExited( Node3D node )
	{
		if ( node is not PlayerController player )
		{
			// throw new System.Exception( "Area trigger entered by non-player." );
			GD.Print( "Area trigger entered by non-player." );
			return;
		}
		
		ShowWall();
	}
	
	public void HideWall()
	{
		if ( Mesh == null )
		{
			throw new System.Exception( "Mesh not set for wall hider." );
		}
		
		var wall = Mesh.GetNode<MeshInstance3D>( WallName );
		if ( wall == null )
		{
			throw new System.Exception( $"Wall not found: {WallName}" );
		}
		
		wall.Hide();
		// _wishedOpacity = 0f;
	}
	
	public void ShowWall()
	{
		if ( Mesh == null )
		{
			throw new System.Exception( "Mesh not set for wall hider." );
		}
		
		var wall = Mesh.GetNode<MeshInstance3D>( WallName );
		if ( wall == null )
		{
			throw new System.Exception( $"Wall not found: {WallName}" );
		}
		
		wall.Show();
		// _wishedOpacity = 1f;
	}
	
	/*public override void _Process( float delta )
	{
		if ( Mesh == null )
		{
			throw new System.Exception( "Mesh not set for wall hider." );
		}
		
		var wall = Mesh.GetNode<MeshInstance3D>( WallName );
		if ( wall == null )
		{
			throw new System.Exception( $"Wall not found: {WallName}" );
		}
		
		var material = wall.GetSurfaceMaterial( 0 ) as SpatialMaterial;
		if ( material == null )
		{
			throw new System.Exception( $"Material not found for wall {WallName}" );
		}
		
		var opacity = material.AlbedoColor.a;
		if ( opacity != _wishedOpacity )
		{
			opacity = Mathf.Lerp( opacity, _wishedOpacity, 0.1f );
			material.AlbedoColor = new Color( 1, 1, 1, opacity );
		}
	}*/
	
	
}
