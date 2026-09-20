using DarkTonic.MasterAudio;
using UnityEngine;

public class MA_Laser : MonoBehaviour
{
	private Transform _trans;

	private void Awake()
	{
		base.useGUILayout = false;
		_trans = base.transform;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.name.StartsWith("Enemy("))
		{
			Object.Destroy(collision.gameObject);
			Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
		float num = 10f * AudioUtil.FrameTime;
		Vector3 position = _trans.position;
		position.y += num;
		_trans.position = position;
		if (_trans.position.y > 7f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
