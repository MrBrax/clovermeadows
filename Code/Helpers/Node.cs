
namespace vcrossing.Code.Helpers;

public static class NodeExtensions
{

	/// <summary>
	/// Retrieves a list of nodes of a specific type from a given Node3D and its children.
	/// </summary>
	/// <typeparam name="T">The type of nodes to retrieve.</typeparam>
	/// <param name="node">The Node3D to search for nodes.</param>
	/// <returns>A list of nodes of the specified type.</returns>
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

	public static void SetCollisionState( Node3D node, bool state )
	{
		foreach ( var collision in GetNodesOfType<CollisionShape3D>( node ) )
		{
			collision.Disabled = !state;
		}
	}

}
