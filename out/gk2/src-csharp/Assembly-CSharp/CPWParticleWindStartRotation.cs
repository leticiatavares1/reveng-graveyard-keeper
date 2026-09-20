using System;
using UnityEngine;

[Serializable]
public class CPWParticleWindStartRotation : CPWParticleWind
{
	[Range(-1f, 1f)]
	public float xWindMultiplication;

	[Range(-1f, 1f)]
	public float yWindMultiplication;

	[Range(-1f, 1f)]
	public float zWindMultiplication;

	protected override void DoWindAffection_Internal(float windValue)
	{
		ParticleSystem.MainModule main = particleSystem.main;
		if (main.startRotation3D)
		{
			main.startRotationX = CPWParticleWind.AddToMinMaxCurve(main.startRotationX, xWindMultiplication * windValue);
			main.startRotationY = CPWParticleWind.AddToMinMaxCurve(main.startRotationY, yWindMultiplication * windValue);
			main.startRotationZ = CPWParticleWind.AddToMinMaxCurve(main.startRotationZ, zWindMultiplication * windValue);
		}
		else
		{
			main.startRotation = CPWParticleWind.AddToMinMaxCurve(main.startRotation, zWindMultiplication * windValue);
		}
	}
}
