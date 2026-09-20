using TMPro;
using UnityEngine;

namespace LazyBearTechnology;

[RequireComponent(typeof(RectTransform))]
public class LocalizedVerticalOffset : MonoBehaviour
{
	[SerializeField]
	private bool japanese = true;

	[SerializeField]
	private bool korean = true;

	[SerializeField]
	private bool chineseSimplified = true;

	[SerializeField]
	private int verticalOffset;

	[SerializeField]
	[Tooltip("If enabled, applies the offset to the TextMesh Pro Extra Settings top margin instead of moving the RectTransform.")]
	private bool useFontExtraSettings;

	private RectTransform rectTransform;

	private TMP_Text tmpText;

	private float originalValue;

	private bool originalStored;

	private Vector4? originalMargin;

	private bool IsVerticallyStretched => rectTransform.anchorMin.y != rectTransform.anchorMax.y;

	private void Awake()
	{
		Apply();
	}

	private void OnEnable()
	{
		Apply();
	}

	public void Apply()
	{
		if (!TryResolveTarget())
		{
			return;
		}
		if (useFontExtraSettings)
		{
			Vector4 valueOrDefault = originalMargin.GetValueOrDefault();
			if (!originalMargin.HasValue)
			{
				valueOrDefault = tmpText.margin;
				originalMargin = valueOrDefault;
			}
			Vector4 value = originalMargin.Value;
			if (ShouldApplyOffset())
			{
				value.y += verticalOffset;
			}
			tmpText.margin = value;
		}
		else
		{
			if (!originalStored)
			{
				originalValue = GetCurrentValue();
				originalStored = true;
			}
			SetCurrentValue(ShouldApplyOffset() ? (originalValue + (float)verticalOffset) : originalValue);
		}
	}

	private bool ShouldApplyOffset()
	{
		return LLBase.CurrentLang switch
		{
			"ja" => japanese, 
			"ko" => korean, 
			"zh_cn" => chineseSimplified, 
			_ => false, 
		};
	}

	private bool TryResolveTarget()
	{
		if (useFontExtraSettings)
		{
			if (tmpText == null)
			{
				tmpText = GetComponent<TMP_Text>();
			}
			return tmpText != null;
		}
		if (rectTransform == null)
		{
			rectTransform = (RectTransform)base.transform;
		}
		return true;
	}

	private float GetCurrentValue()
	{
		if (!IsVerticallyStretched)
		{
			return rectTransform.anchoredPosition.y;
		}
		return rectTransform.offsetMax.y;
	}

	private void SetCurrentValue(float value)
	{
		if (IsVerticallyStretched)
		{
			Vector2 offsetMax = rectTransform.offsetMax;
			offsetMax.y = value;
			rectTransform.offsetMax = offsetMax;
		}
		else
		{
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			anchoredPosition.y = value;
			rectTransform.anchoredPosition = anchoredPosition;
		}
	}
}
