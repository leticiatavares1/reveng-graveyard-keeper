using System;

[Serializable]
public class JobDefinition : BalanceBaseObject
{
	public enum JobType
	{
		DigGrave = 1
	}

	public int time_len;

	public int real_time;

	public JobType type;
}
