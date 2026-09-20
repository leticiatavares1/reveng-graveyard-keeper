using System.Collections.Generic;
using LazyBearTechnology;
using SoftMasking;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubbleCornerFadeMask : MonoBehaviour
{
	private const string MaskObjectName = "CornerOverlapMask";

	private Image background;

	private List<UIBubbleCorner> corners;

	private SoftMask cornerPunchMask;

	public static SpeechBubbleCornerFadeMask Ensure(Image background, List<UIBubbleCorner> corners, GameObject host)
	{
		SpeechBubbleCornerFadeMask speechBubbleCornerFadeMask = host.GetComponent<SpeechBubbleCornerFadeMask>();
		if (speechBubbleCornerFadeMask == null)
		{
			speechBubbleCornerFadeMask = host.AddComponent<SpeechBubbleCornerFadeMask>();
		}
		speechBubbleCornerFadeMask.background = background;
		speechBubbleCornerFadeMask.corners = corners;
		speechBubbleCornerFadeMask.EnsureHierarchy();
		return speechBubbleCornerFadeMask;
	}

	public void Sync(UIBubbleCorner currentCorner)
	{
		if (cornerPunchMask == null)
		{
			EnsureHierarchy();
		}
		if (!(cornerPunchMask == null))
		{
			Image activeCornerImage = GetActiveCornerImage(currentCorner);
			bool flag = activeCornerImage != null && activeCornerImage.sprite != null && activeCornerImage.enabled;
			cornerPunchMask.enabled = flag;
			if (flag)
			{
				cornerPunchMask.sprite = activeCornerImage.sprite;
				cornerPunchMask.spriteBorderMode = SoftMask.BorderMode.Simple;
				cornerPunchMask.spritePixelsPerUnitMultiplier = activeCornerImage.pixelsPerUnitMultiplier;
				cornerPunchMask.separateMask = activeCornerImage.rectTransform;
			}
		}
	}

	private void EnsureHierarchy()
	{
		if (background == null || corners == null || corners.Count == 0)
		{
			return;
		}
		Transform parent = background.rectTransform.parent;
		SoftMask softMask = ((parent != null) ? parent.GetComponent<SoftMask>() : null);
		if (softMask == null && parent != null)
		{
			Transform transform = parent.Find("CornerOverlapMask");
			if (transform != null)
			{
				softMask = transform.GetComponent<SoftMask>();
			}
		}
		Transform transform2 = ((softMask != null) ? softMask.transform.parent : parent);
		if (!(transform2 == null))
		{
			if (softMask != null)
			{
				cornerPunchMask = softMask;
				ApplyHierarchy(transform2);
				return;
			}
			GameObject gameObject = new GameObject("CornerOverlapMask", typeof(RectTransform), typeof(LayoutElement), typeof(SoftMask));
			RectTransform component = gameObject.GetComponent<RectTransform>();
			component.SetParent(transform2, worldPositionStays: false);
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.offsetMin = Vector2.zero;
			component.offsetMax = Vector2.zero;
			component.pivot = background.rectTransform.pivot;
			gameObject.GetComponent<LayoutElement>().ignoreLayout = true;
			cornerPunchMask = gameObject.GetComponent<SoftMask>();
			cornerPunchMask.source = SoftMask.MaskSource.Sprite;
			cornerPunchMask.invertMask = true;
			cornerPunchMask.invertOutsides = true;
			ApplyHierarchy(transform2);
		}
	}

	private void ApplyHierarchy(Transform container)
	{
		for (int i = 0; i < corners.Count; i++)
		{
			if (corners[i].rectTransform.parent != container)
			{
				corners[i].rectTransform.SetParent(container, worldPositionStays: false);
			}
		}
		if (background.rectTransform.parent != cornerPunchMask.transform)
		{
			background.rectTransform.SetParent(cornerPunchMask.transform, worldPositionStays: false);
		}
		cornerPunchMask.transform.SetAsFirstSibling();
		Image[] componentsInChildren = background.GetComponentsInChildren<Image>(includeInactive: true);
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (componentsInChildren[j] != background)
			{
				componentsInChildren[j].maskable = false;
			}
		}
	}

	private Image GetActiveCornerImage(UIBubbleCorner currentCorner)
	{
		if (currentCorner == null)
		{
			return null;
		}
		if (IsUsable(currentCorner.customImage2))
		{
			return currentCorner.customImage2;
		}
		if (IsUsable(currentCorner.customImage))
		{
			return currentCorner.customImage;
		}
		return null;
	}

	private static bool IsUsable(Image image)
	{
		if (image != null && image.enabled && image.gameObject.activeSelf)
		{
			return image.sprite != null;
		}
		return false;
	}
}
