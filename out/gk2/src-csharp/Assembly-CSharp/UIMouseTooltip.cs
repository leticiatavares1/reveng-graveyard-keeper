using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMouseTooltip : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private string lngId;

	[SerializeField]
	private RectTransform overrideTarget;

	[SerializeField]
	private float left;

	[SerializeField]
	private float right;

	[SerializeField]
	private float top;

	[SerializeField]
	private float bottom;

	[SerializeField]
	private Vector2 appearOffset;

	[SerializeField]
	private string headerLngId;

	private Action customShow;

	private RectTransform Target
	{
		get
		{
			if (!(overrideTarget != null))
			{
				return base.transform as RectTransform;
			}
			return overrideTarget;
		}
	}

	public static bool IsAvailable => !LazyInput.IsGamepadActive;

	public static UIMouseTooltip Attach(GameObject go, string lngId, RectTransform overrideTarget = null, bool addRaycastTarget = false, bool disableChildRaycasts = false, UIMouseTooltipEdges edges = default(UIMouseTooltipEdges), Vector2 appearOffset = default(Vector2), string headerLngId = null)
	{
		if (go == null)
		{
			return null;
		}
		UIMouseTooltip uIMouseTooltip = go.GetComponent<UIMouseTooltip>();
		if (uIMouseTooltip == null)
		{
			uIMouseTooltip = go.AddComponent<UIMouseTooltip>();
			uIMouseTooltip.lngId = lngId;
		}
		else
		{
			uIMouseTooltip.SetLocalizationId(lngId);
		}
		uIMouseTooltip.overrideTarget = overrideTarget;
		uIMouseTooltip.left = edges.left;
		uIMouseTooltip.right = edges.right;
		uIMouseTooltip.top = edges.top;
		uIMouseTooltip.bottom = edges.bottom;
		uIMouseTooltip.appearOffset = appearOffset;
		uIMouseTooltip.headerLngId = headerLngId;
		uIMouseTooltip.customShow = null;
		if (addRaycastTarget)
		{
			EnsureRaycastTarget(go);
		}
		if (disableChildRaycasts)
		{
			DisableChildRaycasts(go);
		}
		uIMouseTooltip.ApplyEdgePadding();
		return uIMouseTooltip;
	}

	public static UIMouseTooltip AttachCustom(GameObject go, Action show, bool addRaycastTarget = false, bool disableChildRaycasts = false, UIMouseTooltipEdges edges = default(UIMouseTooltipEdges), Vector2 appearOffset = default(Vector2))
	{
		UIMouseTooltip uIMouseTooltip = Attach(go, string.Empty, null, addRaycastTarget, disableChildRaycasts, edges, appearOffset);
		if (uIMouseTooltip != null)
		{
			uIMouseTooltip.customShow = show;
		}
		return uIMouseTooltip;
	}

	public void SetLocalizationId(string lngId)
	{
		if (!(this.lngId == lngId))
		{
			HideTooltip(immediately: true);
			this.lngId = lngId;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		TryShowTooltip();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (eventData.fullyExited)
		{
			HideTooltip(immediately: false);
			TryShowUnderPointer(eventData);
		}
	}

	private void OnEnable()
	{
		LazyInput.OnInputChanged += OnInputChanged;
		GameSettings.OnLanguageChanged += OnLanguageChanged;
		ApplyEdgePadding();
	}

	private void OnDisable()
	{
		LazyInput.OnInputChanged -= OnInputChanged;
		GameSettings.OnLanguageChanged -= OnLanguageChanged;
		HideTooltip(immediately: true);
	}

	private void OnInputChanged()
	{
		if (!IsAvailable)
		{
			HideTooltip(immediately: true);
		}
	}

	private void OnLanguageChanged()
	{
		HideTooltip(immediately: true);
	}

	private void TryShowTooltip()
	{
		if (customShow != null)
		{
			if (IsAvailable)
			{
				customShow();
			}
		}
		else
		{
			TryShow(Target, lngId, GetResolvedAppearOffset(), headerLngId);
		}
	}

	public Vector2 GetResolvedAppearOffset()
	{
		return new Vector2(appearOffset.x, appearOffset.y - top);
	}

	private void HideTooltip(bool immediately)
	{
		HideIfShowingAt(Target, immediately);
	}

	public static bool TryShow(RectTransform target, string lngId, Vector2 appearOffset = default(Vector2), string headerLngId = null)
	{
		if (target == null || !IsAvailable)
		{
			return false;
		}
		if (!LL.HasLocalizedValueForCurrentLang(lngId))
		{
			return false;
		}
		string header = null;
		if (!string.IsNullOrEmpty(headerLngId) && LL.HasLocalizedValueForCurrentLang(headerLngId))
		{
			header = LLBase.L(headerLngId);
		}
		UITooltip.ShowSimpleInfo(target, LLBase.L(lngId), appearOffset, header);
		return true;
	}

	public static void HideIfShowingAt(RectTransform target, bool immediately)
	{
		if (!(target == null) && UITooltip.IsTooltipShowingAtTarget(target))
		{
			if (immediately)
			{
				UITooltip.HideImmediately();
			}
			else
			{
				UITooltip.Hide();
			}
		}
	}

	private void TryShowUnderPointer(PointerEventData eventData)
	{
		GameObject gameObject = eventData.pointerCurrentRaycast.gameObject;
		if (!(gameObject == null))
		{
			UIMouseTooltip componentInParent = gameObject.GetComponentInParent<UIMouseTooltip>();
			if (!(componentInParent == null) && !(componentInParent == this) && componentInParent.isActiveAndEnabled)
			{
				componentInParent.TryShowTooltip();
			}
		}
	}

	public static RectTransform GetOrCreateOverlay(RectTransform parent, string name)
	{
		if (parent == null)
		{
			return null;
		}
		if (parent.Find(name) is RectTransform result)
		{
			return result;
		}
		GameObject obj = new GameObject(name);
		RectTransform rectTransform = obj.AddComponent<RectTransform>();
		rectTransform.SetParent(parent, worldPositionStays: false);
		obj.AddComponent<LayoutElement>().ignoreLayout = true;
		return rectTransform;
	}

	public static void FitOverlayToWorldRects(RectTransform overlay, IReadOnlyList<RectTransform> targets)
	{
		if (overlay == null || targets == null || targets.Count == 0)
		{
			return;
		}
		Vector3[] array = new Vector3[4];
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		float num4 = float.MinValue;
		Vector3 vector = overlay.position;
		bool flag = false;
		for (int i = 0; i < targets.Count; i++)
		{
			if (!(targets[i] == null))
			{
				targets[i].GetWorldCorners(array);
				num = Mathf.Min(num, array[0].x, array[1].x);
				num3 = Mathf.Max(num3, array[2].x, array[3].x);
				num2 = Mathf.Min(num2, array[0].y, array[3].y);
				num4 = Mathf.Max(num4, array[1].y, array[2].y);
				vector = array[0];
				flag = true;
			}
		}
		if (flag)
		{
			overlay.pivot = new Vector2(0.5f, 0.5f);
			overlay.anchorMin = new Vector2(0.5f, 0.5f);
			overlay.anchorMax = new Vector2(0.5f, 0.5f);
			overlay.position = new Vector3((num + num3) * 0.5f, (num2 + num4) * 0.5f, vector.z);
			Vector3 lossyScale = overlay.lossyScale;
			float x = Mathf.Abs(num3 - num) / Mathf.Max(Mathf.Abs(lossyScale.x), 0.0001f);
			float y = Mathf.Abs(num4 - num2) / Mathf.Max(Mathf.Abs(lossyScale.y), 0.0001f);
			overlay.sizeDelta = new Vector2(x, y);
		}
	}

	public static void FitOverlayExtendRight(RectTransform overlay, RectTransform source, float widthMultiplier)
	{
		if (!(overlay == null) && !(source == null))
		{
			Vector3[] array = new Vector3[4];
			source.GetWorldCorners(array);
			float num = Mathf.Min(array[0].x, array[1].x, array[2].x, array[3].x);
			float num2 = Mathf.Max(array[0].x, array[1].x, array[2].x, array[3].x);
			float num3 = Mathf.Min(array[0].y, array[1].y, array[2].y, array[3].y);
			float num4 = Mathf.Max(array[0].y, array[1].y, array[2].y, array[3].y);
			Vector3 lossyScale = overlay.lossyScale;
			float x = Mathf.Abs(num2 - num) * widthMultiplier / Mathf.Max(Mathf.Abs(lossyScale.x), 0.0001f);
			float y = Mathf.Abs(num4 - num3) / Mathf.Max(Mathf.Abs(lossyScale.y), 0.0001f);
			overlay.pivot = new Vector2(0f, 0.5f);
			overlay.anchorMin = new Vector2(0.5f, 0.5f);
			overlay.anchorMax = new Vector2(0.5f, 0.5f);
			overlay.position = new Vector3(num, (num3 + num4) * 0.5f, array[0].z);
			overlay.sizeDelta = new Vector2(x, y);
		}
	}

	public static void FitOverlayToPreferredSize(RectTransform overlay, RectTransform source)
	{
		if (!(overlay == null) && !(source == null))
		{
			overlay.pivot = new Vector2(0.5f, 0.5f);
			overlay.anchorMin = new Vector2(0.5f, 0.5f);
			overlay.anchorMax = new Vector2(0.5f, 0.5f);
			overlay.anchoredPosition = Vector2.zero;
			overlay.sizeDelta = new Vector2(Mathf.Max(LayoutUtility.GetPreferredWidth(source), 1f), Mathf.Max(LayoutUtility.GetPreferredHeight(source), 1f));
		}
	}

	private void ApplyEdgePadding()
	{
		Graphic component = GetComponent<Image>();
		if (component == null)
		{
			component = GetComponent<Graphic>();
		}
		if (!(component == null))
		{
			component.raycastPadding = new Vector4(left, bottom, right, top);
		}
	}

	private static void EnsureRaycastTarget(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		Graphic component = go.GetComponent<Graphic>();
		if (component == null)
		{
			Image image = go.AddComponent<Image>();
			if (!(image == null))
			{
				image.color = Color.clear;
				image.raycastTarget = true;
			}
		}
		else
		{
			component.raycastTarget = true;
		}
	}

	private static void DisableChildRaycasts(GameObject go)
	{
		Graphic[] componentsInChildren = go.GetComponentsInChildren<Graphic>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!(componentsInChildren[i].gameObject == go))
			{
				componentsInChildren[i].raycastTarget = false;
			}
		}
	}
}
