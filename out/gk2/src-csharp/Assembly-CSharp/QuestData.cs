using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class QuestData : ObjectLinkedToDefinition<QuestDef>
{
	public bool isHidden;

	public bool isUnknown;

	public QuestStatus status;

	private GlobalEventsSystem GlobalEventsSystem => MainGame.Instance.GameSave.globalEventsSystem;

	public string Description
	{
		get
		{
			if (status != QuestStatus.Completed)
			{
				return LLBase.L("quest_open_" + id + "_d");
			}
			return LLBase.L("quest_closed_" + id + "_d");
		}
	}

	public QuestViewStatus ViewStatus
	{
		get
		{
			if (isHidden)
			{
				return QuestViewStatus.Hidden;
			}
			if (isUnknown)
			{
				return QuestViewStatus.Unknown;
			}
			switch (status)
			{
			case QuestStatus.Available:
			case QuestStatus.Awaiting:
				return QuestViewStatus.Visible;
			case QuestStatus.InProgress:
				return QuestViewStatus.Revealed;
			case QuestStatus.Completed:
				return QuestViewStatus.Completed;
			default:
				return QuestViewStatus.Hidden;
			}
		}
	}

	public bool IsActiveQuest
	{
		get
		{
			QuestViewStatus viewStatus = ViewStatus;
			return viewStatus == QuestViewStatus.Visible || viewStatus == QuestViewStatus.Revealed;
		}
	}

	public QuestData()
	{
	}

	public QuestData(QuestDef questDef)
	{
		id = questDef.id;
		status = QuestStatus.Available;
		isHidden = questDef.isHidden;
		isUnknown = questDef.isUnknown;
	}

	public void Start()
	{
		QuestStatus questStatus = status;
		if (questStatus == QuestStatus.Canceled || questStatus == QuestStatus.Completed)
		{
			return;
		}
		if (base.Definition.hasPosInBalance)
		{
			isHidden = false;
		}
		if (base.Definition.startCheck.hasTrigger)
		{
			GlobalEventsSystem.RemoveEvent(base.Definition.startCheck);
		}
		if (base.Definition.finishCheck.hasTrigger)
		{
			GlobalEventsSystem.AddEvent(base.Definition.finishCheck);
		}
		status = QuestStatus.InProgress;
		foreach (LazyExpression item in base.Definition.execExpressionsStart)
		{
			item.Evaluate();
		}
		Debug.Log("Quest:[" + id + "] status = InProgress");
	}

	public void FailedStart()
	{
		foreach (LazyExpression item in base.Definition.execExpressionsStartFail)
		{
			item.Evaluate();
		}
	}

	public void Await()
	{
		GlobalEventsSystem.AddEvent(base.Definition.startCheck);
		status = QuestStatus.Awaiting;
		Debug.Log("Quest:[" + id + "] status = Awaiting");
	}

	public void Complete()
	{
		if (base.Definition.startCheck.hasTrigger)
		{
			GlobalEventsSystem.RemoveEvent(base.Definition.startCheck);
		}
		if (base.Definition.finishCheck.hasTrigger)
		{
			GlobalEventsSystem.RemoveEvent(base.Definition.finishCheck);
		}
		status = QuestStatus.Completed;
		foreach (LazyExpression item in base.Definition.execExpressionsFinish)
		{
			item.Evaluate();
		}
		Debug.Log("Quest:[" + id + "] status = Completed");
	}

	public void Cancel()
	{
		QuestStatus questStatus = status;
		if (questStatus != QuestStatus.Completed && questStatus != QuestStatus.Canceled)
		{
			if (base.Definition.startCheck.hasTrigger)
			{
				GlobalEventsSystem.RemoveEvent(base.Definition.startCheck);
			}
			if (base.Definition.finishCheck.hasTrigger)
			{
				GlobalEventsSystem.RemoveEvent(base.Definition.finishCheck);
			}
			if (!string.IsNullOrEmpty(base.Definition.finishCheck.phrase))
			{
				MainGame.Instance.GameSave.knowledgeSystem.AddPhraseToBlackList(base.Definition.finishCheck.phrase);
			}
			status = QuestStatus.Canceled;
			Debug.Log("Quest:[" + id + "] status = Canceled");
		}
	}
}
