using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(fileName = "FishingSettings", menuName = "ScriptableObjects/FishingSettings")]
public class FishingSettings : LazySingletonSerializedSO<FishingSettings>
{
	[Serializable]
	public class FishingCurve
	{
		[SerializeField]
		public string id;

		[SerializeField]
		public AnimationCurve curve;
	}

	[Serializable]
	public class FishDriftStateSettings
	{
		[Tooltip("Minimum oscillation amplitude")]
		[SerializeField]
		private float minAmplitude = 0.1f;

		[Tooltip("Maximum oscillation amplitude")]
		[SerializeField]
		private float maxAmplitude = 0.1f;

		[Tooltip("Minimum duration of this drift phase before re-randomizing")]
		[SerializeField]
		private float minDuration = 1f;

		[Tooltip("Maximum duration of this drift phase before re-randomizing")]
		[SerializeField]
		private float maxDuration = 2f;

		[Tooltip("Oscillation frequency (cycles per second)")]
		[SerializeField]
		private float frequency = 2f;

		[Tooltip("Minimum period of time between fish splashes")]
		[SerializeField]
		private float minSplashPeriodTime = 0.8f;

		[Tooltip("Axis direction for oscillation. Use (0,0,1) for Z only, (1,0,0) for X only, (1,0,1) for XZ plane")]
		[SerializeField]
		private Vector3 oscillationAxis = Vector3.forward;

		[Tooltip("If enabled, parameters (amplitude) will be re-randomized periodically based on duration. If disabled, oscillation continues unchanged while this preset is active.")]
		[SerializeField]
		private bool enableRandomization = true;

		public float MinAmplitude => minAmplitude;

		public float MaxAmplitude => maxAmplitude;

		public float MinDuration => minDuration;

		public float MaxDuration => maxDuration;

		public float Frequency => frequency;

		public float MinSplashPeriodTime => minSplashPeriodTime;

		public Vector3 OscillationAxis
		{
			get
			{
				if (!(oscillationAxis.sqrMagnitude > 0.001f))
				{
					return Vector3.zero;
				}
				return oscillationAxis.normalized;
			}
		}

		public bool EnableRandomization => enableRandomization;

		public static FishDriftStateSettings BothPullingPreset => new FishDriftStateSettings(0.4f, 0.4f, 0.35f, 0.8f, 7f, Vector3.forward);

		public static FishDriftStateSettings FishPullingOnlyPreset => new FishDriftStateSettings(0.25f, 0.25f, 0.6f, 1.2f, 4.25f, Vector3.forward);

		public static FishDriftStateSettings IdlePreset => new FishDriftStateSettings(0.1f, 0.1f, 1f, 2f, 2f, Vector3.forward);

		public FishDriftStateSettings()
		{
		}

		public FishDriftStateSettings(float minAmp, float maxAmp, float minDur, float maxDur, float freq, Vector3 axis, bool randomize = true)
		{
			minAmplitude = minAmp;
			maxAmplitude = maxAmp;
			minDuration = minDur;
			maxDuration = maxDur;
			frequency = freq;
			oscillationAxis = axis;
			enableRandomization = randomize;
		}

		public void CopyFrom(FishDriftStateSettings other)
		{
			minAmplitude = other.minAmplitude;
			maxAmplitude = other.maxAmplitude;
			minDuration = other.minDuration;
			maxDuration = other.maxDuration;
			frequency = other.frequency;
			minSplashPeriodTime = other.minSplashPeriodTime;
			oscillationAxis = other.oscillationAxis;
			enableRandomization = other.enableRandomization;
		}
	}

	[SerializeField]
	private List<FishingCurve> curvePresets;

	[SerializeField]
	private ReservoirConfig defaultReservoirConfig;

	[SerializeField]
	private float pullingProgressConst;

	[SerializeField]
	private float idleProgressConst;

	[SerializeField]
	private float curveMultiplier;

	[SerializeField]
	public Gradient ropeGradient;

	[SerializeField]
	public Gradient tensionBarGradient;

	[Tooltip("Sharp oscillations when both player and fish are pulling (high tension)")]
	[SerializeField]
	private FishDriftStateSettings bothPullingDrift = FishDriftStateSettings.BothPullingPreset;

	[Tooltip("Medium oscillations when only fish is pulling (fish resisting, player idle)")]
	[SerializeField]
	private FishDriftStateSettings fishPullingOnlyDrift = FishDriftStateSettings.FishPullingOnlyPreset;

	[Tooltip("Slow idle drift when nobody is pulling (calm state)")]
	[SerializeField]
	private FishDriftStateSettings idleDrift = FishDriftStateSettings.IdlePreset;

	[Tooltip("Duration for fade-out when fishing ends or oscillation stops (seconds)")]
	[SerializeField]
	private float driftFadeOutDuration = 0.35f;

	[Tooltip("Speed of frequency transition when switching presets (Hz per second)")]
	[SerializeField]
	private float frequencyTransitionSpeed = 10f;

	[Tooltip("Speed of axis transition when switching presets (units per second)")]
	[SerializeField]
	private float axisTransitionSpeed = 3f;

	public float PullingProgressConst => pullingProgressConst;

	public float IdleProgressConst => idleProgressConst;

	public float CurveMultiplier => curveMultiplier;

	public ReservoirConfig DefaultReservoirConfig => defaultReservoirConfig;

	public FishDriftStateSettings BothPullingDrift => bothPullingDrift;

	public FishDriftStateSettings FishPullingOnlyDrift => fishPullingOnlyDrift;

	public FishDriftStateSettings IdleDrift => idleDrift;

	public float DriftFadeOutDuration => driftFadeOutDuration;

	public float FrequencyTransitionSpeed => frequencyTransitionSpeed;

	public float AxisTransitionSpeed => axisTransitionSpeed;

	private void ResetDriftSettingsToDefaults()
	{
		bothPullingDrift = FishDriftStateSettings.BothPullingPreset;
		fishPullingOnlyDrift = FishDriftStateSettings.FishPullingOnlyPreset;
		idleDrift = FishDriftStateSettings.IdlePreset;
		driftFadeOutDuration = 0.35f;
		frequencyTransitionSpeed = 10f;
		axisTransitionSpeed = 3f;
	}

	public AnimationCurve GetCurveById(string fishingId)
	{
		if (curvePresets.Exists((FishingCurve x) => x.id == fishingId))
		{
			return curvePresets.Find((FishingCurve x) => x.id == fishingId).curve;
		}
		Debug.LogError("[FishingSettings]: curve with id '" + fishingId + "' not found");
		return null;
	}
}
