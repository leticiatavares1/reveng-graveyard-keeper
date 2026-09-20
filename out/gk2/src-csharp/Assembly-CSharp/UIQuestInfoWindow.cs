using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestInfoWindow : LazyWindow<UIQuestInfoWindowData>
{
	[SerializeField]
	private TextMeshProUGUI header;

	[SerializeField]
	private TextMeshProUGUI repLabel;

	[SerializeField]
	private QuestTreeElementWidget currentWidget;

	[SerializeField]
	private LinkedEntityWidget linkedEntityWidgetPrefab;

	[SerializeField]
	private UIDialogWindowButton okBtn;

	[SerializeField]
	private TextStyle enoughStyle;

	[SerializeField]
	private TextStyle notEnoughStyle;

	[SerializeField]
	private TextStyle rewardTextStyle;

	[SerializeField]
	private QuestInfoElement[] questInfoElements;

	[SerializeField]
	private QuestInfoDecor[] decors;

	private List<LinkedEntityWidget> shownLinked = new List<LinkedEntityWidget>();

	private static Pool linkedPool;

	private UIDialogWindowData.ButtonData btnData;

	public override void Init()
	{
		base.Init();
		linkedPool = new Pool(linkedEntityWidgetPrefab, base.transform, 1);
	}

	public override void Redraw()
	{
		base.Redraw();
		btnData = new UIDialogWindowData.ButtonData(Close, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select);
		okBtn.Draw(btnData);
		for (int i = 1; i < questInfoElements.Length; i++)
		{
			questInfoElements[i].gameObject.SetActive(value: false);
		}
		QuestData questData = data.QuestData;
		QuestTreeElementWidgetData questTreeElementWidgetData = new QuestTreeElementWidgetData();
		questTreeElementWidgetData.questData = questData;
		questTreeElementWidgetData.hideQuestionMark = true;
		currentWidget.Draw(questTreeElementWidgetData);
		header.text = LLBase.L(questData.id);
		questInfoElements[0].Draw(questData, ConstructLinkedForQuestData(questData));
		for (int j = 1; j < decors.Length - 1; j++)
		{
			decors[j].gameObject.SetActive(value: false);
			decors[j].DisableAll();
		}
		decors[0].DisableAll();
		decors[^1].DisableAll();
		if (shownLinked.Count > 0)
		{
			decors[0].centerDown.SetActive(value: true);
		}
		else
		{
			decors[0].noCenter.SetActive(value: true);
		}
		List<QuestInfoElement> list = new List<QuestInfoElement>();
		List<string> list2 = new List<string>();
		list.Add(questInfoElements[0]);
		list2.Add(questData.id);
		bool flag = false;
		for (int k = 0; k < questData.Definition.brotherIds.Count; k++)
		{
			if (list.Count == questInfoElements.Length)
			{
				break;
			}
			QuestData questData2 = MainGame.Instance.GameSave.questSystemData.questCollection.questsCache[questData.Definition.brotherIds[k]];
			if (questData2.isHidden)
			{
				continue;
			}
			QuestViewStatus viewStatus = questData2.ViewStatus;
			if (viewStatus != 0 && viewStatus != QuestViewStatus.Unknown && viewStatus != QuestViewStatus.Completed && !list2.Contains(questData2.id))
			{
				if (questData.ViewStatus == QuestViewStatus.Completed && !flag)
				{
					flag = true;
					list.RemoveAt(0);
					list2.RemoveAt(0);
				}
				questInfoElements[list.Count].Draw(questData2, ConstructLinkedForQuestData(questData2));
				list.Add(questInfoElements[list.Count]);
				list2.Add(questData2.id);
			}
		}
		for (int l = 0; l < MainGame.Instance.GameSave.questSystemData.questCollection.quests.Count; l++)
		{
			if (list.Count == questInfoElements.Length)
			{
				break;
			}
			QuestData questData3 = MainGame.Instance.GameSave.questSystemData.questCollection.quests[l];
			if (questData3 != questData && questData3.Definition.TreePos == questData.Definition.TreePos && questData3.status == QuestStatus.InProgress && !list2.Contains(questData3.id))
			{
				questInfoElements[list.Count].Draw(questData3, ConstructLinkedForQuestData(questData3));
				list.Add(questInfoElements[list.Count]);
				list2.Add(questData3.id);
			}
		}
		if (list.Count <= 1)
		{
			if (shownLinked.Count > 0)
			{
				decors[^1].centerUp.SetActive(value: true);
			}
			else
			{
				decors[^1].noCenter.SetActive(value: true);
			}
		}
		else
		{
			QuestInfoElement questInfoElement = list[0];
			for (int m = 1; m < list.Count; m++)
			{
				QuestInfoElement questInfoElement2 = list[m];
				decors[m].gameObject.SetActive(value: true);
				if (questInfoElement.anyLinked)
				{
					if (questInfoElement2.anyLinked)
					{
						decors[m].centerUpAndDown.SetActive(value: true);
					}
					else
					{
						decors[m].centerUp.SetActive(value: true);
					}
				}
				else if (questInfoElement2.anyLinked)
				{
					decors[m].centerDown.SetActive(value: true);
				}
				else
				{
					decors[m].noCenter.SetActive(value: true);
				}
				questInfoElement = questInfoElement2;
			}
			if (questInfoElement.anyLinked)
			{
				decors[^1].centerUp.SetActive(value: true);
			}
			else
			{
				decors[^1].noCenter.SetActive(value: true);
			}
		}
		string text = string.Empty;
		for (int n = 0; n < data.QuestData.Definition.repVisualisationRes.List.Count; n++)
		{
			if (n > 0)
			{
				text += "\n";
			}
			text += data.QuestData.Definition.repVisualisationRes.List[n].ToFormattedString(showOnlyType: false, (string s, string s1) => s + "+" + s1);
		}
		if (string.IsNullOrEmpty(text))
		{
			repLabel.gameObject.SetActive(value: false);
		}
		else
		{
			repLabel.gameObject.SetActive(value: true);
			text = rewardTextStyle.ApplyStyleToString(LLBase.L("ui_reward")) + ":\n" + text;
		}
		repLabel.text = text;
		currentWidget.button.interactable = false;
		((RectTransform)base.transform).RefreshContentFitter();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	public override void Hide()
	{
		currentWidget.Hide();
		foreach (LinkedEntityWidget item in shownLinked)
		{
			linkedPool.ReleaseObject(item);
		}
		shownLinked.Clear();
		base.Hide();
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Clear();
	}

	private List<LinkedEntityWidget> ConstructLinkedForQuestData(QuestData questData)
	{
		List<LinkedEntityWidget> list = new List<LinkedEntityWidget>();
		if (questData.ViewStatus == QuestViewStatus.Completed)
		{
			return list;
		}
		for (int i = 0; i < questData.Definition.finishCheck.phraseReqs.Count; i++)
		{
			LinkedEntityWidget orCreateObject = linkedPool.GetOrCreateObject<LinkedEntityWidget>();
			QuestPhraseRequirement questPhraseRequirement = questData.Definition.finishCheck.phraseReqs[i];
			LinkedEntityWidgetData linkedEntityWidgetData;
			switch (questPhraseRequirement.entity)
			{
			case QuestPhraseRequirement.Entity.Item:
				linkedEntityWidgetData = new LinkedEntityWidgetData(new Item(questPhraseRequirement.itemCount.itemId, questPhraseRequirement.itemCount.count), null);
				if (MainGame.PlayerData.Inventory.Data.GetTotalCountInInventory(questPhraseRequirement.itemCount.itemId) >= questPhraseRequirement.itemCount.count)
				{
					linkedEntityWidgetData.LabelCustomStyle = enoughStyle;
				}
				else
				{
					linkedEntityWidgetData.LabelCustomStyle = notEnoughStyle;
				}
				break;
			case QuestPhraseRequirement.Entity.GameResAtom:
				linkedEntityWidgetData = new LinkedEntityWidgetData(questPhraseRequirement.gameResAtom.type, (int)questPhraseRequirement.gameResAtom.value, null);
				if (MainGame.PlayerData.GetRes(questPhraseRequirement.gameResAtom.type) >= questPhraseRequirement.gameResAtom.value)
				{
					linkedEntityWidgetData.LabelCustomStyle = enoughStyle;
				}
				else
				{
					linkedEntityWidgetData.LabelCustomStyle = notEnoughStyle;
				}
				break;
			case QuestPhraseRequirement.Entity.Day:
				linkedEntityWidgetData = new LinkedEntityWidgetData(questPhraseRequirement.dayNumber, null);
				if (MainGame.Instance.GameSave.environmentData.CurrentDayNumber == ConstDef.Get(questPhraseRequirement.dayNumber).IntValue)
				{
					linkedEntityWidgetData.LabelCustomStyle = enoughStyle;
				}
				else
				{
					linkedEntityWidgetData.LabelCustomStyle = notEnoughStyle;
				}
				break;
			case QuestPhraseRequirement.Entity.Order:
				linkedEntityWidgetData = new LinkedEntityWidgetData(GameBalance.Me.GetData<VendorOrderDef>(questPhraseRequirement.order), null);
				if (MainGame.Instance.GameSave.vendorSystem.IsOrderFinished(questPhraseRequirement.order))
				{
					linkedEntityWidgetData.LabelCustomStyle = enoughStyle;
				}
				else
				{
					linkedEntityWidgetData.LabelCustomStyle = notEnoughStyle;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			orCreateObject.Draw(linkedEntityWidgetData);
			list.Add(orCreateObject);
			shownLinked.Add(orCreateObject);
		}
		return list;
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Select, () => false);
		return gameKeyDelegates;
	}

	protected override void TestDraw()
	{
	}

	[LazyUITest]
	protected void TestDrawUnknown()
	{
		QuestData questData = new QuestData(GameBalance.Me.GetData<QuestDef>("6_intro_guards_burial"));
		questData.isHidden = false;
		questData.isUnknown = true;
		Open(new UIQuestInfoWindowData(questData));
	}

	[LazyUITest]
	protected void TestDrawVisible()
	{
		QuestData questData = new QuestData(GameBalance.Me.GetData<QuestDef>("6_intro_guards_burial"));
		questData.isHidden = false;
		questData.isUnknown = false;
		questData.status = QuestStatus.Available;
		Open(new UIQuestInfoWindowData(questData));
	}

	[LazyUITest]
	protected void TestDrawRevealed()
	{
		QuestData questData = new QuestData(GameBalance.Me.GetData<QuestDef>("6_intro_guards_burial"));
		questData.isHidden = false;
		questData.isUnknown = false;
		questData.status = QuestStatus.InProgress;
		Open(new UIQuestInfoWindowData(questData));
	}

	[LazyUITest]
	protected void TestDrawCompleted()
	{
		QuestData questData = new QuestData(GameBalance.Me.GetData<QuestDef>("6_intro_guards_burial"));
		questData.isHidden = false;
		questData.isUnknown = false;
		questData.status = QuestStatus.Completed;
		Open(new UIQuestInfoWindowData(questData));
	}
}
