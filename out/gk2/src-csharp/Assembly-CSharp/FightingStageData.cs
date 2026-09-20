using System;

[Serializable]
public class FightingStageData
{
	public int id;

	public bool enabled;

	public FightingStageData(int id)
	{
		this.id = id;
	}
}
