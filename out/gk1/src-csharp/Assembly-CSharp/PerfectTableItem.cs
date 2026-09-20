using UnityEngine;

[ExecuteInEditMode]
public class PerfectTableItem : MonoBehaviour
{
	private Transform _tf;

	public Transform tf
	{
		get
		{
			if (_tf == null)
			{
				_tf = base.transform;
			}
			return _tf;
		}
	}

	private void Update()
	{
		Vector2 vector = tf.localPosition;
		vector.x = (float)Mathf.RoundToInt(vector.x * 10f) / 10f;
		vector.y = (float)Mathf.RoundToInt(vector.y * 10f) / 10f;
		tf.localPosition = vector;
		tf.localScale = Vector3.one;
	}
}
