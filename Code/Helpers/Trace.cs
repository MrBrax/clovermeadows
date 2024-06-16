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
		public CollisionObject3D Collider;
		public int Shape;
		public Rid Rid;
	}

	public class ShapeTraceResult
	{
		/// <summary>
		/// The collider ID.
		/// </summary>
		public int ColliderId;
		public object Collider;

		/// <summary>
		/// The shape index of the colliding shape.
		/// </summary>
		public int Shape;
		public Rid Rid;

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
			Collider = result["collider"].As<CollisionObject3D>(),
			Shape = result["shape"].AsInt32(),
			Rid = result["rid"].AsRid()
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
				Shape = r["shape"].AsInt32(),
				Rid = r["rid"].AsRid()
			};
		}

	}

}
