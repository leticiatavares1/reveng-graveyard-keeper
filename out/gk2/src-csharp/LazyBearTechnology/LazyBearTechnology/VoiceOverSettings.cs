using UnityEngine;

namespace LazyBearTechnology;

[CreateAssetMenu(fileName = "VoiceOverSettings", menuName = "Lazy/VoiceOverSettings", order = 1)]
public class VoiceOverSettings : LazySingletonSO<VoiceOverSettings>
{
	private const float DEFAULT_TIME = 0.2f;

	private const string DEFAULT_VOICE_OVERS_LABEL = "Voiceovers";

	public float additionalClipLength = 0.2f;

	public float startPause = 0.2f;

	public string voiceOversLabel = "Voiceovers";

	public static bool IsEnabled { get; set; } = true;


	public static string LanguageId { get; set; } = "en";


	public static float AdditionalClipLength
	{
		get
		{
			if (!(LazySingletonSO<VoiceOverSettings>.Instance != null))
			{
				return 0.2f;
			}
			return Mathf.Max(0f, LazySingletonSO<VoiceOverSettings>.Instance.additionalClipLength);
		}
	}

	public static float StartPause
	{
		get
		{
			if (!(LazySingletonSO<VoiceOverSettings>.Instance != null))
			{
				return 0.2f;
			}
			return Mathf.Max(0f, LazySingletonSO<VoiceOverSettings>.Instance.startPause);
		}
	}

	public static string VoiceOversLabel
	{
		get
		{
			if (!(LazySingletonSO<VoiceOverSettings>.Instance != null) || string.IsNullOrWhiteSpace(LazySingletonSO<VoiceOverSettings>.Instance.voiceOversLabel))
			{
				return "Voiceovers";
			}
			return LazySingletonSO<VoiceOverSettings>.Instance.voiceOversLabel;
		}
	}
}
