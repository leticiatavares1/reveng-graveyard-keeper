using System;
using UnityEngine;

[Serializable]
public class CPWParticleWindSize : CPWParticleWind
{
	public float xWindMultiplication;

	public float yWindMultiplication;

	public float zWindMultiplication;

	protected override void DoWindAffection_Internal(float windValue)
	{
		ParticleSystem.MainModule main = particleSystem.main;
		if (main.startSize3D)
		{
			main.startSizeX = CPWParticleWind.AddToMinMaxCurve(main.startSizeX, Mathf.Abs(xWindMultiplication * windValue));
			main.startSizeY = CPWParticleWind.AddToMinMaxCurve(main.startSizeY, Mathf.Abs(yWindMultiplication * windValue));
			main.startSizeZ = CPWParticleWind.AddToMinMaxCurve(main.startSizeZ, Mathf.Abs(zWindMultiplication * windValue));
		}
		else
		{
			main.startSize = CPWParticleWind.AddToMinMaxCurve(main.startSize, Mathf.Abs(xWindMultiplication * windValue));
		}
	}
}
