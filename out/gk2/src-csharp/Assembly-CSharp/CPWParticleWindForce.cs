using System;
using UnityEngine;

[Serializable]
public class CPWParticleWindForce : CPWParticleWind
{
	public float xWindMultiplication;

	public float yWindMultiplication;

	public float zWindMultiplication;

	protected override void DoWindAffection_Internal(float windValue)
	{
		AddToForce(particleSystem.forceOverLifetime, new Vector3(xWindMultiplication, yWindMultiplication, zWindMultiplication) * windValue);
	}

	private static void AddToForce(ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule, Vector3 velocity)
	{
		if (forceOverLifetimeModule.enabled)
		{
			forceOverLifetimeModule.x = CPWParticleWind.AddToMinMaxCurve(forceOverLifetimeModule.x, velocity.x);
			forceOverLifetimeModule.y = CPWParticleWind.AddToMinMaxCurve(forceOverLifetimeModule.y, velocity.y);
			forceOverLifetimeModule.z = CPWParticleWind.AddToMinMaxCurve(forceOverLifetimeModule.z, velocity.z);
		}
	}
}
