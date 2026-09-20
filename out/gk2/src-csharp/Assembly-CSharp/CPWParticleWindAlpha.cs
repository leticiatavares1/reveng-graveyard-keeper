using System;
using UnityEngine;

[Serializable]
public class CPWParticleWindAlpha : CPWParticleWind
{
	[Range(-1f, 1f)]
	public float alphaAffectedByWind;

	protected override void DoWindAffection_Internal(float windValue)
	{
		ParticleSystem.MainModule main = particleSystem.main;
		if (Mathf.Abs(alphaAffectedByWind) > 0.001f)
		{
			float num = ((alphaAffectedByWind > 0f) ? alphaAffectedByWind : (1f - alphaAffectedByWind));
			main.startColor = MultiplyToMinMaxGradient(main.startColor, num * Mathf.Abs(windValue));
		}
	}
}
