using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class QuestFinishCheck : QuestCheck
{
	public string phrase;

	public List<QuestPhraseRequirement> phraseReqs = new List<QuestPhraseRequirement>();

	public AnswerData GetAnswerDataByReqs()
	{
		AnswerData answerData = new AnswerData();
		foreach (QuestPhraseRequirement phraseReq in phraseReqs)
		{
			switch (phraseReq.entity)
			{
			case QuestPhraseRequirement.Entity.Item:
				if (phraseReq.requirement == QuestPhraseRequirement.Requirement.Lock)
				{
					answerData.AddLockRes(new SmartRes
					{
						items = new List<ItemCount> { phraseReq.itemCount }
					});
				}
				else
				{
					answerData.AddCostRes(new SmartRes
					{
						items = new List<ItemCount> { phraseReq.itemCount }
					});
				}
				break;
			case QuestPhraseRequirement.Entity.GameResAtom:
				if (phraseReq.requirement == QuestPhraseRequirement.Requirement.Lock)
				{
					answerData.AddLockRes(new SmartRes
					{
						gameRes = new GameRes(new List<GameResAtom> { phraseReq.gameResAtom })
					});
				}
				else
				{
					answerData.AddCostRes(new SmartRes
					{
						gameRes = new GameRes(new List<GameResAtom> { phraseReq.gameResAtom })
					});
				}
				break;
			case QuestPhraseRequirement.Entity.Day:
				answerData.AddDay(phraseReq.dayNumber);
				break;
			case QuestPhraseRequirement.Entity.Order:
				answerData.AddOrder(phraseReq.order);
				break;
			}
		}
		return answerData;
	}

	public bool IsReadyToFinish()
	{
		PlayerData playerData = MainGame.PlayerData;
		foreach (QuestPhraseRequirement phraseReq in phraseReqs)
		{
			switch (phraseReq.entity)
			{
			case QuestPhraseRequirement.Entity.Item:
			{
				QuestPhraseRequirement.Requirement requirement = phraseReq.requirement;
				if ((requirement == QuestPhraseRequirement.Requirement.Lock || requirement == QuestPhraseRequirement.Requirement.Price) && !playerData.Inventory.Data.HasItemQuantityInInventory(phraseReq.itemCount.itemId, phraseReq.itemCount.count))
				{
					return false;
				}
				break;
			}
			case QuestPhraseRequirement.Entity.GameResAtom:
			{
				QuestPhraseRequirement.Requirement requirement = phraseReq.requirement;
				if ((requirement == QuestPhraseRequirement.Requirement.Lock || requirement == QuestPhraseRequirement.Requirement.Price) && !playerData.IsEnoughRes(phraseReq.gameResAtom))
				{
					return false;
				}
				break;
			}
			case QuestPhraseRequirement.Entity.Day:
				if (MainGame.Instance.GameSave.environmentData.CurrentDayNumber != ConstDef.Get(phraseReq.dayNumber).IntValue)
				{
					return false;
				}
				break;
			case QuestPhraseRequirement.Entity.Order:
				if (!MainGame.Instance.GameSave.vendorSystem.IsOrderFinished(phraseReq.order))
				{
					return false;
				}
				break;
			}
		}
		return true;
	}
}
