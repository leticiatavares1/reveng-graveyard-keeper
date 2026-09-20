using UnityEngine;

public class BubbleCornerPoint : MonoBehaviour
{
	public Transform top_parent_tf;

	public Vector2 shift;

	public Vector2 pixel_shift;

	private Transform _tf;

	public SimpleUITable.Alignment bubble_custom_align = SimpleUITable.Alignment.NotSet;

	public void Init(Transform top_parent)
	{
		_tf = base.transform;
		top_parent_tf = top_parent;
	}

	public void CalcShift(Camera gui_cam)
	{
		if (_tf == null)
		{
			_tf = base.transform;
		}
		if (!(top_parent_tf == null))
		{
			shift = top_parent_tf.position - _tf.position;
			pixel_shift = gui_cam.WorldToScreenPoint(shift) - gui_cam.WorldToScreenPoint(Vector3.zero);
		}
	}
}
