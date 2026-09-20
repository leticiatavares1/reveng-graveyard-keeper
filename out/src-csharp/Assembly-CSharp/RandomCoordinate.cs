using UnityEngine;

public class RandomCoordinate : MonoBehaviour
{
	public Vector3 random = Vector3.zero;

	public float speed = 1f;

	public float roll_period_sec = 0.05f;

	public bool animate_z;

	private Vector3 _cur_target = Vector3.zero;

	private float _time;

	private Transform _tr;

	private void Update()
	{
		float num = Mathf.Min(Time.deltaTime, 0.05f);
		_time += num;
		if (_time > roll_period_sec)
		{
			while (_time > roll_period_sec)
			{
				_time -= roll_period_sec;
			}
			_cur_target = new Vector3(Random.Range(0f - random.x, random.x), Random.Range(0f - random.y, random.y), Random.Range(0f - random.z, random.z));
		}
		if (_tr == null)
		{
			_tr = base.transform;
		}
		float num2 = Mathf.Min(1f, speed * num);
		if (animate_z)
		{
			Vector3 vector = _cur_target - _tr.localPosition;
			_tr.localPosition += vector * num2;
			return;
		}
		Vector2 vector2 = _cur_target - _tr.localPosition;
		if (vector2.x > 10000f)
		{
			vector2.x = 0f;
		}
		if (vector2.y > 10000f)
		{
			vector2.y = 0f;
		}
		_tr.localPosition += (Vector3)(vector2 * num2);
	}
}
