using System;
using UnityEngine;

[Serializable]
public class CPWParticleWindVelocity : CPWParticleWind
{
	public float xWindMultiplication;

	public float yWindMultiplication;

	public float zWindMultiplication;

	protected override void DoWindAffection_Internal(float windValue)
	{
		AddToVelocity(particleSystem.velocityOverLifetime, new Vector3(xWindMultiplication, yWindMultiplication, zWindMultiplication) * windValue);
	}

	private static void AddToVelocity(ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule, Vector3 velocity)
	{
		if (velocityOverLifetimeModule.enabled)
		{
			velocityOverLifetimeModule.x = CPWParticleWind.AddToMinMaxCurve(velocityOverLifetimeModule.x, velocity.x);
			velocityOverLifetimeModule.y = CPWParticleWind.AddToMinMaxCurve(velocityOverLifetimeModule.y, velocity.y);
			velocityOverLifetimeModule.z = CPWParticleWind.AddToMinMaxCurve(velocityOverLifetimeModule.z, velocity.z);
		}
	}
}
