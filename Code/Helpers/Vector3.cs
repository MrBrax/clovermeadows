
namespace vcrossing.Code.Helpers;

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

	/// <summary>
	///  Clamp the vector to a given value, returning a new vector.
	/// </summary>
	public static Vector3 Clamp( this Vector3 vector, float min, float max )
	{
		return new Vector3( Mathf.Clamp( vector.X, min, max ), Mathf.Clamp( vector.Y, min, max ), Mathf.Clamp( vector.Z, min, max ) );
	}

	public static Vector3 LerpDegreeAngles( this Vector3 from, Vector3 to, float t )
	{
		return new Vector3(
			Mathf.RadToDeg( Mathf.LerpAngle( Mathf.DegToRad( from.X ), Mathf.DegToRad( to.X ), t ) ),
			Mathf.RadToDeg( Mathf.LerpAngle( Mathf.DegToRad( from.Y ), Mathf.DegToRad( to.Y ), t ) ),
			Mathf.RadToDeg( Mathf.LerpAngle( Mathf.DegToRad( from.Z ), Mathf.DegToRad( to.Z ), t ) )
		);
	}

	public static Vector3 LerpRadAngles( this Vector3 from, Vector3 to, float t )
	{
		return new Vector3(
			Mathf.LerpAngle( from.X, Mathf.DegToRad( to.X ), t ),
			Mathf.LerpAngle( from.Y, Mathf.DegToRad( to.Y ), t ),
			Mathf.LerpAngle( from.Z, Mathf.DegToRad( to.Z ), t )
		);
	}
}
