using Godot;
using vcrossing2.Code.Objects;

namespace vcrossing2.Code.WorldBuilder;

public partial class Fish : Node3D
{

	private float _speed = 1.0f;
	private float _bobberMaxDistance = 2f;
	private float _bobberDiscoverAngle = 45f;

	public override void _Ready()
	{
		AddToGroup( "fish" );
	}

	public override void _Process( double delta )
	{
		// Rotate the fish
		// RotateY( (float)delta );

		var bobber = GetTree().GetNodesInGroup( "fishing_bobber" ).Cast<FishingBobber>().FirstOrDefault();

		if ( !IsInstanceValid( bobber ) )
		{
			return;
		}

		var bobberPosition = bobber.GlobalTransform.Origin.WithY( 0 );
		var fishPosition = GlobalTransform.Origin.WithY( 0 );

		// check if the bobber is near the fish
		var distance = fishPosition.DistanceTo( bobberPosition );
		if ( distance > _bobberMaxDistance )
		{
			// Logger.Info( "Fish", $"Bobber is too far away ({distance})." );
			return;
		}

		// check if the bobber is within the fish's view
		var direction = (bobberPosition - GlobalTransform.Origin).Normalized();
		var angle = Mathf.RadToDeg( Mathf.Acos( direction.Dot( GlobalTransform.Basis.Z ) ) );

		/* if ( angle > _bobberDiscoverAngle )
		{
			Logger.Info( "Fish", $"Bobber is not in view ({angle})." );
			return;
		} */

		// move towards the bobber
		var target = bobber.GlobalTransform.Origin;
		var moveDirection = (target - GlobalTransform.Origin).Normalized();
		GlobalPosition += moveDirection * _speed * (float)delta;

		// rotate towards the bobber
		var targetRotation = Mathf.Atan2( moveDirection.X, moveDirection.Z );
		var currentRotation = Rotation.Y;
		var newRotation = Mathf.LerpAngle( currentRotation, targetRotation, (float)delta * 2 );

		Rotation = new Vector3( 0, newRotation, 0 );

		if ( distance < 0.1f )
		{
			Logger.Info( "Fish", "Reached bobber." );
			bobber.Rod.CatchFish( this );
		}
	}

}
