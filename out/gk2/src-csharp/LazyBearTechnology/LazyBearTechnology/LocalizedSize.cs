using UnityEngine;

namespace LazyBearTechnology;

[RequireComponent(typeof(RectTransform))]
public class LocalizedSize : MonoBehaviour
{
	[SerializeField]
	private bool japanese = true;

	[SerializeField]
	private bool korean = true;

	[SerializeField]
	private bool chineseSimplified = true;

	[SerializeField]
	private float deltaX;

	[SerializeField]
	private float deltaY;

	private RectTransform rectTransform;

	private Vector2 originalSize;

	private bool originalStored;

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
		if (rectTransform == null)
		{
			rectTransform = (RectTransform)base.transform;
		}
		if (!originalStored)
		{
			originalSize = rectTransform.sizeDelta;
			originalStored = true;
		}
		rectTransform.sizeDelta = (ShouldApply() ? (originalSize + new Vector2(deltaX, deltaY)) : originalSize);
	}

	private bool ShouldApply()
	{
		string currentLang = LLBase.CurrentLang;
		if (LanguageModHooks.RequiresResize != null && LanguageModHooks.RequiresResize(currentLang))
		{
			return true;
		}
		return currentLang switch
		{
			"ja" => japanese, 
			"ko" => korean, 
			"zh_cn" => chineseSimplified, 
			_ => false, 
		};
	}
}
