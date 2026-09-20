using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPrefightWindow : LazyWindow<UIPrefightWindowData>
{
	[SerializeField]
	private TextMeshProUGUI headerLabel;

	[SerializeField]
	private TextMeshProUGUI squadValueLabel;

	[SerializeField]
	private TextMeshProUGUI defencePowerValueLabel;

	[SerializeField]
	private TextMeshProUGUI buildingsInZoneLabel;

	[SerializeField]
	private GameObject noBuildingsObj;

	[SerializeField]
	private GameObject buildingsObj;

	[SerializeField]
	private GameObject noRewardsObj;

	[SerializeField]
	private GameObject rewardsObj;

	[SerializeField]
	private TextStyle slashStyleDefault;

	[SerializeField]
	private TextStyle notEnoughStyle;

	[SerializeField]
	private TextStyle notEnoughStyleSlash;

	[SerializeField]
	private TextStyle enoughStyle;

	[SerializeField]
	private TextStyle enoughStyleSlash;

	[SerializeField]
	private TextStyle buildingsPowerStyle;

	[SerializeField]
	private UIPrefightSquadWidget[] squadWidgets;

	[SerializeField]
	private RectTransform entitiesContent;

	[SerializeField]
	private RectTransform entitiesBackground;

	[SerializeField]
	private ContentSizeFitter entitiesSizeFitter;

	[SerializeField]
	private ScrollRect entitiesScrollRect;

	[SerializeField]
	private float maxEntitiesBackgroundWidth;

	[SerializeField]
	private LinkedEntityWidget linkedEntityWidgetPrefab;

	[SerializeField]
	private UIItemCell[] rewardCells;

	[SerializeField]
	private LazyButton startFightButton;

	private List<LinkedEntityWidget> shownLinked = new List<LinkedEntityWidget>();

	private static Pool linkedPool;

	public override void Init()
	{
		base.Init();
		linkedPool = new Pool(linkedEntityWidgetPrefab, base.transform, 1);
		linkedEntityWidgetPrefab.gameObject.SetActive(value: false);
		startFightButton.onClick.AddListener(OnStartButtonPressed);
		UIMouseTooltip.Attach(squadValueLabel.transform.parent.gameObject, "tt_prefight_1", null, addRaycastTarget: true, disableChildRaycasts: true);
	}

	public override void Redraw()
	{
		base.Redraw();
		UpdateCloseButtonState();
		data.OnPressedSquad = OnPressedSquad;
		foreach (LinkedEntityWidget item in shownLinked)
		{
			linkedPool.ReleaseObject(item);
		}
		shownLinked.Clear();
		headerLabel.text = LLBase.L(data.FightDefinition.id);
		startFightButton.interactable = data.HasEnoughDefencePower;
		RedrawSquadAndDefenceLabels();
		string buildingPower = buildingsPowerStyle.ApplyStyleToString(data.BuildingsQuality.ToString());
		buildingsInZoneLabel.text = GetBuildingsInZoneText(buildingPower);
		UIItemCell[] array = rewardCells;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		if (data.FightDefinition.rewards.Count > 0)
		{
			for (int j = 0; j < data.FightDefinition.rewards.Count; j++)
			{
				rewardCells[j].gameObject.SetActive(value: true);
				rewardCells[j].Draw(new Item(data.FightDefinition.rewards[j].id, data.FightDefinition.rewards[j].GetCount()));
			}
			noRewardsObj.SetActive(value: false);
			rewardsObj.SetActive(value: true);
		}
		else
		{
			noRewardsObj.SetActive(value: true);
			rewardsObj.SetActive(value: false);
		}
		entitiesSizeFitter.enabled = true;
		entitiesScrollRect.enabled = false;
		if (data.BuildingInZone.Count > 0)
		{
			noBuildingsObj.SetActive(value: false);
			buildingsObj.SetActive(value: true);
			GameRes gameRes = new GameRes();
			for (int k = 0; k < data.BuildingInZone.Count; k++)
			{
				gameRes.Add(data.BuildingInZone[k].id, 1f);
			}
			for (int l = 0; l < gameRes.List.Count; l++)
			{
				LinkedEntityWidget orCreateObject = linkedPool.GetOrCreateObject<LinkedEntityWidget>();
				GameResAtom gameResAtom = gameRes.List[l];
				BuildingDef buildingDef = GameBalance.Me.GetData<BuildingDef>(gameResAtom.type + "_fb");
				bool isInactive = (data.FightDefinition.isBarricadesUnavailable && GameBalance.Me.HasWgoIdByGroup("barricades", buildingDef.wgoId)) || (data.FightDefinition.isTowersUnavailable && GameBalance.Me.HasWgoIdByGroup("towers", buildingDef.wgoId));
				LinkedEntityWidgetData linkedEntityWidgetData = new LinkedEntityWidgetData(GameBalance.Me.GetData<BuildingDef>(gameResAtom.type + "_fb"), null, (int)gameResAtom.value, isInactive);
				orCreateObject.Draw(linkedEntityWidgetData);
				orCreateObject.gameObject.SetActive(value: true);
				orCreateObject.transform.SetParent(entitiesContent);
				shownLinked.Add(orCreateObject);
			}
			if (shownLinked.Count > 9)
			{
				entitiesSizeFitter.enabled = false;
				entitiesBackground.sizeDelta = new Vector2(entitiesBackground.sizeDelta.x, maxEntitiesBackgroundWidth);
				entitiesScrollRect.enabled = true;
			}
		}
		else
		{
			noBuildingsObj.SetActive(value: true);
			buildingsObj.SetActive(value: false);
		}
		DisableLastSquadWidget();
		int val = ((squadWidgets != null) ? Math.Max(0, squadWidgets.Length - 1) : 0);
		int num = Math.Min(data.SquadWidgetDatas.Count, val);
		for (int m = 0; m < num; m++)
		{
			if (squadWidgets != null)
			{
				squadWidgets[m].Draw(data.SquadWidgetDatas[m]);
			}
		}
		DisableLastSquadWidget();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		((RectTransform)base.transform).RefreshContentFitter();
	}

	private void RedrawSquadAndDefenceLabels()
	{
		string arg = slashStyleDefault.ApplyStyleToString("/");
		squadValueLabel.text = $"{data.CurrentSquadCount}{arg}{data.FightDefinition.squads}";
		bool showDefencePowerRequirement = data.ShowDefencePowerRequirement;
		UIMouseTooltip.Attach(defencePowerValueLabel.transform.parent.gameObject, showDefencePowerRequirement ? "tt_prefight_2" : "tt_prefight_2b", null, addRaycastTarget: true, disableChildRaycasts: true);
		if (!showDefencePowerRequirement)
		{
			string text = enoughStyle.ApplyStyleToString($"{data.CurrentDefencePower}");
			defencePowerValueLabel.text = "barracks".FontIcon() + text;
		}
		else if (data.CurrentDefencePower >= data.FightDefinition.defencePowerLock)
		{
			string text2 = enoughStyle.ApplyStyleToString($"{data.CurrentDefencePower}");
			string text3 = enoughStyleSlash.ApplyStyleToString("/");
			defencePowerValueLabel.text = string.Format("{0}{1}{2}{3}", "barracks".FontIcon(), text2, text3, data.FightDefinition.defencePowerLock);
		}
		else
		{
			string text4 = notEnoughStyle.ApplyStyleToString($"{data.CurrentDefencePower}");
			string text5 = notEnoughStyleSlash.ApplyStyleToString("/");
			defencePowerValueLabel.text = string.Format("{0}{1}{2}{3}", "barracks".FontIcon(), text4, text5, data.FightDefinition.defencePowerLock);
		}
	}

	private bool OnStartPressed()
	{
		if (startFightButton.interactable)
		{
			OnStartButtonPressed();
			return true;
		}
		return false;
	}

	private void OnStartButtonPressed()
	{
		Close();
		MainGame.Instance.GameSave.militaryBaseData.fighterContainersSelectedForFight = new List<SGuid>();
		for (int i = 0; i < data.SquadWidgetDatas.Count; i++)
		{
			if (data.SquadWidgetDatas[i].IsTurnedOn)
			{
				MainGame.Instance.GameSave.militaryBaseData.fighterContainersSelectedForFight.Add(data.SquadWidgetDatas[i].WgoData.UniqueId);
			}
		}
		data.OnStartButtonPressed?.Invoke();
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.StartFight, OnStartPressed);
		return gameKeyDelegates;
	}

	protected override bool OnPressedBack()
	{
		if (data != null && !data.AllowClose)
		{
			return false;
		}
		return base.OnPressedBack();
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		UpdateCloseButtonState();
	}

	protected override void PrintTips()
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>
		{
			new LazyGameKeyTip(GameKey.StartFight, "start_fight", startFightButton.interactable),
			LazyGameKeyTip.Select()
		};
		if (data == null || data.AllowClose)
		{
			list.Add(LazyGameKeyTip.Back());
		}
		lazyButtonTips.Print(list);
	}

	private void UpdateCloseButtonState()
	{
		if ((bool)closeButton)
		{
			bool flag = data?.AllowClose ?? true;
			closeButton.interactable = flag;
			closeButton.gameObject.SetActive(flag && !LazyInput.IsGamepadActive);
		}
	}

	private void OnPressedSquad()
	{
		RedrawSquadAndDefenceLabels();
		startFightButton.interactable = data.HasEnoughDefencePower && (MainGame.PlayerController.Sword.id != "empty" || MainGame.PlayerController.Bow.id != "empty");
		if (LazyInput.IsGamepadActive)
		{
			PrintTips();
		}
	}

	private void DisableLastSquadWidget()
	{
		if (squadWidgets != null && squadWidgets.Length != 0)
		{
			squadWidgets[^1].gameObject.SetActive(value: false);
		}
	}

	private string GetBuildingsInZoneText(string buildingPower)
	{
		if (data.FightDefinition.isBarricadesUnavailable && data.FightDefinition.isTowersUnavailable)
		{
			return LLBase.L("ui_fight_construction_unavailable") ?? "";
		}
		if (data.FightDefinition.isBarricadesUnavailable)
		{
			return LLBase.L("ui_fight_barricades_unavailable") + " (" + "barracks".FontIcon() + buildingPower + "):";
		}
		if (data.FightDefinition.isTowersUnavailable)
		{
			return LLBase.L("ui_fight_towers_unavailable") + " (" + "barracks".FontIcon() + buildingPower + "):";
		}
		return LLBase.L("ui_buildings_quality_in_zone") + " (" + "barracks".FontIcon() + buildingPower + "):";
	}

	protected override void TestDraw()
	{
	}
}
