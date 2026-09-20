using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class QuestTreePageWidget : LazyWidget<QuestTreePageWidgetData>
{
	[Space]
	[SerializeField]
	private LazyScrollRect scrollRect;

	[SerializeField]
	private GamepadNavigationController gamepadNavigationController;

	[Space]
	[SerializeField]
	private Vector2 edgeOffset = new Vector2(10f, 10f);

	[SerializeField]
	private Vector2 elementOffset = new Vector2(210f, 80f);

	[SerializeField]
	private Vector2 elementSize = new Vector2(32f, 32f);

	[SerializeField]
	private GameObject backgroundConnectorsContent;

	[SerializeField]
	private GameObject activeConnectorsContent;

	[SerializeField]
	private GameObject inactiveConnectorsContent;

	[SerializeField]
	private Vector2 connectorPortOffsetUp = new Vector2(0f, 16f);

	[SerializeField]
	private Vector2 connectorPortOffsetDown = new Vector2(0f, -16f);

	[SerializeField]
	private Vector2 connectorPortOffsetRight = new Vector2(16f, 0f);

	[SerializeField]
	private Vector2 connectorPortOffsetLeft = new Vector2(-16f, 0f);

	private List<QuestTreeConnector> connectors = new List<QuestTreeConnector>();

	public override void Init()
	{
		base.Init();
		QuestDef.LinkQuests();
		scrollRect.Init(GetQuestWidget, ReleaseWidgetForParent, gamepadNavigationController);
		SmoothMouseWheelScroll.Ensure(scrollRect);
	}

	public static void OpenWindowFromScratchOnSelectedQuest(QuestData questData)
	{
		if (!MainGame.Instance.GameSave.knowledgeSystem.IsCharTabLocked(CharacterWindowData.CharPage.QuestTree))
		{
			CharacterWindowData characterWindowData = new CharacterWindowData(MainGame.Instance.GameSave, CharacterWindowData.CharPage.QuestTree);
			characterWindowData.FocusOnQuest = questData.id;
			LazyUI.GetWindow<CharacterWindow>().Open(characterWindowData);
			UIQuestInfoWindow window = LazyUI.GetWindow<UIQuestInfoWindow>();
			UIQuestInfoWindowData uIQuestInfoWindowData = new UIQuestInfoWindowData(questData);
			window.Open(uIQuestInfoWindowData);
		}
	}

	public override void Hide()
	{
		HideCurrentElements();
		base.Hide();
	}

	public void Display(string focusOnQuest = "")
	{
		HideCurrentElements();
		HideConnectors();
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < MainGame.Instance.GameSave.questSystemData.questCollection.quests.Count; i++)
		{
			QuestData questData = MainGame.Instance.GameSave.questSystemData.questCollection.quests[i];
			if (!questData.isHidden && (questData.status != QuestStatus.Completed || !HasActiveBrother(questData)) && !IsAnyBrotherAlreadyAdded(questData, hashSet))
			{
				hashSet.Add(questData.id);
				QuestTreeElementWidgetData questTreeElementWidgetData = new QuestTreeElementWidgetData();
				questTreeElementWidgetData.questData = questData;
				questTreeElementWidgetData.onQuestClicked = OnQuestClicked;
				LazyScrollableElement lazyScrollableElement = scrollRect.AddScrollableElement(questTreeElementWidgetData);
				lazyScrollableElement.transform.SetSiblingIndex(i + 4);
				lazyScrollableElement.RectTransform.sizeDelta = elementSize;
			}
		}
		if (!string.IsNullOrEmpty(focusOnQuest))
		{
			scrollRect.DOKill();
			LazyScrollableElement lazyScrollableElement2 = FindQuestByDefinition(GameBalance.Me.GetData<QuestDef>(focusOnQuest));
			scrollRect.ScrollToTargetInstant(lazyScrollableElement2.RectTransform, RectTransform.Axis.Vertical);
		}
		else
		{
			scrollRect.verticalNormalizedPosition = 0f;
		}
		UpdateRectContentSize();
		ApplyDisplayViewStatuses();
		foreach (LazyScrollableElement displayingElement in scrollRect.DisplayingElements)
		{
			QuestTreeElementWidgetData obj = displayingElement.Data as QuestTreeElementWidgetData;
			QuestData questData2 = obj.questData;
			displayingElement.RectTransform.localPosition = new Vector2((float)questData2.Definition.TreePos.x * elementOffset.x + edgeOffset.x, (float)questData2.Definition.TreePos.y * elementOffset.y * -1f - edgeOffset.y);
			obj.downConnectorPos = displayingElement.RectTransform.localPosition + (Vector3)connectorPortOffsetDown;
			obj.upConnectorPos = displayingElement.RectTransform.localPosition + (Vector3)connectorPortOffsetUp;
			obj.leftConnectorPos = displayingElement.RectTransform.localPosition + (Vector3)connectorPortOffsetLeft;
			obj.rightConnectorPos = displayingElement.RectTransform.localPosition + (Vector3)connectorPortOffsetRight;
		}
		((RectTransform)base.transform).RefreshContentFitter();
		for (int j = 0; j < scrollRect.DisplayingElements.Count; j++)
		{
			List<QuestDef> childDefinitionList = (scrollRect.DisplayingElements[j].Data as QuestTreeElementWidgetData).questData.Definition.childDefinitionList;
			for (int k = 0; k < childDefinitionList.Count; k++)
			{
				QuestDef questDef = childDefinitionList[k];
				if (questDef == null || !MainGame.Instance.GameSave.questSystemData.questCollection.questsCache.TryGetValue(questDef.id, out var value) || value.isHidden)
				{
					continue;
				}
				LazyScrollableElement lazyScrollableElement3 = FindQuestByDefinition(questDef);
				if (!(lazyScrollableElement3 == null))
				{
					QuestTreeConnector elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<QuestTreeConnector>(scrollRect.content);
					QuestTreeConnector elementFromPool2 = UIPrefabsPooler.Instance.GetElementFromPool<QuestTreeConnector>(scrollRect.content);
					connectors.Add(elementFromPool);
					connectors.Add(elementFromPool2);
					elementFromPool.Draw(scrollRect.DisplayingElements[j], lazyScrollableElement3, isBackground: false);
					elementFromPool2.Draw(scrollRect.DisplayingElements[j], lazyScrollableElement3, isBackground: true);
					switch (elementFromPool.ConnectorType)
					{
					case QuestTreeConnectorType.Active:
						elementFromPool.transform.SetParent(activeConnectorsContent.transform);
						break;
					case QuestTreeConnectorType.Inactive:
						elementFromPool.transform.SetParent(inactiveConnectorsContent.transform);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
					elementFromPool.transform.SetAsFirstSibling();
					elementFromPool2.transform.SetParent(backgroundConnectorsContent.transform);
					elementFromPool2.transform.SetAsFirstSibling();
				}
			}
		}
		scrollRect.CheckVisibility();
		((RectTransform)base.transform).RefreshContentFitter();
	}

	private void ApplyDisplayViewStatuses()
	{
		Dictionary<string, QuestViewStatus> cache = new Dictionary<string, QuestViewStatus>();
		QuestCollectionData questCollection = MainGame.Instance.GameSave.questSystemData.questCollection;
		foreach (LazyScrollableElement displayingElement in scrollRect.DisplayingElements)
		{
			QuestTreeElementWidgetData obj = (QuestTreeElementWidgetData)displayingElement.Data;
			obj.displayViewStatus = GetDisplayViewStatus(obj.questData, questCollection, cache);
		}
	}

	private static QuestViewStatus GetDisplayViewStatus(QuestData questData, QuestCollectionData questCollection, Dictionary<string, QuestViewStatus> cache)
	{
		if (cache.TryGetValue(questData.id, out var value))
		{
			return value;
		}
		QuestViewStatus viewStatus = questData.ViewStatus;
		if (viewStatus == QuestViewStatus.Unknown)
		{
			cache[questData.id] = QuestViewStatus.Unknown;
			return QuestViewStatus.Unknown;
		}
		cache[questData.id] = viewStatus;
		List<QuestDef> parentDefinitionList = questData.Definition.parentDefinitionList;
		for (int i = 0; i < parentDefinitionList.Count; i++)
		{
			QuestDef questDef = parentDefinitionList[i];
			if (questDef != null && questCollection.questsCache.TryGetValue(questDef.id, out var value2) && GetDisplayViewStatus(value2, questCollection, cache) == QuestViewStatus.Unknown)
			{
				cache[questData.id] = QuestViewStatus.Unknown;
				return QuestViewStatus.Unknown;
			}
		}
		return viewStatus;
	}

	private static bool IsAnyBrotherAlreadyAdded(QuestData questData, HashSet<string> alreadyAddedToTreeIds)
	{
		if (!GameBalance.Me.questBrothersCache.TryGetValue(questData.id, out var value))
		{
			return false;
		}
		QuestCollectionData questCollection = MainGame.Instance.GameSave.questSystemData.questCollection;
		for (int i = 0; i < value.Count; i++)
		{
			string text = value[i];
			if (alreadyAddedToTreeIds.Contains(text))
			{
				if (questData.Definition.brotherIds.Contains(text))
				{
					return true;
				}
				if (questCollection.questsCache.TryGetValue(text, out var value2) && value2.status == QuestStatus.InProgress)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool HasActiveBrother(QuestData questData)
	{
		if (!GameBalance.Me.questBrothersCache.TryGetValue(questData.id, out var value))
		{
			return false;
		}
		QuestCollectionData questCollection = MainGame.Instance.GameSave.questSystemData.questCollection;
		for (int i = 0; i < value.Count; i++)
		{
			string text = value[i];
			if (questCollection.questsCache.TryGetValue(text, out var value2) && value2.IsActiveQuest)
			{
				if (questData.Definition.brotherIds.Contains(text))
				{
					return true;
				}
				if (value2.status == QuestStatus.InProgress)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void HideConnectors()
	{
		foreach (QuestTreeConnector connector in connectors)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(connector);
		}
		connectors.Clear();
	}

	private LazyScrollableElement FindQuestByDefinition(QuestDef questDef)
	{
		foreach (LazyScrollableElement displayingElement in scrollRect.DisplayingElements)
		{
			if ((displayingElement.Data as QuestTreeElementWidgetData).questData.Definition.id == questDef.id)
			{
				return displayingElement;
			}
		}
		Debug.Log("Cannot find element for definition " + questDef.id);
		return null;
	}

	private QuestTreeElementWidget GetQuestWidget(LazyScrollableElement parent)
	{
		QuestTreeElementWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<QuestTreeElementWidget>(parent.RectTransform);
		elementFromPool.Init();
		elementFromPool.Draw(parent.Data);
		((RectTransform)elementFromPool.transform).sizeDelta = elementSize;
		parent.GamepadNavigationItem.SetCallbacks(elementFromPool.button.ForceOnEnter, elementFromPool.button.ForceOnExit, elementFromPool.button.ForceOnClick);
		elementFromPool.transform.position = parent.transform.position;
		return elementFromPool;
	}

	private void ReleaseWidgetForParent(LazyScrollableElement parent)
	{
		ReleaseCommonWidget(parent.Widget as QuestTreeElementWidget);
	}

	private void ReleaseCommonWidget(QuestTreeElementWidget questTreeElementWidget)
	{
		questTreeElementWidget.DeInit();
		questTreeElementWidget.Hide();
		UIPrefabsPooler.Instance.ReleaseElementToPool(questTreeElementWidget);
	}

	private void UpdateRectContentSize()
	{
		if (scrollRect.DisplayingElements.Count == 0)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		foreach (LazyScrollableElement displayingElement in scrollRect.DisplayingElements)
		{
			QuestDef definition = (displayingElement.Data as QuestTreeElementWidgetData).questData.Definition;
			if (definition == null)
			{
				Debug.Log($"data is null?:[{displayingElement.Data == null}]");
			}
			if ((float)definition.TreePos.x > num)
			{
				num = definition.TreePos.x;
			}
			if ((float)definition.TreePos.y > num2)
			{
				num2 = definition.TreePos.y;
			}
		}
		scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, num2 * elementOffset.y + edgeOffset.y * 2f + elementOffset.y / 2f);
	}

	private void HideCurrentElements()
	{
		scrollRect.ClearDisplayingScrollableElements();
	}

	private void OnQuestClicked(QuestTreeElementWidgetData widgetData)
	{
		UIQuestInfoWindow window = LazyUI.GetWindow<UIQuestInfoWindow>();
		UIQuestInfoWindowData uIQuestInfoWindowData = new UIQuestInfoWindowData(widgetData.questData);
		window.Open(uIQuestInfoWindowData);
	}

	public override List<LazyGameKeyTip> GetTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (gamepadNavigationItem != null)
		{
			list.Add(LazyGameKeyTip.Select());
		}
		return list;
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new QuestTreePageWidgetData());
	}
}
