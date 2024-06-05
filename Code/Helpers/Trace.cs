using Godot;

namespace vcrossing.Code.Helpers;

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

	public class ShapeTraceResult
	{
		public int ColliderId;
		public object Collider;
		public object Shape;
		public object Rid;

		public void DebugPrint()
		{
			GD.Print( $"ColliderId: {ColliderId}, Collider: {Collider}, Shape: {Shape}, Rid: {Rid}" );
		}
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

	public IEnumerable<ShapeTraceResult> CastShape( PhysicsShapeQueryParameters3D parameters )
	{
		var result = _space.IntersectShape( parameters );

		foreach ( var r in result )
		{

			foreach ( var key in r.Keys )
			{
				GD.Print( $"{key}: {r[key]} ({r[key].GetType()})" );
			}

			yield return new ShapeTraceResult
			{
				ColliderId = (int)r["collider_id"],
				Collider = r["collider"],
				Shape = r["shape"],
				Rid = r["rid"]
			};
		}

	}

}
