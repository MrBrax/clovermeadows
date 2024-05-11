using Godot;

namespace vcrossing2.Code.Vehicles;

public partial class BaseVehicle : Node3D
{
	
	[Export] public float Speed { get; set; } = 5;
	public override void _Process( double delta )
	{
		base._Process( delta );
		
		// Move the vehicle forward
		var forward = GlobalTransform.Basis.Z.Normalized();
		GlobalPosition += forward * -Speed * (float)delta;
		
		if ( GlobalPosition.X > 128 )
		{
			GlobalPosition = new Vector3( -10f, GlobalPosition.Y, GlobalPosition.Z );
		} else if ( GlobalPosition.X < -10 )
		{
			GlobalPosition = new Vector3( 128, GlobalPosition.Y, GlobalPosition.Z );
		}
	}
}
