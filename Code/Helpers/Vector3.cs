
namespace vcrossing2.Code.Helpers;

public static class Vector3Extensions
{
	/// <summary>
	///  Returns a new Vector3 with the X component set to the given value.
	/// </summary>
	public static Vector3 WithX( this Vector3 vector, float x )
	{
		return new Vector3( x, vector.Y, vector.Z );
	}

	/// <summary>
	///  Returns a new Vector3 with the Y component set to the given value.
	/// </summary>
	public static Vector3 WithY( this Vector3 vector, float y )
	{
		return new Vector3( vector.X, y, vector.Z );
	}

	/// <summary>
	///  Returns a new Vector3 with the Z component set to the given value.
	/// </summary>
	public static Vector3 WithZ( this Vector3 vector, float z )
	{
		return new Vector3( vector.X, vector.Y, z );
	}
}
