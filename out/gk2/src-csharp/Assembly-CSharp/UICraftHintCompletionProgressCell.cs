using UnityEngine;

public class UICraftHintCompletionProgressCell : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private RectTransform rectTransform;

	public CanvasGroup CanvasGroup
	{
		get
		{
			EnsureReferences();
			return canvasGroup;
		}
	}

	public RectTransform RectTransform
	{
		get
		{
			EnsureReferences();
			return rectTransform;
		}
	}

	public void ShowOver(RectTransform source, RectTransform parent)
	{
		EnsureReferences();
		CopyRectTransform(source, parent);
		SetAlpha(0f);
		base.gameObject.SetActive(value: true);
	}

	public void SetAlpha(float alpha)
	{
		canvasGroup.alpha = alpha;
	}

	private void EnsureReferences()
	{
		if (canvasGroup == null && !TryGetComponent<CanvasGroup>(out canvasGroup))
		{
			canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
		}
		if (rectTransform == null)
		{
			rectTransform = base.transform as RectTransform;
		}
	}

	private void CopyRectTransform(RectTransform source, RectTransform parent)
	{
		RectTransform obj = rectTransform;
		Vector2 anchorMin = (rectTransform.anchorMax = new Vector2(0.5f, 0.5f));
		obj.anchorMin = anchorMin;
		rectTransform.pivot = source.pivot;
		rectTransform.sizeDelta = source.rect.size;
		rectTransform.rotation = source.rotation;
		rectTransform.localScale = source.localScale;
		rectTransform.anchoredPosition = GetLocalCenter(source, parent);
	}

	private Vector2 GetLocalCenter(RectTransform source, RectTransform parent)
	{
		Vector3 position = source.TransformPoint(source.rect.center);
		return parent.InverseTransformPoint(position);
	}
}
