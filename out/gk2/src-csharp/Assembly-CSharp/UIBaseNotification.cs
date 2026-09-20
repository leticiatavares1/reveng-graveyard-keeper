using UnityEngine;

public abstract class UIBaseNotification : MonoBehaviour
{
	private const float HEIGHT = 58f;

	public float displayingTime = 4f;

	public float CurrentTime { get; set; }

	public bool IsTimerActive { get; set; }

	public RectTransform RectTransform => base.transform as RectTransform;

	public abstract void Draw();

	public abstract void ReleaseToPool();
}
