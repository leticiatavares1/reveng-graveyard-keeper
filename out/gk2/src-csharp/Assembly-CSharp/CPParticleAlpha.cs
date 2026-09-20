using System;
using UnityEngine;

[Serializable]
public class CPParticleAlpha : ControllableParameter
{
	public float alpha = 255f;

	public ParticleSystem particleSystem;

	public override void UpdateParameter(float v, WeatherComponent weatherComponent)
	{
		ParticleSystem.MainModule main = particleSystem.main;
		Color color = main.startColor.color;
		color.a = v * (alpha / 255f);
		main.startColor = color;
	}
}
