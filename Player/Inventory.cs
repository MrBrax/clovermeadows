using Godot;
using vcrossing.Carriable;

namespace vcrossing.Player;

public partial class Inventory : Node3D
{

	public BaseCarriable CurrentCarriable;
	
	public override void _Ready()
	{
		// CurrentCarriable = GetNode<BaseCarriable>( "CurrentCarriable" );
	}
	
	public override void _Input( InputEvent @event )
	{
		if ( @event is InputEventAction eventAction )
		{
			if ( eventAction.Action == "Use" )
			{
				if ( CurrentCarriable != null )
				{
					CurrentCarriable.OnUse( GetNode<PlayerController>( "../Player" ) );
				}
			}
		}
	}

}
