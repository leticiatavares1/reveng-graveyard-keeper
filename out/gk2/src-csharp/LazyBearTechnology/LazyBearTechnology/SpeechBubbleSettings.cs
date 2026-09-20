using UnityEngine;

namespace LazyBearTechnology;

[CreateAssetMenu(fileName = "SpeechBubbleSettings", menuName = "Lazy/SpeechBubbleSettings", order = 1)]
public class SpeechBubbleSettings : ScriptableObject
{
	public float preferredWidth = 150f;

	public float showTime = 2.5f;

	public float oneSymbolTime = 0.08449999f;

	public float easternSymbolTimeCoef = 4f;

	public float letterAnimAppearTime = 0.02f;

	public float fadeTime = 0.35f;

	public float voiceAdditionalAverageTime = 0.6f;

	public float CalculateBubbleShowingTime(string displayingText)
	{
		float num = showTime;
		float num2 = (float)displayingText.Length * oneSymbolTime;
		if (LLBase.IsEastern())
		{
			num2 *= easternSymbolTimeCoef;
		}
		return num + num2;
	}
}
