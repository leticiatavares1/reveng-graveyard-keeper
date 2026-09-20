using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GroundSprite : MonoBehaviour
{
	private void OnDrawGizmosSelected()
	{
		EnsureCorrectScaleForHorizontalSprite(base.transform);
	}

	private void EnsureCorrectScaleForHorizontalSprite(Transform t)
	{
		Vector3 localScale = t.localScale;
		if (localScale.y > 0f || localScale.z > 0f)
		{
			SetCorrectLocalScaleForHorizontalSprite(t);
		}
	}

	private void SetCorrectLocalScaleForHorizontalSprite(Transform t)
	{
		Vector3 localScale = t.localScale;
		localScale.y = 1.25f;
		localScale.z = 1f;
		t.localScale = localScale;
	}
}
