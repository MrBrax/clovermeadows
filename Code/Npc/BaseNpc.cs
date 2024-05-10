using Godot;
using vcrossing2.Code.Player;

namespace vcrossing2.Code.Npc;

public partial class BaseNpc : CharacterBody3D
{
	[Export] public virtual string Name { get; set; }
	[Export] public Node3D Model { get; set; }
	[Export] public NavigationAgent3D NavigationAgent { get; set; }
	// public virtual string Description { get; set; }
	
	[Export] public float WalkSpeed { get; set; } = 2.0f;
	private Vector3 TargetPosition { get; set; }

	public Vector3 MovementTarget
	{
		get => NavigationAgent.TargetPosition;
		set => NavigationAgent.TargetPosition = value;
	}

	public enum CurrentState { Idle, Walking, Talking, Interacting, Pose }

	public CurrentState State { get; set; }

	public override void _Ready()
	{
		base._Ready();
		SetState( CurrentState.Idle );
		
		// NavigationAgent = GetNode<NavigationAgent3D>( "NavigationAgent" );
		NavigationAgent.PathDesiredDistance = 0.5f;
		NavigationAgent.TargetDesiredDistance = 0.5f;
		
		// Callable.From( ActorSetup ).CallDeferred();
	}
	
	public void SetTargetPosition( Vector3 position )
	{
		MovementTarget = position;
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

		SetTargetPosition( new Vector3( 4, 0, 4 ) );
	}

	public override void _PhysicsProcess( double delta )
	{
		base._PhysicsProcess( delta );

		if ( NavigationAgent.IsNavigationFinished() )
		{
			return;
		}

		var currentAgentPosition = GlobalTransform.Origin;
		var nextPathPosition = NavigationAgent.GetNextPathPosition();
		
		// Velocity = ( nextPathPosition - currentAgentPosition ).Normalized() * WalkSpeed;
		Velocity = currentAgentPosition.DirectionTo( nextPathPosition ) * WalkSpeed;
		MoveAndSlide();
	}
	
	/*private async void ActorSetup()
	{
		await ToSignal( GetTree(), SceneTree.SignalName.PhysicsFrame );
		
		
	}*/
}
