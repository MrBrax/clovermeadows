
namespace vcrossing2.Code.Helpers;

public static class NodeExtensions
{

	public static List<T> GetNodesOfType<T>( Node3D node ) where T : Node3D
	{
		var nodes = new List<T>();

		foreach ( var child in node.GetChildren() )
		{

			if ( child is not Node3D node3d ) continue;

			if ( child is T t )
			{
				nodes.Add( t );


			}

			if ( child.GetChildCount() > 0 )
			{
				nodes.AddRange( GetNodesOfType<T>( node3d ) );
			}
			/* if ( child is T t )
			{
				nodes.Add( t );

				if ( child.GetChildCount() > 0 )
				{
					nodes.AddRange( GetNodesOfType<T>( t ) );
				}
			} */

		}

		return nodes;
	}

}
