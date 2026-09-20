using UnityEngine;

[ExecuteInEditMode]
public class PixelPerfectGUI : MonoBehaviour
{
	public bool work_only_in_editor = true;

	public int pixel_size = 2;

	private void DoRound()
	{
		Vector3 localPosition = base.transform.localPosition;
		Vector3 vector = new Vector3(Mathf.Round(localPosition.x / (float)pixel_size) * (float)pixel_size, Mathf.Round(localPosition.y / (float)pixel_size) * (float)pixel_size, localPosition.z);
		if (!((double)(vector - base.transform.localPosition).magnitude < 0.001))
		{
			base.transform.localPosition = vector;
			base.transform.localScale = Vector3.one;
		}
	}

	public void Update()
	{
		if (!Application.isPlaying)
		{
			DoRound();
		}
	}

	public void LateUpdate()
	{
		if (!Application.isPlaying || !work_only_in_editor)
		{
			DoRound();
		}
	}
}
