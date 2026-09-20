using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LazyBearTechnology;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedTextMargins : MonoBehaviour
{
	[Serializable]
	private class MarginsData
	{
		[SerializeField]
		private float left;

		[SerializeField]
		private float top;

		[SerializeField]
		private float right;

		[SerializeField]
		private float bottom;

		public Vector4 Value => new Vector4(left, top, right, bottom);
	}

	[Serializable]
	private class LocalizedMarginsData
	{
		[SerializeField]
		private string languageId;

		[SerializeField]
		private MarginsData margins;

		public string LanguageId => languageId;

		public MarginsData Margins => margins;
	}

	[SerializeField]
	private MarginsData defaultMargins = new MarginsData();

	[SerializeField]
	private List<LocalizedMarginsData> localizedMargins = new List<LocalizedMarginsData>();

	private TMP_Text label;

	private TMP_Text Label
	{
		get
		{
			if (label == null)
			{
				label = GetComponent<TMP_Text>();
			}
			return label;
		}
	}

	private void OnEnable()
	{
		ApplyMargins();
	}

	public void ApplyMargins()
	{
		Label.margin = GetMarginsForCurrentLanguage();
		Label.ForceMeshUpdate();
		LayoutRebuilder.ForceRebuildLayoutImmediate(Label.rectTransform);
	}

	private Vector4 GetMarginsForCurrentLanguage()
	{
		string currentLang = LLBase.CurrentLang;
		foreach (LocalizedMarginsData localizedMargin in localizedMargins)
		{
			if (localizedMargin != null && localizedMargin.Margins != null && !(localizedMargin.LanguageId != currentLang))
			{
				return localizedMargin.Margins.Value;
			}
		}
		return defaultMargins.Value;
	}
}
