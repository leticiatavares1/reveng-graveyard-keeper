using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class TechDef : BalanceBaseObject
{
	private static bool isInitialized;

	[AutoParse("custom_icon_id")]
	public string customIconId;

	[AutoParse("available_at_start")]
	public bool availableAtStart;

	[AutoParse("hidden_at_start")]
	public bool hiddenAtStart;

	[AutoParse("is_available_in_demo")]
	public bool isAvailableInDemo;

	[AutoParse("tab")]
	public TechTreeTab tab;

	[AutoParse("parents")]
	public List<string> parents = new List<string>();

	[AutoParse("xpos")]
	[SerializeField]
	private float posX;

	[AutoParse("ypos")]
	[SerializeField]
	private float posY;

	[AutoParse("red_spheres")]
	[SerializeField]
	private int redSpheresPrice;

	[AutoParse("green_spheres")]
	[SerializeField]
	private int greenSpheresPrice;

	[AutoParse("blue_spheres")]
	[SerializeField]
	private int blueSpheresPrice;

	[AutoParse("char_rep")]
	[SerializeField]
	public GameRes wgoRepLock;

	[AutoParse("dis_rep")]
	[SerializeField]
	public GameRes districtReputationLock;

	[AutoParse("crafts_on_unlock")]
	public List<string> craftsAfterUnlock = new List<string>();

	[AutoParse("alchemy_formulas_on_unlock")]
	public List<string> alchemyFormulasAfterUnlock = new List<string>();

	[AutoParse("buildings_on_unlock")]
	public List<string> buildingsAfterUnlock = new List<string>();

	[AutoParse("town_buildings_on_unlock")]
	public List<string> townBuildingsAfterUnlock = new List<string>();

	[AutoParse("perks_on_unlock")]
	public List<string> perksAfterUnlock = new List<string>();

	[AutoParse("add_res_on_unlock")]
	public GameRes addGameResAfterUnlock;

	[AutoParse("set_res_on_unlock")]
	public GameRes setGameResAfterUnlock;

	[AutoParse("expr_on_unlock")]
	public List<LazyExpression> expressionsAfterUnlock;

	public TechDefType techDefType;

	public TechLockType techLockType;

	public static List<string> FlyingReses = new List<string> { "tech_red", "tech_green", "tech_blue", "happiness" };

	[NonSerialized]
	public List<TechDef> childDefinitionList = new List<TechDef>();

	[NonSerialized]
	public List<TechDef> parentDefinitionList = new List<TechDef>();

	[NonSerialized]
	public List<LinkedEntityWidgetData> linkedEntityWidgetDatas = new List<LinkedEntityWidgetData>();

	private GameRes repRes;

	public Vector2 TreePos => new Vector2(posX, posY);

	public TechState TechState
	{
		get
		{
			if (MainGame.Instance.GameSave.knowledgeSystem.IsTechUnlocked(id))
			{
				return TechState.Unlocked;
			}
			if (MainGame.Instance.GameSave.knowledgeSystem.IsTechHidden(id))
			{
				return TechState.Hidden;
			}
			if (!ParentsUnlocked)
			{
				return TechState.Visible;
			}
			return TechState.Available;
		}
	}

	public bool EnoughResources
	{
		get
		{
			if (MainGame.PlayerData.IsEnoughRes(PriceRes))
			{
				return MainGame.PlayerData.IsEnoughRes(LockRes);
			}
			return false;
		}
	}

	public bool ParentsUnlocked
	{
		get
		{
			bool result = false;
			switch (techLockType)
			{
			case TechLockType.All:
				result = true;
				foreach (string parent in parents)
				{
					if (!MainGame.Instance.GameSave.knowledgeSystem.unlockedTechs.Contains(parent))
					{
						result = false;
						break;
					}
				}
				break;
			case TechLockType.Any:
				foreach (string parent2 in parents)
				{
					if (MainGame.Instance.GameSave.knowledgeSystem.unlockedTechs.Contains(parent2))
					{
						result = true;
					}
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return result;
		}
	}

	public GameRes PriceRes
	{
		get
		{
			GameRes gameRes = new GameRes();
			gameRes.Add("tech_red", redSpheresPrice);
			gameRes.Add("tech_green", greenSpheresPrice);
			gameRes.Add("tech_blue", blueSpheresPrice);
			return gameRes;
		}
	}

	private GameRes LockRes
	{
		get
		{
			GameRes gameRes = new GameRes();
			gameRes.Add(districtReputationLock);
			gameRes.Add(CharReputationLock);
			return gameRes;
		}
	}

	public GameRes CharReputationLock
	{
		get
		{
			if (repRes == null)
			{
				repRes = new GameRes();
			}
			repRes.Clear();
			if (wgoRepLock.List.Count <= 0)
			{
				return repRes;
			}
			WGODef data = GameBalance.Me.GetData<WGODef>(wgoRepLock.List[0].type);
			if (data != null)
			{
				repRes.Add(data.repResName, wgoRepLock.List[0].value);
			}
			return repRes;
		}
	}

	public static void InitTechs()
	{
		if (isInitialized)
		{
			return;
		}
		isInitialized = true;
		foreach (TechDef techDef in GameBalance.Me.techDefs)
		{
			techDef.InitTechDef();
		}
	}

	private void InitTechDef()
	{
		for (int i = 0; i < parents.Count; i++)
		{
			string text = parents[i];
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			TechDef data = GameBalance.Me.GetData<TechDef>(text);
			if (data == null)
			{
				Debug.LogError("no tech definition with id: [" + text + "]");
				continue;
			}
			if (!data.childDefinitionList.Contains(this))
			{
				data.childDefinitionList.Add(this);
			}
			if (!parentDefinitionList.Contains(data))
			{
				parentDefinitionList.Add(data);
			}
		}
		SetLinkedEntityWidgetDatas();
	}

	public void Unlock(bool free = false)
	{
		if (!isAvailableInDemo)
		{
			MainGame.Instance.GameSave.knowledgeSystem.AddDelayedDemoTechUnlock(id);
			return;
		}
		if (!free)
		{
			MainGame.PlayerData.AddRes(PriceRes * -1f);
		}
		MainGame.Instance.GameSave.knowledgeSystem.UnlockTech(id, silent: false);
		foreach (string item in craftsAfterUnlock)
		{
			MainGame.Instance.GameSave.knowledgeSystem.UnlockCraft(item);
		}
		foreach (string item2 in alchemyFormulasAfterUnlock)
		{
			MainGame.Instance.GameSave.knowledgeSystem.UnlockAlchemyFormula(item2);
		}
		foreach (string item3 in buildingsAfterUnlock)
		{
			MainGame.Instance.GameSave.knowledgeSystem.UnlockBuilding(item3);
		}
		foreach (string item4 in townBuildingsAfterUnlock)
		{
			MainGame.Instance.GameSave.knowledgeSystem.UnlockTownBuilding(item4);
		}
		foreach (string item5 in perksAfterUnlock)
		{
			MainGame.Instance.GameSave.perkSystemData.AddPerk(item5);
		}
		MainGame.Instance.GameSave.knowledgeSystem.TryRevealInspirations();
		MainGame.PlayerData.AddRes(addGameResAfterUnlock);
		MainGame.PlayerData.SetRes(setGameResAfterUnlock);
		foreach (LazyExpression item6 in expressionsAfterUnlock)
		{
			item6.Evaluate();
		}
	}

	public void RemoveTech()
	{
		MainGame.Instance.GameSave.knowledgeSystem.RemoveTech(id);
	}

	public string GetPriceLabel(TextStyle normal, TextStyle notEnough, GameResIconType gameResIconType)
	{
		string text = string.Empty;
		for (int i = 0; i < PriceRes.List.Count; i++)
		{
			string str = PriceRes.List[i].ToFormattedString(showOnlyType: false, (string s, string s1) => s + s1, ignoreZeroValues: false, appendSpace: true, gameResIconType);
			str = ((!(MainGame.PlayerData.GetRes(PriceRes.List[i].type) >= PriceRes.List[i].value)) ? notEnough.ApplyStyleToString(str, staticFont: true) : normal.ApplyStyleToString(str, staticFont: true));
			text += str;
			if (i != PriceRes.List.Count - 1)
			{
				text += " ";
			}
		}
		return text;
	}

	private void SetLinkedEntityWidgetDatas()
	{
		for (int i = 0; i < craftsAfterUnlock.Count; i++)
		{
			CraftDef craftDef = GameBalance.GetCraftDef(craftsAfterUnlock[i]);
			linkedEntityWidgetDatas.Add(new LinkedEntityWidgetData(craftDef, null));
		}
		for (int j = 0; j < alchemyFormulasAfterUnlock.Count; j++)
		{
			AlchemyFormulaDef data = GameBalance.Me.GetData<AlchemyFormulaDef>(alchemyFormulasAfterUnlock[j]);
			linkedEntityWidgetDatas.Add(new LinkedEntityWidgetData(data, null));
		}
		for (int k = 0; k < buildingsAfterUnlock.Count; k++)
		{
			BuildingDef data2 = GameBalance.Me.GetData<BuildingDef>(buildingsAfterUnlock[k] ?? "");
			if (data2 != null)
			{
				linkedEntityWidgetDatas.Add(new LinkedEntityWidgetData(data2, null));
			}
		}
		for (int l = 0; l < townBuildingsAfterUnlock.Count; l++)
		{
			TownBuildingDef dataOrNull = GameBalance.Me.GetDataOrNull<TownBuildingDef>(townBuildingsAfterUnlock[l]);
			if (dataOrNull != null)
			{
				linkedEntityWidgetDatas.Add(new LinkedEntityWidgetData(dataOrNull, null));
			}
		}
		for (int m = 0; m < perksAfterUnlock.Count; m++)
		{
			PerkDef data3 = GameBalance.Me.GetData<PerkDef>(perksAfterUnlock[m]);
			if (data3 != null && !data3.isHidden)
			{
				linkedEntityWidgetDatas.Add(new LinkedEntityWidgetData(data3, null));
			}
		}
		foreach (LinkedEntityWidgetData linkedEntityWidgetData in linkedEntityWidgetDatas)
		{
			linkedEntityWidgetData.NotShowStudyWidgetInItemTooltips = true;
			linkedEntityWidgetData.ShowCraftedAtFromCraftDefInsteadOfItem = true;
		}
	}
}
