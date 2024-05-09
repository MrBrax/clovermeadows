using Godot;

namespace vcrossing.Helpers;

public class BBox
{
	public Vector3 Origin = Vector3.Zero;
	public Vector3 Center => (Min + Max) * 0.5f;
	public Vector3 Size => Max - Min;
	public Vector3 Extents => Size * 0.5f;

	private Vector3 _min;
	private Vector3 _max;

	public Vector3 Min;

	public Vector3 Max;

	public BBox( Vector3 min, Vector3 max )
	{
		Min = new Vector3( Mathf.Min( min.X, max.X ), Mathf.Min( min.Y, max.Y ), Mathf.Min( min.Z, max.Z ) );
		Max = new Vector3( Mathf.Max( min.X, max.X ), Mathf.Max( min.Y, max.Y ), Mathf.Max( min.Z, max.Z ) );
	}
	
	/*public BBox( Vector3 center, Vector3 size )
	{
		Min = center - size / 2;
		Max = center + size / 2;
	}*/

	public BBox( BoxShape3D shape )
	{
		var boxSize = shape.Size;
		Min = -boxSize / 2;
		Max = boxSize / 2;
	}

	public void Translate( Vector3 translation )
	{
		Min += translation;
		Max += translation;
	}

	/// <summary>
	///  Checks if this bounding box intersects with another bounding box.
	///  Includes the origin.
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public bool Intersects( BBox other )
	{
		return Min.X <= other.Max.X && Max.X >= other.Min.X &&
		       Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
		       Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
	}

	public bool Contains( Vector3 point )
	{
		return point.X >= Min.X && point.X <= Max.X &&
		       point.Y >= Min.Y && point.Y <= Max.Y &&
		       point.Z >= Min.Z && point.Z <= Max.Z;
	}

	public void Rotate( Quaternion rotation )
	{
		var center = Center;
		var extents = Extents;

		var r = rotation;
		var absR = new Quaternion( Mathf.Abs( r.X ), Mathf.Abs( r.Y ), Mathf.Abs( r.Z ), Mathf.Abs( r.W ) );

		var x = r.X * 2;
		var y = r.Y * 2;
		var z = r.Z * 2;
		var xx = r.X * x;
		var yy = r.Y * y;
		var zz = r.Z * z;
		var xy = r.X * y;
		var xz = r.X * z;
		var yz = r.Y * z;
		var wx = r.W * x;
		var wy = r.W * y;
		var wz = r.W * z;

		var m00 = 1 - (yy + zz);
		var m01 = xy - wz;
		var m02 = xz + wy;
		var m10 = xy + wz;
		var m11 = 1 - (xx + zz);
		var m12 = yz - wx;
		var m20 = xz - wy;
		var m21 = yz + wx;
		var m22 = 1 - (xx + yy);

		var newMin = new Vector3(
			m00 * Min.X + m01 * Min.Y + m02 * Min.Z,
			m10 * Min.X + m11 * Min.Y + m12 * Min.Z,
			m20 * Min.X + m21 * Min.Y + m22 * Min.Z
		);

		var newMax = new Vector3(
			m00 * Max.X + m01 * Max.Y + m02 * Max.Z,
			m10 * Max.X + m11 * Max.Y + m12 * Max.Z,
			m20 * Max.X + m21 * Max.Y + m22 * Max.Z
		);

		Min = newMin + center;
		Max = newMax + center;
	}

	public void Grow( float amount )
	{
		Min -= new Vector3( amount, amount, amount );
		Max += new Vector3( amount, amount, amount );
	}

	public BBox Clone()
	{
		return new BBox( Min, Max );
	}

	public override string ToString()
	{
		return $"BBox({Min}, {Max})";
	}
}
