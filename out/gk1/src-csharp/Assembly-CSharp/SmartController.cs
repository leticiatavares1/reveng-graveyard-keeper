using System.Collections.Generic;
using UnityEngine;

public class SmartController : MonoBehaviour
{
	public float min;

	public float max = 1f;

	public float value = 1f;

	public List<SmartControllerParameter> parameters = new List<SmartControllerParameter>();

	public void Update()
	{
		for (int i = 0; i < parameters.Count; i++)
		{
			parameters[i].Evaluate(value);
		}
	}

	public SmartControllerParameter FindParameterOfType(SmartControllerParameter.Action action)
	{
		foreach (SmartControllerParameter parameter in parameters)
		{
			if (parameter.action == action)
			{
				return parameter;
			}
		}
		Debug.LogError("Couldn't find parameter with action = " + action);
		return null;
	}
}
