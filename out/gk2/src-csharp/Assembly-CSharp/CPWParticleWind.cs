using System;
using UnityEngine;

[Serializable]
public abstract class CPWParticleWind : CPWParticle
{
	public bool DoWindAffection(float windValue)
	{
		if (particleSystem == null)
		{
			Debug.LogError("CPWParticleWind null particle system");
			return false;
		}
		DoWindAffection_Internal(windValue);
		return true;
	}

	protected virtual void DoWindAffection_Internal(float windValue)
	{
	}

	protected static ParticleSystem.MinMaxCurve AddToMinMaxCurve(ParticleSystem.MinMaxCurve minMaxCurve, float additiveParameter)
	{
		switch (minMaxCurve.mode)
		{
		case ParticleSystemCurveMode.Constant:
			minMaxCurve = new ParticleSystem.MinMaxCurve(minMaxCurve.constant + additiveParameter);
			break;
		case ParticleSystemCurveMode.TwoConstants:
			minMaxCurve = new ParticleSystem.MinMaxCurve(minMaxCurve.constantMin + additiveParameter, minMaxCurve.constantMax + additiveParameter);
			break;
		}
		return minMaxCurve;
	}

	protected static ParticleSystem.MinMaxGradient AddToMinMaxGradient(ParticleSystem.MinMaxGradient minMaxGradient, float additiveParameter)
	{
		switch (minMaxGradient.mode)
		{
		case ParticleSystemGradientMode.Color:
		{
			Color color = minMaxGradient.color;
			minMaxGradient = new ParticleSystem.MinMaxGradient(new Color(color.r, color.g, color.b, Mathf.Clamp01(color.a + additiveParameter)));
			break;
		}
		case ParticleSystemGradientMode.Gradient:
		{
			GradientAlphaKey[] array3 = new GradientAlphaKey[minMaxGradient.gradient.alphaKeys.Length];
			for (int k = 0; k < minMaxGradient.gradient.alphaKeys.Length; k++)
			{
				GradientAlphaKey gradientAlphaKey3 = minMaxGradient.gradient.alphaKeys[k];
				array3[k] = new GradientAlphaKey(Mathf.Clamp01(gradientAlphaKey3.alpha + additiveParameter), gradientAlphaKey3.time);
			}
			minMaxGradient = new ParticleSystem.MinMaxGradient(new Gradient
			{
				alphaKeys = array3,
				mode = minMaxGradient.gradient.mode,
				colorKeys = minMaxGradient.gradient.colorKeys
			});
			break;
		}
		case ParticleSystemGradientMode.TwoColors:
		{
			Color colorMin = minMaxGradient.colorMin;
			Color colorMax = minMaxGradient.colorMax;
			minMaxGradient = new ParticleSystem.MinMaxGradient(new Color(colorMin.r, colorMin.g, colorMin.b, Mathf.Clamp01(colorMin.a + additiveParameter)), new Color(colorMax.r, colorMax.g, colorMax.b, Mathf.Clamp01(colorMax.a + additiveParameter)));
			break;
		}
		case ParticleSystemGradientMode.TwoGradients:
		{
			GradientAlphaKey[] array = new GradientAlphaKey[minMaxGradient.gradient.alphaKeys.Length];
			for (int i = 0; i < minMaxGradient.gradient.alphaKeys.Length; i++)
			{
				GradientAlphaKey gradientAlphaKey = minMaxGradient.gradient.alphaKeys[i];
				array[i] = new GradientAlphaKey(Mathf.Clamp01(gradientAlphaKey.alpha + additiveParameter), gradientAlphaKey.time);
			}
			GradientAlphaKey[] array2 = new GradientAlphaKey[minMaxGradient.gradientMin.alphaKeys.Length];
			for (int j = 0; j < minMaxGradient.gradientMin.alphaKeys.Length; j++)
			{
				GradientAlphaKey gradientAlphaKey2 = minMaxGradient.gradientMin.alphaKeys[j];
				array2[j] = new GradientAlphaKey(Mathf.Clamp01(gradientAlphaKey2.alpha + additiveParameter), gradientAlphaKey2.time);
			}
			minMaxGradient = new ParticleSystem.MinMaxGradient(new Gradient
			{
				alphaKeys = array,
				mode = minMaxGradient.gradient.mode,
				colorKeys = minMaxGradient.gradient.colorKeys
			}, new Gradient
			{
				alphaKeys = array2,
				mode = minMaxGradient.gradientMin.mode,
				colorKeys = minMaxGradient.gradientMin.colorKeys
			});
			break;
		}
		}
		return minMaxGradient;
	}

