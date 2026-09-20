using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestSystemData
{
	[Serializable]
	public class DelayedQuest
	{
		public string id;

		public float delayTime;

		public QuestStatus status;

		public DelayedQuest()
		{
		}

		public DelayedQuest(string id, float delayTime, QuestStatus status)
		{
			this.id = id;
			this.delayTime = delayTime;
			this.status = status;
		}
	}

	public QuestCollectionData questCollection = new QuestCollectionData();

	public Dictionary<string, DelayedQuest> delayedQuests = new Dictionary<string, DelayedQuest>();

	public event Action<QuestData> OnQuestStarted;

	public event Action<QuestData> OnQuestCompleted;

	public event Action<QuestData> OnQuestCanceled;

	public QuestSystemData()
	{
	}

	public QuestSystemData(List<QuestDef> questDefs)
	{
		foreach (QuestDef questDef in questDefs)
		{
			questCollection.quests.Add(new QuestData(questDef));
		}
	}

	public void PrepareForGame()
	{
		questCollection.PrepareForGame();
		RebuildGlobalEventsFromQuestStatuses();
	}

	public void AddQuestData(string questId)
	{
		QuestDef data = GameBalance.Me.GetData<QuestDef>(questId);
		if (data != null)
		{
			questCollection.AddQuestData(new QuestData(data));
		}
	}

	private void RebuildGlobalEventsFromQuestStatuses()
	{
		GlobalEventsSystem globalEventsSystem = MainGame.Instance.GameSave.globalEventsSystem;
		for (int i = 0; i < questCollection.quests.Count; i++)
		{
			QuestData questData = questCollection.quests[i];
			if (questData == null || questData.Definition == null)
			{
				continue;
			}
			switch (questData.status)
			{
			case QuestStatus.Awaiting:
				globalEventsSystem.AddEvent(questData.Definition.startCheck);
				break;
			case QuestStatus.InProgress:
				if (questData.Definition.finishCheck.hasTrigger)
				{
					globalEventsSystem.AddEvent(questData.Definition.finishCheck);
				}
				break;
			}
		}
	}

	public void StartQuest(string id, float delayTime = 0f)
	{
		if (!questCollection.questsCache.TryGetValue(id, out var value))
		{
			Debug.LogError("Quest wasn't found by id: " + id);
			return;
		}
		QuestStatus status = value.status;
		if (status == QuestStatus.InProgress || status == QuestStatus.Completed || status == QuestStatus.Canceled)
		{
			Debug.LogError("Quest with id " + id + " can't be started because quest status is already: " + value.status);
		}
		else
		{
			if (IsQuestDelayed(value, delayTime, QuestStatus.InProgress))
			{
				return;
			}
			for (int i = 0; i < questCollection.quests.Count; i++)
			{
				QuestData questData = questCollection.quests[i];
				if (questData == null)
				{
					Debug.LogError($"Quest is null at index {i}");
				}
				else if (questData.Definition == null)
				{
					Debug.LogError("Quest definition is null for quest: " + questData.id);
				}
				else if (questData != value && questData.Definition.TreePos == value.Definition.TreePos && questData.status == QuestStatus.Completed)
				{
					questData.isHidden = true;
				}
			}
			value.Start();
			this.OnQuestStarted?.Invoke(value);
		}
	}

	public void OnStartFailed(string id)
	{
		if (!questCollection.questsCache.TryGetValue(id, out var value))
		{
			Debug.LogError("Quest wasn't found by id: " + id);
		}
		else
		{
			value.FailedStart();
		}
	}

	public void AwaitQuest(string id, float delayTime = 0f)
	{
		if (!questCollection.questsCache.TryGetValue(id, out var value))
		{
			Debug.LogError("Quest wasn't found by id: " + id);
			return;
		}
		QuestStatus status = value.status;
		if (status != QuestStatus.Awaiting && status != QuestStatus.Completed && status != QuestStatus.Canceled && !IsQuestDelayed(value, delayTime, QuestStatus.Awaiting))
		{
			value.Await();
		}
	}

	public void CompleteQuest(string id, float delayTime = 0f)
	{
		if (!questCollection.questsCache.TryGetValue(id, out var value))
		{
			Debug.LogError("Quest wasn't found by id: " + id);
			return;
		}
		QuestStatus status = value.status;
		if (status != QuestStatus.Completed && status != QuestStatus.Canceled && !IsQuestDelayed(value, delayTime, QuestStatus.Completed))
		{
			value.Complete();
			this.OnQuestCompleted?.Invoke(value);
		}
	}

	public void CancelQuest(string id)
	{
		if (!questCollection.questsCache.TryGetValue(id, out var value))
		{
			Debug.LogError("Quest wasn't found by id: " + id);
			return;
		}
		delayedQuests.Remove(id);
		QuestStatus status = value.status;
		if (status != QuestStatus.Completed && status != QuestStatus.Canceled)
		{
			value.Cancel();
			this.OnQuestCanceled?.Invoke(value);
		}
	}

	public bool IsQuestInStatus(string id, QuestStatus status)
	{
		if (questCollection.questsCache.TryGetValue(id, out var value))
		{
			return value.status == status;
		}
		return false;
	}

	public void ChangeQuestHiddenState(string id, bool state)
	{
		if (!questCollection.questsCache.TryGetValue(id, out var value))
		{
			Debug.LogError("Quest wasn't found by id: " + id);
		}
		else
		{
			value.isHidden = state;
		}
	}

	public void ChangeQuestUnknownState(string id, bool state)
	{
		if (!questCollection.questsCache.TryGetValue(id, out var value))
		{
			Debug.LogError("Quest wasn't found by id: " + id);
		}
		else
		{
			value.isUnknown = state;
		}
	}

	public bool RaiseCustomQuestTrigger(string id, Action callback)
	{
		if (!questCollection.customTriggersCache.TryGetValue(id, out var value))
		{
			Debug.LogError("CustomTrigger wasn't found by id: " + id);
			return false;
		}
		return value.EvaluateWithCallback(callback);
	}

	public bool WgoHasReadyToFinishQuest(string wgoId)
	{
		if (!FinishPhrasesByWgoParser.TryGetFinishPhrases(wgoId, out var phrasesByWgo))
		{
			return false;
		}
		foreach (string phrase in phrasesByWgo.phrases)
		{
			if (GameBalance.Me.questDefByFinishPhrase.TryGetValue(phrase, out var value) && MainGame.Instance.GameSave.questSystemData.questCollection.questsCache.TryGetValue(value.id, out var value2) && value2.status == QuestStatus.InProgress && value.finishCheck.IsReadyToFinish())
			{
				return true;
			}
		}
		return false;
	}

	private bool IsQuestDelayed(QuestData questData, float delayTime, QuestStatus targetStatus)
	{
		if (delayTime.EqualsTo(0f, 0.01f))
		{
			delayedQuests.Remove(questData.id);
			return false;
		}
		if (delayedQuests.TryGetValue(questData.id, out var value))
		{
			value.delayTime = delayTime;
			return true;
		}
		delayedQuests.Add(questData.id, new DelayedQuest(questData.id, delayTime, targetStatus));
		return true;
	}
}
