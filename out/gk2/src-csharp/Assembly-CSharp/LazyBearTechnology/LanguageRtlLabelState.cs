using TMPro;
using UnityEngine;

namespace LazyBearTechnology;

internal sealed class LanguageRtlLabelState : MonoBehaviour
{
	public bool captured;

	public bool originalRtl;

	public TextAlignmentOptions originalAlignment;

	public ITextPreprocessor originalPreprocessor;
}
