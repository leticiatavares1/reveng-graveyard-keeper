using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestCollectionData
{
	public List<QuestData> quests = new List<QuestData>();

	[NonSerialized]
	public Dictionary<QuestStatus, List<QuestData>> questStatusFilteredQuests;

	[NonSerialized]
	public Dictionary<string, QuestData> questsCache;

	[NonSerialized]
	public Dictionary<string, LazyExpression> customTriggersCache;

	public void PrepareForGame()
	{
		questsCache = new Dictionary<string, QuestData>();
		questStatusFilteredQuests = new Dictionary<QuestStatus, List<QuestData>>();
		customTriggersCache = new Dictionary<string, LazyExpression>();
		for (int i = 0; i < quests.Count; i++)
		{
			QuestData questData = quests[i];
			questsCache.Add(questData.id, questData);
			if (questData.Definition == null)
			{
				Debug.LogError("Quest definition not found for quest: " + questData.id);
			}
			else
			{
				AddCustomTriggersToCache(questData);
			}
		}
		foreach (QuestStatus value in Enum.GetValues(typeof(QuestStatus)))
		{
			questStatusFilteredQuests.Add(value, new List<QuestData>());
			for (int j = 0; j < quests.Count; j++)
			{
				if (quests[j].status == value)
				{
					questStatusFilteredQuests[value].Add(quests[j]);
				}
			}
		}
		AddNewQuestsFromBalance();
	}

	public void AddQuestData(QuestData questData)
	{
		if (questsCache.ContainsKey(questData.id))
		{
			Debug.LogError("Quest collection already have same quest: " + questData.id);
			return;
		}
		if (questData.Definition == null)
		{
			Debug.LogError("Quest definition not found for quest: " + questData.id);
			return;
		}
		quests.Add(questData);
		questsCache.Add(questData.id, questData);
		questStatusFilteredQuests[questData.status].Add(questData);
		AddCustomTriggersToCache(questData);
	}

	private void AddNewQuestsFromBalance()
	{
		List<QuestDef> questDefs = GameBalance.Me.questDefs;
		for (int i = 0; i < questDefs.Count; i++)
		{
			QuestDef questDef = questDefs[i];
			if (questDef != null && !questsCache.ContainsKey(questDef.id))
			{
				AddQuestData(new QuestData(questDef));
			}
		}
	}

	private void AddCustomTriggersToCache(QuestData questData)
	{
		for (int i = 0; i < questData.Definition.customTriggers.Count; i++)
		{
			customTriggersCache.Add(questData.Definition.customTriggers[i].name, questData.Definition.customTriggers[i].expression);
		}
	}
}
