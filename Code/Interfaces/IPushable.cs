namespace vcrossing.Code.Interfaces;

public interface IPushable
{

	public float PushForce { get; set; }
	public bool PushOnce { get; set; }

	public void OnPushed( Node3D pusher ) { }

}
