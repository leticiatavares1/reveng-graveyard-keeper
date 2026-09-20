using System;

[Serializable]
public class InspirationProgressData
{
	public string id;

	public int currentValue;

	public int completionGoalValue;

	public InspirationProgressData()
	{
	}

	public InspirationProgressData(string id, int currentValue, int completionGoalValue = 0)
	{
		this.id = id;
		this.currentValue = currentValue;
		this.completionGoalValue = completionGoalValue;
	}
}
