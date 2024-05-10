using Godot;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Npc;

public partial class BaseNpc : Node3D
{
	[Export] public virtual string Name { get; set; }
	[Export] public Node3D Model { get; set; }
	// public virtual string Description { get; set; }

	public enum CurrentState { Idle, Walking, Talking, Interacting, Pose }

	public CurrentState State { get; set; }

	public override void _Ready()
	{
		base._Ready();
		SetState( CurrentState.Idle );
	}

	public void SetState( CurrentState state )
	{
		State = state;
	}
	
	public void LookAtNode( Node3D node )
	{
		var target = node.GlobalTransform.Origin;
		var position = GlobalTransform.Origin;
		var direction = target - position;
		Model.GlobalTransform = Model.GlobalTransform.LookingAt( position - direction, Vector3.Up );
		
	}

	public virtual void OnInteract( PlayerInteract player )
	{
		GD.Print( $"Interacting with {Name}" );
		LookAtNode( player );
	}
}
