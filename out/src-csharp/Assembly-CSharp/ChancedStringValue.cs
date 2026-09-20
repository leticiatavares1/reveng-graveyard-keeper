using System;
using UnityEngine;

[Serializable]
public class ChancedStringValue
{
	[SerializeField]
	private string _id = "";

	[SerializeField]
	private string _id2 = "";

	[SerializeField]
	private SmartExpression _expression;

	[SerializeField]
	private bool _chanced;

	public string GetValue(WorldGameObject wgo = null, WorldGameObject character = null)
	{
		if (_expression == null || !_chanced)
		{
			return _id;
		}
		if (!(_expression.EvaluateFloat(wgo, character) * 100f >= (float)UnityEngine.Random.Range(0, 100)))
		{
			return _id2;
		}
		return _id;
	}
}
