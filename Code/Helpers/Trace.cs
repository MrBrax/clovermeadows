using Godot;

namespace vcrossing2.Code.Helpers;

public class Trace
{

	private PhysicsDirectSpaceState3D _space;
	public Trace( PhysicsDirectSpaceState3D space )
	{
		_space = space;
	}
	
	public class TraceResult
	{
		public Vector3 Position;
		public Vector3 Normal;
		public int FaceIndex;
		public int ColliderId;
		public object Collider;
		public object Shape;
		public object Rid;
	}
	
	public TraceResult CastRay( PhysicsRayQueryParameters3D parameters )
	{
		var result = _space.IntersectRay( parameters );
		if ( result.Count == 0 )
		{
			return null;
		}
		
		return new TraceResult
		{
			Position = (Vector3)result["position"],
			Normal = (Vector3)result["normal"],
			FaceIndex = (int)result["face_index"],
			ColliderId = (int)result["collider_id"],
			Collider = result["collider"],
			Shape = result["shape"],
			Rid = result["rid"]
		};
	}
		
	
}
