using TMPro;
using UnityEngine;

namespace LazyBearTechnology;

public class LocalizedLabel : MonoBehaviour
{
	[SerializeField]
	public string langToken = string.Empty;

	[SerializeField]
	private TextStyle highlightColorType;

	private TextMeshProUGUI label;

	private bool isInitialized;

	public bool IgnoreLocalize { get; set; }

	private void Start()
	{
		Localize();
	}

	public void Localize()
	{
		if (!IgnoreLocalize)
		{
			label = GetComponent<TextMeshProUGUI>();
			isInitialized = true;
			if (string.IsNullOrEmpty(langToken))
			{
				Debug.LogError("LocalizedLabel token is empty", base.gameObject);
			}
			else
			{
				label.text = ((highlightColorType != null) ? highlightColorType.TranslateAndColorizeTags(langToken) : LLBase.L(langToken));
			}
		}
	}
}
