using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class GameResCondition
{
	[Serializable]
	public enum Condition
	{
		Null,
		Equals,
		More,
		Less,
		MoreEq,
		LessEq
	}

	[SerializeField]
	private List<float> resValues = new List<float>();

	[SerializeField]
	private List<string> resType = new List<string>();

	[SerializeField]
	private List<Condition> resCond = new List<Condition>();

	public List<string> TypesList => resType;

	public bool CheckCondition(GameRes gameRes)
	{
		return CheckCondition((string id) => gameRes.Get(id));
	}

	public bool CheckCondition(GameResInt gameResInt)
	{
		return CheckCondition((string id) => gameResInt.Get(id));
	}

	private bool CheckCondition(Func<string, float> getFunction)
	{
		for (int i = 0; i < resValues.Count; i++)
		{
			float num = getFunction(resType[i]);
			float num2 = resValues[i];
			if (resCond[i] switch
			{
				Condition.Equals => Mathf.Approximately(num, num2) ? 1 : 0, 
				Condition.More => (num > num2) ? 1 : 0, 
				Condition.MoreEq => (num >= num2) ? 1 : 0, 
				Condition.Less => (num < num2) ? 1 : 0, 
				Condition.LessEq => (num <= num2) ? 1 : 0, 
				_ => throw new Exception($"Condition {resCond[i]} not implemented."), 
			} == 0)
			{
				return false;
			}
		}
		return true;
	}

	public static string ConditionToString(Condition condition)
	{
		return condition switch
		{
			Condition.Equals => "==", 
			Condition.More => ">", 
			Condition.MoreEq => ">=", 
			Condition.Less => "<", 
			Condition.LessEq => "<=", 
			_ => "?", 
		};
	}
}
