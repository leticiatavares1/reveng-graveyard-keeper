using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableAnimatorParameters
{
	[Serializable]
	public class SerializableAnimatorParameter
	{
		public int int_v;

		public bool bool_v;

		public float float_v;

		public bool trigger_v;

		public int param_type;

		public string param_name;

		public void SetValue<T>(AnimatorControllerParameterType at, T value) where T : struct
		{
			switch (at)
			{
			case AnimatorControllerParameterType.Float:
				float_v = Convert.ToSingle(value);
				break;
			case AnimatorControllerParameterType.Int:
				int_v = Convert.ToInt32(value);
				break;
			case AnimatorControllerParameterType.Bool:
				bool_v = Convert.ToBoolean(value);
				break;
			case AnimatorControllerParameterType.Trigger:
				trigger_v = Convert.ToBoolean(value);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			param_type = (int)at;
		}
	}

	[Serializable]
	public struct Trigger
	{
		public bool v;
	}

	public List<SerializableAnimatorParameter> pars = new List<SerializableAnimatorParameter>();

	public SerializableAnimatorParameters()
	{
		Set("global_state", 0);
		Set("direction_angle", -90f);
	}

	public void Set<T>(string name, T value) where T : struct
	{
		AnimatorControllerParameterType animType = GetAnimType(typeof(T));
		foreach (SerializableAnimatorParameter par in pars)
		{
			if (par.param_name == name && par.param_type == (int)animType)
			{
				par.SetValue(animType, value);
				return;
			}
		}
		SerializableAnimatorParameter serializableAnimatorParameter = new SerializableAnimatorParameter
		{
			param_name = name
		};
		serializableAnimatorParameter.SetValue(animType, value);
		pars.Add(serializableAnimatorParameter);
	}

	public bool HasParameter(string name)
	{
		foreach (SerializableAnimatorParameter par in pars)
		{
			if (par.param_name == name)
			{
				return true;
			}
		}
		return false;
	}

	public float GetParameterFloat(string name)
	{
		foreach (SerializableAnimatorParameter par in pars)
		{
			if (par.param_name == name)
			{
				return par.float_v;
			}
		}
		return 0f;
	}

	private AnimatorControllerParameterType GetAnimType(Type t)
	{
		if (t == typeof(int))
		{
			return AnimatorControllerParameterType.Int;
		}
		if (t == typeof(bool))
		{
			return AnimatorControllerParameterType.Bool;
		}
		if (t == typeof(float))
		{
			return AnimatorControllerParameterType.Float;
		}
		if (t == typeof(Trigger))
		{
			return AnimatorControllerParameterType.Trigger;
		}
		throw new Exception("Unknown type: " + t);
	}
}
