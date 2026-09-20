using System;
using UnityEngine;

[Serializable]
public class CPWParticleWindRateOverTime : CPWParticleWind
{
	public float rateOverTimeAffectedByWind;

	protected override void DoWindAffection_Internal(float windValue)
	{
		ParticleSystem.EmissionModule emission = particleSystem.emission;
		emission.rateOverTime = CPWParticleWind.AddToMinMaxCurve(emission.rateOverTime, Mathf.Abs(rateOverTimeAffectedByWind * windValue));
	}
}
