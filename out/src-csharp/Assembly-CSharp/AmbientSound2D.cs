using DarkTonic.MasterAudio;
using UnityEngine;

public class AmbientSound2D : AmbientSound
{
	public new void OnEnable()
	{
		Vector3 position = base.transform.position;
		position.z = MainGame.camera_z;
		base.transform.position = position;
		base.OnEnable();
	}
}
