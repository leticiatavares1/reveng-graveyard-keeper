using UnityEngine;

[CreateAssetMenu(fileName = "VoiceClipProcessorSettings", menuName = "GK2/Audio/Voice Clip Processor Settings")]
public class VoiceClipProcessorSettings : ScriptableObject
{
	public const string ResourceName = "VoiceClipProcessorSettings";

	public const string DefaultAssetPath = "Assets/Resources/VoiceClipProcessorSettings.asset";

	public float frameDurationSeconds = 0.02f;

	[Range(0f, 1f)]
	public float speechLevelPercentile = 0.85f;

	[Range(0f, 1f)]
	public float silenceRelativeToSpeech = 0.12f;

	[Range(0f, 1f)]
	[Tooltip("Threshold for continuous trailing silence from the end of the file. Middle pauses are ignored.")]
	public float endSilenceRelativeToSpeech = 0.04f;

	[Tooltip("If detected end silence is shorter than this, it is ignored (set to 0).")]
	public float minEndSilenceSeconds = 0.1f;

	public int minConsecutiveSpeechFrames = 3;

	public int minConsecutiveSilenceFrames = 3;

	public float absoluteSilenceFloor = 0.001f;

	[Range(0f, 1f)]
	public float maxSilenceRatio = 0.9f;
}
