using TMPro;
using UnityEngine;

namespace LazyBearTechnology;

[ExecuteInEditMode]
[RequireComponent(typeof(TMP_Text))]
public class TextStyleComponent : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private TextStyle textStyle;

	[SerializeField]
	private bool staticFont;

	[SerializeField]
	private bool hasCustomColor;

	[SerializeField]
	private Color customColor = Color.white;

	[SerializeField]
	private bool hasCustomOutlineColor;

	[SerializeField]
	private Color customOutlineColor = Color.white;

	[SerializeField]
	private bool increaseLineSpacingForAsianFonts;

	[SerializeField]
	private int asianFontsLineSpacingIncrease = 12;

	[SerializeField]
	private bool icnIfChinese = true;

	[SerializeField]
	private bool icnIfJapanese;

	[SerializeField]
	private bool icnIfKorean;

	private float? originalLineSpacing;

	private TMP_Text label;

	public TextStyle CurrentTextStyle => textStyle;

	private void Awake()
	{
		if (label == null)
		{
			label = GetComponent<TMP_Text>();
		}
		TryCaptureOriginalLineSpacing();
	}

	public static void RefreshAll()
	{
		TextStyleComponent[] array = Object.FindObjectsByType<TextStyleComponent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ApplyStyle();
		}
	}

	public void ApplyStyle()
	{
		if (textStyle == null)
		{
			if (Application.isPlaying)
			{
				Debug.LogError("Null style on go:[" + base.gameObject.name + "]", this);
			}
			return;
		}
		if (label == null)
		{
			label = GetComponent<TMP_Text>();
		}
		textStyle.ApplyStyle(label, staticFont, hasCustomColor ? new Color?(customColor) : null, hasCustomOutlineColor ? new Color?(customOutlineColor) : null);
		TryCaptureOriginalLineSpacing();
		if (ShouldIncreaseLineSpacingForCurrentLanguage())
		{
			label.lineSpacing = originalLineSpacing.Value + (float)asianFontsLineSpacingIncrease;
		}
		else
		{
			label.lineSpacing = originalLineSpacing.Value;
		}
	}

	public void SetTextStyle(TextStyle newStyle)
	{
		if (newStyle == null)
		{
			Debug.LogWarning("Can't find TextStyle!");
		}
		else if (newStyle != textStyle)
		{
			textStyle = newStyle;
			ApplyStyle();
		}
	}

	private void OnEnable()
	{
		TryCaptureOriginalLineSpacing();
		if (textStyle != null)
		{
			ApplyStyle();
		}
	}

	private void TryCaptureOriginalLineSpacing()
	{
		if (!originalLineSpacing.HasValue)
		{
			if (label == null)
			{
				label = GetComponent<TMP_Text>();
			}
			if (label != null)
			{
				originalLineSpacing = label.lineSpacing;
			}
		}
	}

	private bool ShouldIncreaseLineSpacingForCurrentLanguage()
	{
		if (!increaseLineSpacingForAsianFonts)
		{
			return false;
		}
		switch (LLBase.CurrentLang)
		{
		case "zh_cn":
		case "zh_cht":
			return icnIfChinese;
		case "ja":
			return icnIfJapanese;
		case "ko":
			return icnIfKorean;
		default:
			return false;
		}
	}
}
