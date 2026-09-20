using System.Collections.Generic;

public class QuestSystem : ICustomUpdatable
{
	private static QuestSystemData QuestSystemData => MainGame.Instance.GameSave.questSystemData;

	public void CustomUpdate(float deltaTime)
	{
		float num = EnvironmentEngine.Instance.ConvertDeltaTimeToGameplayTime01(deltaTime);
		List<QuestSystemData.DelayedQuest> list = new List<QuestSystemData.DelayedQuest>();
		foreach (QuestSystemData.DelayedQuest value in QuestSystemData.delayedQuests.Values)
		{
			value.delayTime -= num;
			if (value.delayTime <= 0f)
			{
				list.Add(value);
			}
		}
		foreach (QuestSystemData.DelayedQuest item in list)
		{
			QuestSystemData.delayedQuests.Remove(item.id);
			switch (item.status)
			{
			case QuestStatus.Awaiting:
				QuestSystemData.AwaitQuest(item.id);
				break;
			case QuestStatus.InProgress:
				QuestSystemData.StartQuest(item.id);
				break;
			case QuestStatus.Completed:
				QuestSystemData.CompleteQuest(item.id);
				break;
			}
		}
	}
}
