using UnityEngine;

public static class ProjectTools
{
	public static int GenerateIntId()
	{
		return Random.Range(int.MinValue, int.MaxValue);
	}
}
