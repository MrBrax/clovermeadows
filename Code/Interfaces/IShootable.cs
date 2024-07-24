namespace vcrossing.Code.Interfaces;

/// <summary>
///  For objects that can get shot by pellets. Used by the pellet gun.
///  Collision layer HAS to be set to 15 (PelletScan) for this to work.
/// </summary>
/// <remarks>
/// This interface is used by <see cref="Node3D"/> objects that can be shot at. 
/// </remarks>
public interface IShootable
{
	// float LookAtWhenShotTimeout { get; }
	// Node3D LookAtWhenShotTarget { get; set; }
	// bool LookAtWhenShot { get; }


	public void OnShot( Node3D pellet );

}
