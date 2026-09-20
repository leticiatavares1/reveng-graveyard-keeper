using DarkTonic.MasterAudio;
using UnityEngine;

public class MA_EnemyOne : MonoBehaviour
{
	public GameObject ExplosionParticlePrefab;

	private Transform _trans;

	private float _speed;

	private float _horizSpeed;

	private void Awake()
	{
		base.useGUILayout = false;
		_trans = base.transform;
		_speed = (float)Random.Range(-3, -8) * AudioUtil.FrameTime;
		_horizSpeed = (float)Random.Range(-3, 3) * AudioUtil.FrameTime;
	}

	private void OnCollisionEnter(Collision collision)
	{
		Object.Instantiate(ExplosionParticlePrefab, _trans.position, Quaternion.identity);
	}

	private void Update()
	{
		Vector3 position = _trans.position;
		position.x += _horizSpeed;
		position.y += _speed;
		_trans.position = position;
		_trans.Rotate(Vector3.down * 300f * AudioUtil.FrameTime);
		if (_trans.position.y < -5f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
