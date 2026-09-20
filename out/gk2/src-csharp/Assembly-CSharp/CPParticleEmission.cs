using System;
using UnityEngine;

[Serializable]
public class CPParticleEmission : ControllableParameter
{
	[SerializeField]
	private float defaultValue;

	public ParticleSystem particleSystem;

	public override void UpdateParameter(float v, WeatherComponent weatherComponent)
	{
		ParticleSystem.EmissionModule emission = particleSystem.emission;
		emission.rateOverTime = Mathf.Lerp(0f, defaultValue, v);
		particleSystem.gameObject.SetActive(v > 0f);
	}
}
