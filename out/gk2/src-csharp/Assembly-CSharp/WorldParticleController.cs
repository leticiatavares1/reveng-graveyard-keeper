using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[DisallowMultipleComponent]
[ExecuteAlways]
public class WorldParticleController : MonoBehaviour
{
	[Serializable]
	private class ParticleSystemCache
	{
		public ParticleSystem.MinMaxCurve originalEmissionRate;

		public ParticleSystem.MinMaxGradient originalStartColor;

		public ParticleSystem.MinMaxCurve originalStartRotation;

		public ParticleSystem.MinMaxCurve originalStartRotationX;

		public ParticleSystem.MinMaxCurve originalStartRotationY;

		public ParticleSystem.MinMaxCurve originalStartRotationZ;

		public bool startSize3D;

		public ParticleSystem.MinMaxCurve originalStartSize;

		public ParticleSystem.MinMaxCurve originalStartSizeX;

		public ParticleSystem.MinMaxCurve originalStartSizeY;

		public ParticleSystem.MinMaxCurve originalStartSizeZ;

		public ParticleSystem.MinMaxCurve originalVelocityX;

		public ParticleSystem.MinMaxCurve originalVelocityY;

		public ParticleSystem.MinMaxCurve originalVelocityZ;
	}

	public static List<WorldParticleController> activeControllers = new List<WorldParticleController>();

	[SerializeField]
	private ParticleSystem particleSystem;

	[SerializeReference]
	[Space]
	private List<CPWParticleWind> particleCustomParametersByWind = new List<CPWParticleWind>();

	private ParticleSystemCache originalParameters;

	private bool wereParametersApplied;

	public static void UpdateParameters()
	{
		for (int i = 0; i < activeControllers.Count; i++)
		{
			activeControllers[i].UpdateParameters_Internal();
		}
	}

	private void UpdateParameters_Internal()
	{
		if (wereParametersApplied)
		{
			RestoreOriginalParameters();
		}
		for (int i = 0; i < particleCustomParametersByWind.Count; i++)
		{
			if (!particleCustomParametersByWind[i].DoWindAffection(WeatherSystem.Instance.WindValue))
			{
				Debug.LogError("CPWParticleWind failed to affect particle system: " + base.gameObject.name, this);
			}
		}
		wereParametersApplied = true;
	}

	private void Awake()
	{
		if (particleSystem == null)
		{
			TryGetComponent<ParticleSystem>(out particleSystem);
		}
		foreach (CPWParticleWind item in particleCustomParametersByWind)
		{
			if (item != null && !item.particleSystem)
			{
				item.particleSystem = particleSystem;
			}
		}
		CacheOriginalParameters();
	}

	private void OnEnable()
	{
		activeControllers.Add(this);
		UpdateParameters_Internal();
	}

	private void OnDisable()
	{
		activeControllers.Remove(this);
	}

	private void CacheOriginalParameters(bool force = false)
	{
		if (originalParameters == null || force)
		{
			ParticleSystemCache particleSystemCache = new ParticleSystemCache();
			particleSystemCache.originalEmissionRate = particleSystem.emission.rateOverTime;
			ParticleSystem.MainModule main = particleSystem.main;
			particleSystemCache.originalStartColor = main.startColor;
			particleSystemCache.originalStartRotation = main.startRotation;
			particleSystemCache.originalStartRotationX = main.startRotationX;
			particleSystemCache.originalStartRotationY = main.startRotationY;
			particleSystemCache.originalStartRotationZ = main.startRotationZ;
			particleSystemCache.startSize3D = main.startSize3D;
			particleSystemCache.originalStartSize = main.startSize;
			particleSystemCache.originalStartSizeX = main.startSizeX;
			particleSystemCache.originalStartSizeY = main.startSizeY;
			particleSystemCache.originalStartSizeZ = main.startSizeZ;
			ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
			if (velocityOverLifetime.enabled)
			{
				particleSystemCache.originalVelocityX = velocityOverLifetime.x;
				particleSystemCache.originalVelocityY = velocityOverLifetime.y;
				particleSystemCache.originalVelocityZ = velocityOverLifetime.z;
			}
			originalParameters = particleSystemCache;
		}
	}

	private void RestoreOriginalParameters()
	{
		if (particleSystem == null)
		{
			return;
		}
		ParticleSystemCache particleSystemCache = originalParameters;
		if (particleSystemCache != null)
		{
			ParticleSystem.EmissionModule emission = particleSystem.emission;
			emission.rateOverTime = particleSystemCache.originalEmissionRate;
			ParticleSystem.MainModule main = particleSystem.main;
			main.startColor = particleSystemCache.originalStartColor;
			main.startRotation = particleSystemCache.originalStartRotation;
			main.startRotationX = particleSystemCache.originalStartRotationX;
			main.startRotationY = particleSystemCache.originalStartRotationY;
			main.startRotationZ = particleSystemCache.originalStartRotationZ;
			main.startSize3D = particleSystemCache.startSize3D;
			if (!particleSystemCache.startSize3D)
			{
				main.startSize = particleSystemCache.originalStartSize;
			}
			else
			{
				main.startSizeX = particleSystemCache.originalStartSizeX;
				main.startSizeY = particleSystemCache.originalStartSizeY;
				main.startSizeZ = particleSystemCache.originalStartSizeZ;
			}
			ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
			if (velocityOverLifetime.enabled)
			{
				velocityOverLifetime.x = particleSystemCache.originalVelocityX;
				velocityOverLifetime.y = particleSystemCache.originalVelocityY;
				velocityOverLifetime.z = particleSystemCache.originalVelocityZ;
			}
		}
	}
}
