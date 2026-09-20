using UnityEngine;

namespace LazyBearTechnology;

public class RandomCoordinate : MonoBehaviour
{
	public Vector3 random = Vector3.zero;

	public float speed = 1f;

	public float rollPeriodSec = 0.05f;

	public bool animateZ;

	private Vector3 curTarget = Vector3.zero;

	private float time;

	private Transform tr;

	private void Update()
	{
		float num = Mathf.Min(Time.deltaTime, 0.05f);
		time += num;
		if (time > rollPeriodSec)
		{
			while (time > rollPeriodSec)
			{
				time -= rollPeriodSec;
			}
			curTarget = new Vector3(Random.Range(0f - random.x, random.x), Random.Range(0f - random.y, random.y), Random.Range(0f - random.z, random.z));
		}
		if (tr == null)
		{
			tr = base.transform;
		}
		float num2 = Mathf.Min(1f, speed * num);
		if (animateZ)
		{
			Vector3 vector = curTarget - tr.localPosition;
			tr.localPosition += vector * num2;
			return;
		}
		Vector2 vector2 = curTarget - tr.localPosition;
		if (vector2.x > 10000f)
		{
			vector2.x = 0f;
		}
		if (vector2.y > 10000f)
		{
			vector2.y = 0f;
		}
		tr.localPosition += (Vector3)(vector2 * num2);
	}
}
