using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

namespace LazyBearTechnology;

[RequireComponent(typeof(TMP_Text))]
[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class ArabicTextPreprocessor : MonoBehaviour, ITextPreprocessor
{
	[Tooltip("Render tashkeel/harakat (diacritic marks). When off they are stripped from the text.")]
	[SerializeField]
	private bool tashkeel;

	private TMP_Text label;

	private ITextPreprocessor previous;

	public bool Tashkeel
	{
		get
		{
			return tashkeel;
		}
		set
		{
			if (tashkeel != value)
			{
				tashkeel = value;
				Reprocess();
			}
		}
	}

	private void Reprocess()
	{
		if (label != null && base.isActiveAndEnabled)
		{
			label.ForceMeshUpdate();
		}
	}

	public string PreprocessText(string text)
	{
		if (previous != null)
		{
			text = previous.PreprocessText(text);
		}
		return ArabicShaper.ShapeForRender(text, tashkeel);
	}

	private void OnEnable()
	{
		label = GetComponent<TMP_Text>();
		if (label == null)
		{
			return;
		}
		if (label.textPreprocessor != this)
		{
			previous = label.textPreprocessor;
		}
		label.textPreprocessor = this;
		List<OTL_FeatureTag> fontFeatures = label.fontFeatures;
		if (!fontFeatures.Contains(OTL_FeatureTag.mark) || !fontFeatures.Contains(OTL_FeatureTag.mkmk))
		{
			if (!fontFeatures.Contains(OTL_FeatureTag.mark))
			{
				fontFeatures.Add(OTL_FeatureTag.mark);
			}
			if (!fontFeatures.Contains(OTL_FeatureTag.mkmk))
			{
				fontFeatures.Add(OTL_FeatureTag.mkmk);
			}
			label.fontFeatures = fontFeatures;
		}
		label.ForceMeshUpdate();
	}

	private void OnDisable()
	{
		if (!(label == null) && label.textPreprocessor == this)
		{
			label.textPreprocessor = previous;
			previous = null;
			label.ForceMeshUpdate();
		}
	}
}
