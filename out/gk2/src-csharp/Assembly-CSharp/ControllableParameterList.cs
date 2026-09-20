using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ControllableParameterList
{
	[SerializeReference]
	public List<ControllableParameter> parameters = new List<ControllableParameter>();

	public void Init()
	{
		foreach (ControllableParameter parameter in parameters)
		{
			parameter.Init();
		}
	}

	public void UpdateParameters(float v, WeatherComponent weatherComponent)
	{
		for (int i = 0; i < parameters.Count; i++)
		{
			parameters[i].UpdateParameter(v, weatherComponent);
		}
	}

	public void OnDisable()
	{
		foreach (ControllableParameter parameter in parameters)
		{
			parameter.OnDisable();
		}
	}
}
