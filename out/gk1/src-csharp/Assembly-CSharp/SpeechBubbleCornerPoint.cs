using UnityEngine;

[ExecuteInEditMode]
public class SpeechBubbleCornerPoint : MonoBehaviour
{
	[HideInInspector]
	public Transform tf;

	[HideInInspector]
	public Transform top_parent_tf;

	[HideInInspector]
	public Vector2 shift;

	[HideInInspector]
	public Vector2 pixel_shift;

	private void Start()
	{
		Log();
	}

	private void OnEnable()
	{
		Log();
	}

	public void Init(Transform top_parent)
	{
		tf = base.transform;
		top_parent_tf = top_parent;
	}

	public void CalcShift(Camera gui_cam)
	{
		shift = top_parent_tf.position - tf.position;
		pixel_shift = gui_cam.WorldToScreenPoint(shift) - gui_cam.WorldToScreenPoint(Vector3.zero);
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			Log();
		}
	}

	private void Log()
	{
		Debug.LogError("SpeechBubbleCornerPoint is obsolete, Use BubbleCornerPoint instead", base.gameObject);
	}
}
