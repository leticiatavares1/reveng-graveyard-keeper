using System;

[Serializable]
public class QuestState
{
	[Serializable]
	public enum State
	{
		Failed = -1,
		InProgress,
		Succeeded
	}

	public QuestDefinition definition = new QuestDefinition();

	public long start_time;

	public State state;

	public State CheckQuestProgress()
	{
		if (definition.IsFailed())
		{
			return State.Failed;
		}
		if (definition.IsSucceed())
		{
			return State.Succeeded;
		}
		return State.InProgress;
	}

	public void FromGJCode(string gjcode)
	{
	}

	public string ToGJCode()
	{
		return null;
	}

	public void OnDeserialize()
	{
	}
}
