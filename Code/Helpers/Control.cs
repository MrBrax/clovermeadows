
namespace vcrossing.Code.Helpers;

public static class ControlExtensions
{

	public static List<T> GetControlsOfType<T>( this Control node ) where T : Control
	{
		var nodes = new List<T>();

		foreach ( var child in node.GetChildren() )
		{

			if ( child is not Control control ) continue;

			if ( child is T t )
			{
				nodes.Add( t );
			}

			if ( child.GetChildCount() > 0 )
			{
				nodes.AddRange( GetControlsOfType<T>( control ) );
			}

		}

		return nodes;
	}

	public static void QueueFreeAllChildren( this Control node )
	{
		foreach ( Node child in node.GetChildren() )
		{
			child.QueueFree();
		}
	}

}