	protected ParticleSystem.MinMaxGradient MultiplyToMinMaxGradient(ParticleSystem.MinMaxGradient minMaxGradient, float multiplier)
	{
		switch (minMaxGradient.mode)
		{
		case ParticleSystemGradientMode.Color:
		{
			Color color = minMaxGradient.color;
			minMaxGradient = new ParticleSystem.MinMaxGradient(new Color(color.r, color.g, color.b, Mathf.Clamp01(color.a * multiplier)));
			break;
		}
		case ParticleSystemGradientMode.Gradient:
		{
			GradientAlphaKey[] array3 = new GradientAlphaKey[minMaxGradient.gradient.alphaKeys.Length];
			for (int k = 0; k < minMaxGradient.gradient.alphaKeys.Length; k++)
			{
				GradientAlphaKey gradientAlphaKey3 = minMaxGradient.gradient.alphaKeys[k];
				array3[k] = new GradientAlphaKey(Mathf.Clamp01(gradientAlphaKey3.alpha * multiplier), gradientAlphaKey3.time);
			}
			minMaxGradient = new ParticleSystem.MinMaxGradient(new Gradient
			{
				alphaKeys = array3,
				mode = minMaxGradient.gradient.mode,
				colorKeys = minMaxGradient.gradient.colorKeys
			});
			break;
		}
		case ParticleSystemGradientMode.TwoColors:
		{
			Color colorMin = minMaxGradient.colorMin;
			Color colorMax = minMaxGradient.colorMax;
			minMaxGradient = new ParticleSystem.MinMaxGradient(new Color(colorMin.r, colorMin.g, colorMin.b, Mathf.Clamp01(colorMin.a * multiplier)), new Color(colorMax.r, colorMax.g, colorMax.b, Mathf.Clamp01(colorMax.a * multiplier)));
			break;
		}
		case ParticleSystemGradientMode.TwoGradients:
		{
			GradientAlphaKey[] array = new GradientAlphaKey[minMaxGradient.gradient.alphaKeys.Length];
			for (int i = 0; i < minMaxGradient.gradient.alphaKeys.Length; i++)
			{
				GradientAlphaKey gradientAlphaKey = minMaxGradient.gradient.alphaKeys[i];
				array[i] = new GradientAlphaKey(Mathf.Clamp01(gradientAlphaKey.alpha * multiplier), gradientAlphaKey.time);
			}
			GradientAlphaKey[] array2 = new GradientAlphaKey[minMaxGradient.gradientMin.alphaKeys.Length];
			for (int j = 0; j < minMaxGradient.gradientMin.alphaKeys.Length; j++)
			{
				GradientAlphaKey gradientAlphaKey2 = minMaxGradient.gradientMin.alphaKeys[j];
				array2[j] = new GradientAlphaKey(Mathf.Clamp01(gradientAlphaKey2.alpha * multiplier), gradientAlphaKey2.time);
			}
			minMaxGradient = new ParticleSystem.MinMaxGradient(new Gradient
			{
				alphaKeys = array,
				mode = minMaxGradient.gradient.mode,
				colorKeys = minMaxGradient.gradient.colorKeys
			}, new Gradient
			{
				alphaKeys = array2,
				mode = minMaxGradient.gradientMin.mode,
				colorKeys = minMaxGradient.gradientMin.colorKeys
			});
			break;
		}
		}
		return minMaxGradient;
	}
}
