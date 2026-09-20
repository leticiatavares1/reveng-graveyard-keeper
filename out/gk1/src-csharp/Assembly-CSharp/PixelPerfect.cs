using UnityEngine;

public class PixelPerfect : MonoBehaviour
{
	public int pixel_k = 2;

	public bool only_in_editor = true;

	public void OnBecameVisible()
	{
		if (Application.isPlaying && only_in_editor)
		{
			base.enabled = false;
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			DoPixelPerfect();
		}
	}

	public void Update()
	{
		if (!Application.isPlaying || !only_in_editor)
		{
			DoPixelPerfect();
		}
	}

	private void DoPixelPerfect(bool force = false)
	{
		Transform transform = base.transform;
		Vector3 localPosition = transform.localPosition;
		Vector3 vector = localPosition;
		int num = 96 / pixel_k;
		localPosition.x = Mathf.Round(localPosition.x * (float)num) / (float)num;
		localPosition.y = Mathf.Round(localPosition.y * (float)num) / (float)num;
		if ((double)Mathf.Abs(localPosition.x) < 1E-05)
		{
			localPosition.x = 0f;
		}
		if ((double)Mathf.Abs(localPosition.y) < 1E-05)
		{
			localPosition.y = 0f;
		}
		if (force || !((double)(vector - localPosition).sqrMagnitude < 0.0001))
		{
			transform.localPosition = localPosition;
		}
	}

	public void OnValidate()
	{
		DoPixelPerfect(force: true);
	}
}
