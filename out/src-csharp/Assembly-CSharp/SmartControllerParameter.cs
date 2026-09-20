using System;
using UnityEngine;

[Serializable]
public class SmartControllerParameter
{
	public enum Action
	{
		None = 0,
		GameObjectActivate = 1,
		GameObjectDeactivate = 2,
		Action = 3,
		ComponentActivate = 4,
		ComponentDeactivate = 5,
		AbstractControllerCmp = 6,
		MaterialFloatParam = 90,
		ParticleSystem = 100
	}

	public enum SubActionParticle
	{
		None,
		MaxParticles,
		VelocityX,
		VelocityY,
		VelocityZ,
		LifetimeRange,
		EmissionRate
	}

	public enum Condition
	{
		False,
		True,
		More,
		Less,
		InRange
	}

	public Action action;

	public Condition condition;

	public SubActionParticle action_particle;

	public GameObject go;

	public MonoBehaviour cmp;

	public AbstractControllerComponent abs_cmp;

	public ParticleSystem particle_system;

	public float v1;

	public float v2;

	public float p1;

	public float p2;

	public float x1;

	public float x2;

	public float range_min;

	public float range_max;

	public AnimationCurve p_curve = new AnimationCurve();

	public Gradient gradient = new Gradient();

	public Material mat;

	public string s;

	public Action<float> action_delegate;

	public void Evaluate(float value)
	{
		switch (action)
		{
		case Action.None:
			break;
		case Action.GameObjectActivate:
			if (!(go == null))
			{
				go.SetActive(EvaluateBoolean(value));
			}
			break;
		case Action.GameObjectDeactivate:
			if (!(go == null))
			{
				go.SetActive(!EvaluateBoolean(value));
			}
			break;
		case Action.ComponentActivate:
			if (!(go == null))
			{
				cmp.enabled = EvaluateBoolean(value);
			}
			break;
		case Action.ComponentDeactivate:
			if (!(cmp == null))
			{
				cmp.enabled = !EvaluateBoolean(value);
			}
			break;
		case Action.MaterialFloatParam:
			if (!(mat == null))
			{
				mat.SetFloat(s, p_curve.Evaluate(value));
			}
			break;
		case Action.AbstractControllerCmp:
			if (!(abs_cmp == null))
			{
				abs_cmp.Set(p_curve.Evaluate(value));
			}
			break;
		case Action.Action:
			action_delegate(p_curve.Evaluate(value));
			break;
		case Action.ParticleSystem:
			if (!(particle_system == null))
			{
				ParticleSystem.MainModule main = particle_system.main;
				ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particle_system.velocityOverLifetime;
				ParticleSystem.EmissionModule emission = particle_system.emission;
				switch (action_particle)
				{
				case SubActionParticle.None:
					break;
				case SubActionParticle.MaxParticles:
					main.maxParticles = (int)p_curve.Evaluate(value);
					break;
				case SubActionParticle.VelocityX:
				{
					ParticleSystem.MinMaxCurve curve = velocityOverLifetime.x;
					EvaluateAnimationCurve(value, ref curve);
					velocityOverLifetime.x = curve;
					break;
				}
				case SubActionParticle.VelocityY:
				{
					ParticleSystem.MinMaxCurve curve = velocityOverLifetime.y;
					EvaluateAnimationCurve(value, ref curve);
					velocityOverLifetime.y = curve;
					break;
				}
				case SubActionParticle.VelocityZ:
				{
					ParticleSystem.MinMaxCurve curve = velocityOverLifetime.z;
					EvaluateAnimationCurve(value, ref curve);
					velocityOverLifetime.z = curve;
					break;
				}
				case SubActionParticle.LifetimeRange:
				{
					ParticleSystem.MinMaxCurve curve = main.startLifetime;
					EvaluateAnimationCurve(value, ref curve);
					main.startLifetime = curve;
					break;
				}
				case SubActionParticle.EmissionRate:
				{
					ParticleSystem.MinMaxCurve curve = emission.rateOverTime;
					EvaluateAnimationCurve(value, ref curve);
					emission.rateOverTime = curve;
					break;
				}
				default:
					Debug.LogError("SubActionParticle is not supported: " + action_particle);
					break;
				}
			}
			break;
		default:
			Debug.LogError("Action is not supported: " + action);
			break;
		}
	}

	private void EvaluateCurve(float value, ref ParticleSystem.MinMaxCurve curve, bool inverse = false)
	{
		if (inverse)
		{
			curve.constantMin = EvaluateFloatRange(value);
			curve.constantMax = EvaluateFloatRangeX(value);
		}
		else
		{
			curve.constantMin = EvaluateFloatRangeX(value);
			curve.constantMax = EvaluateFloatRange(value);
		}
	}

	private void EvaluateAnimationCurve(float value, ref ParticleSystem.MinMaxCurve curve, bool inverse = false)
	{
		float num2 = (curve.constant = p_curve.Evaluate(value));
		float range = GetRange(value);
		if (inverse)
		{
			curve.constantMin = num2 - range;
			curve.constantMax = num2 + range;
		}
		else
		{
			curve.constantMin = num2 + range;
			curve.constantMax = num2 - range;
		}
	}

	public bool IsBooleanEvaluation()
	{
		Action action = this.action;
		if ((uint)(action - 1) <= 1u || (uint)(action - 4) <= 1u)
		{
			return true;
		}
		return false;
	}

	private bool EvaluateBoolean(float value)
	{
		switch (condition)
		{
		case Condition.False:
			return false;
		case Condition.True:
			return true;
		case Condition.More:
			return value > v1;
		case Condition.Less:
			return value < v1;
		case Condition.InRange:
			if (value > v1)
			{
				return value < v2;
			}
			return false;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private float GetRange(float value)
	{
		return Mathf.Lerp(range_min, range_max, NormalizeValue(value));
	}

	private float NormalizeValue(float value)
	{
		return (value - v1) / (v2 - v1);
	}

	private float EvaluateFloatRange(float value)
	{
		return Mathf.Lerp(p1, p2, NormalizeValue(value));
	}

	private float EvaluateFloatRangeX(float value)
	{
		return Mathf.Lerp(x1, x2, NormalizeValue(value));
	}
}
