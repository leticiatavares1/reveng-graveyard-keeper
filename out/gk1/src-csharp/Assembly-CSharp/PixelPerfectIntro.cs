using UnityEngine;

[ExecuteInEditMode]
public class PixelPerfectIntro : MonoBehaviour
{
	public bool work_only_in_editor = true;

	public int pixel_size = 3;

	private void DoRound()
	{
		Vector3 localPosition = base.transform.localPosition;
		float num = (float)pixel_size / 48f / 3f;
		Vector3 vector = new Vector3(Mathf.Round(localPosition.x / num) * num, Mathf.Round(localPosition.y / num) * num, localPosition.z);
		if (!((double)(vector - base.transform.localPosition).magnitude < 0.001))
		{
			base.transform.localPosition = vector;
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
