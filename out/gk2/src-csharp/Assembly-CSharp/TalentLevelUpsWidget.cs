using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class TalentLevelUpsWidget : LazyWidget<TalentLevelUpsWidgetData>
{
	[Serializable]
	private class TalentViewData
	{
		public string talentId;

		public string pointIconId;
	}

	[SerializeField]
	private RectTransform content;

	private List<TalentLevelUpWidget> displayedTalentLevelUps = new List<TalentLevelUpWidget>();

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

	[SerializeField]
	private List<TalentViewData> viewDatas = new List<TalentViewData>();

	private TalentViewData currentViewData;

	private List<TalentLevelUpsConnector> connectors = new List<TalentLevelUpsConnector>();

	private Action onQueueChanged;

	public override void Init()
	{
		base.Init();
		TalentLevelUpDef.Link();
	}

	public override void Redraw()
	{
		base.Redraw();
		HideCurrentElements();
		HideConnectors();
		if (data.ZombieWgoData == null)
		{
			currentViewData = viewDatas.Find((TalentViewData d) => d.talentId == data.TalentData.id);
		}
		foreach (TalentLevelUpDef levelUp in data.LevelUps)
		{
			TalentLevelUpWidgetData talentLevelUpWidgetData = ((!levelUp.isZombiePerk) ? new TalentLevelUpWidgetData(levelUp, data.TalentData.GetLevelUpState(levelUp), currentViewData.pointIconId) : new TalentLevelUpWidgetData(levelUp, data.ZombieWgoData.GetLevelUpState(levelUp), data.ZombieWgoData, Redraw));
			talentLevelUpWidgetData.localPosition = new Vector2(levelUp.TreePos.x * elementOffset.x + edgeOffset.x, levelUp.TreePos.y * elementOffset.y * -1f - edgeOffset.y);
			talentLevelUpWidgetData.downConnectorPos = talentLevelUpWidgetData.localPosition + connectorPortOffsetDown;
			talentLevelUpWidgetData.upConnectorPos = talentLevelUpWidgetData.localPosition + connectorPortOffsetUp;
			talentLevelUpWidgetData.leftConnectorPos = talentLevelUpWidgetData.localPosition + connectorPortOffsetLeft;
			talentLevelUpWidgetData.rightConnectorPos = talentLevelUpWidgetData.localPosition + connectorPortOffsetRight;
			TalentLevelUpWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<TalentLevelUpWidget>(content);
			elementFromPool.gameObject.name = levelUp.id;
			elementFromPool.transform.localScale = Vector3.one;
			((RectTransform)elementFromPool.transform).sizeDelta = elementSize;
			elementFromPool.Draw(talentLevelUpWidgetData);
			displayedTalentLevelUps.Add(elementFromPool);
		}
		((RectTransform)base.transform).RefreshContentFitter();
		for (int i = 0; i < displayedTalentLevelUps.Count; i++)
		{
			TalentLevelUpDef def = displayedTalentLevelUps[i].Data.Def;
			List<TalentLevelUpDef> childDefinitionList = def.childDefinitionList;
			for (int j = 0; j < childDefinitionList.Count; j++)
			{
				TalentLevelUpDef talentLevelUpDef = childDefinitionList[j];
				TalentLevelUpsConnector elementFromPool2 = UIPrefabsPooler.Instance.GetElementFromPool<TalentLevelUpsConnector>(content);
				TalentLevelUpsConnector elementFromPool3 = UIPrefabsPooler.Instance.GetElementFromPool<TalentLevelUpsConnector>(content);
				connectors.Add(elementFromPool2);
				connectors.Add(elementFromPool3);
				TalentLevelUpWidget talentLevelUpWidget = FindByDefinition(talentLevelUpDef);
				if (talentLevelUpWidget == null)
				{
					Debug.LogError("Can't find child element:[" + talentLevelUpDef.id + "] for:[" + def.id + "]");
				}
				elementFromPool2.Draw(displayedTalentLevelUps[i], talentLevelUpWidget, isBackground: false);
				elementFromPool3.Draw(displayedTalentLevelUps[i], talentLevelUpWidget, isBackground: true);
				switch (elementFromPool2.TalentConnectorType)
				{
				case TalentConnectorType.Active:
					elementFromPool2.transform.SetParent(activeConnectorsContent.transform);
					break;
				case TalentConnectorType.Inactive:
					elementFromPool2.transform.SetParent(inactiveConnectorsContent.transform);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				elementFromPool2.transform.SetAsFirstSibling();
				elementFromPool3.transform.SetParent(backgroundConnectorsContent.transform);
				elementFromPool3.transform.SetAsFirstSibling();
			}
		}
		backgroundConnectorsContent.transform.SetAsLastSibling();
		inactiveConnectorsContent.transform.SetAsLastSibling();
		activeConnectorsContent.transform.SetAsLastSibling();
		data.onTalentLevelPurchased = OnTalentLevelUpPurchased;
		((RectTransform)base.transform).RefreshContentFitter();
	}

	private TalentLevelUpWidget FindByDefinition(TalentLevelUpDef def)
	{
		foreach (TalentLevelUpWidget displayedTalentLevelUp in displayedTalentLevelUps)
		{
			if (displayedTalentLevelUp.Data.Def == def)
			{
				return displayedTalentLevelUp;
			}
		}
		Debug.Log("Cannot find element for definition " + def.id);
		return null;
	}

	public override void Hide()
	{
		base.Hide();
		data.onTalentLevelPurchased = null;
	}

	private void OnTalentLevelUpPurchased(string talentId, string levelUpId)
	{
		Redraw();
	}

	private void HideCurrentElements()
	{
		for (int num = displayedTalentLevelUps.Count - 1; num >= 0; num--)
		{
			TalentLevelUpWidget talentLevelUpWidget = displayedTalentLevelUps[num];
			talentLevelUpWidget.gameObject.SetActive(value: false);
			talentLevelUpWidget.ClearCallbacks();
			UIPrefabsPooler.Instance.ReleaseElementToPool(talentLevelUpWidget);
		}
		displayedTalentLevelUps.Clear();
	}

	private void HideConnectors()
	{
		foreach (TalentLevelUpsConnector connector in connectors)
		{
			connector.gameObject.SetActive(value: false);
			UIPrefabsPooler.Instance.ReleaseElementToPool(connector);
		}
		connectors.Clear();
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new TalentLevelUpsWidgetData(MainGame.Instance.GameSave.talentSystemData.GetTalentBranch("talent_orange")));
	}
}
