using UnityEngine;

public class SmartCondition_Random : SmartCondition
{
	public float chance = 50f;

	public override bool CheckCondition()
	{
		return (float)Random.Range(1, 100) < chance;
	}

	public override string GetName()
	{
		return "Random (" + chance + ")";
	}
}
