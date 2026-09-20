using System.Collections.Generic;
using System.Text;
using LazyBearTechnology;
using LinqTools;
using TMPro;
using UnityEngine;

public class LazyButtonTipsStr : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI label;

	private List<LazyGameKeyTip> cachedTips;

	private string cachedSeparator = "  ";

	private bool hasCachedTips;

	private void Awake()
	{
		if (label == null)
		{
			label = GetComponent<TextMeshProUGUI>();
		}
	}

	public void Clear()
	{
		hasCachedTips = false;
		cachedTips = null;
		label.text = string.Empty;
	}

	public void Print(params LazyGameKeyTip[] tips)
	{
		Print(tips.ToList());
	}

	public void Print(string separator, params LazyGameKeyTip[] tips)
	{
		Print(tips.ToList(), separator);
	}

	public void Print(List<LazyGameKeyTip> tips, string separator = "  ")
	{
		CacheTips(tips, separator);
		ApplyCachedTips();
	}

	public void Print(LazyGameKeyTip tip)
	{
		CacheTips(new List<LazyGameKeyTip> { tip }, "  ");
		ApplyCachedTips();
	}

	public static void RefreshAll()
	{
		LazyButtonTipsStr[] array = Object.FindObjectsByType<LazyButtonTipsStr>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RefreshFromCache();
		}
	}

	private void RefreshFromCache()
	{
		if (hasCachedTips && cachedTips != null && !(label == null))
		{
			ApplyCachedTips();
		}
	}

	private void CacheTips(List<LazyGameKeyTip> tips, string separator)
	{
		cachedTips = new List<LazyGameKeyTip>(tips);
		cachedSeparator = separator;
		hasCachedTips = true;
	}

	private void ApplyCachedTips()
	{
		if (!hasCachedTips || cachedTips == null || label == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < cachedTips.Count; i++)
		{
			string text = cachedTips[i].ToString();
			stringBuilder.Append(text);
			if (text.Length > 0 && i < cachedTips.Count - 1)
			{
				stringBuilder.Append(cachedSeparator);
			}
		}
		label.text = stringBuilder.ToString();
	}

	public void ApplyStyle(TextStyle style)
	{
		style.ApplyStyle(label);
	}
}
