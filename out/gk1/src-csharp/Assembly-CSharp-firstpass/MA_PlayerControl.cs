using DarkTonic.MasterAudio;
using UnityEngine;

public class MA_PlayerControl : MonoBehaviour
{
	public GameObject ProjectilePrefab;

	public bool canShoot = true;

	private const float MoveSpeed = 10f;

	private Transform _trans;

	private float _lastMoveAmt;

	private void Awake()
	{
		base.useGUILayout = false;
		_trans = base.transform;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.name.StartsWith("Enemy("))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
	}

	private void OnBecameInvisible()
	{
	}

	private void OnBecameVisible()
	{
	}

	private void Update()
	{
		float num = Input.GetAxis("Horizontal") * 10f * AudioUtil.FrameTime;
		if (num != 0f)
		{
			if (_lastMoveAmt == 0f)
			{
				MasterAudio.FireCustomEvent("PlayerMoved", _trans);
			}
		}
		else if (_lastMoveAmt != 0f)
		{
			MasterAudio.FireCustomEvent("PlayerStoppedMoving", _trans);
		}
		_lastMoveAmt = num;
		Vector3 position = _trans.position;
		position.x += num;
		_trans.position = position;
		if (canShoot && Input.GetMouseButtonDown(0))
		{
			Vector3 position2 = _trans.position;
			position2.y += 1f;
			Object.Instantiate(ProjectilePrefab, position2, ProjectilePrefab.transform.rotation);
		}
	}
}
