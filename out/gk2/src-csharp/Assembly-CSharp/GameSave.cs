using System;
using System.Collections.Generic;
using System.Globalization;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class GameSave : ISerializableData
{
	private const string PREORDER_REWARD_BODY_PART_ID = "bdy_9007";

	private const string PREORDER_REWARD_ARMS_PART_ID = "arm_9007";

	private const int PREORDER_REWARD_PART_SKIN_ID = 9007;

	[SerializeField]
	private string gameSaveVersion;

	public bool isDemoSaveLoadedInReleaseCustomActionsApplied;

	public WorldData worldData = new WorldData();

	public EnvironmentData environmentData = new EnvironmentData();

	public GameLogicsSystemData gameLogicSystemData = new GameLogicsSystemData();

	public PlayerData playerData = new PlayerData();

	public TalentSystemData talentSystemData;

	public PerkSystemData perkSystemData = new PerkSystemData();

	public KnowledgeSystem knowledgeSystem = new KnowledgeSystem();

	public GlobalEventsSystem globalEventsSystem = new GlobalEventsSystem();

	public QuestSystemData questSystemData = new QuestSystemData();

	public VendorSystem vendorSystem = new VendorSystem();

	public TownSystem townSystem = new TownSystem();

	public WeatherData weatherData = new WeatherData();

	public CraftSystemData craftSystemData = new CraftSystemData();

	public MovementSystemData movementSystemData = new MovementSystemData();

	public ZombieSystemData zombieSystemData = new ZombieSystemData();

	public ConveyorSystemData conveyorSystemData = new ConveyorSystemData();

	public RiverDropSystemData riverDropSystemData = new RiverDropSystemData();

	public MilitaryBaseData militaryBaseData = new MilitaryBaseData();

	public WgoCustomDeathSystemData wgoCustomDeathSystemData = new WgoCustomDeathSystemData();

	public NPCLifeSimulatorData npcLifeSimulatorData = new NPCLifeSimulatorData();

	public WgoDelayedEventSystemData wgoDelayedEventSystemData = new WgoDelayedEventSystemData();

	public WgoDelayedSpawnSystemData wgoDelayedSpawnSystemData = new WgoDelayedSpawnSystemData();

	public AchievementsSystem achievementsSystem = new AchievementsSystem();

	public NetworkPlayer hostPlayer;

	public List<NetworkPlayer> clientPlayers = new List<NetworkPlayer>();

	[NonSerialized]
	private GameSaveVersion saveVersion;

	public string GameSaveVer
	{
		get
		{
			return gameSaveVersion;
		}
		set
		{
			gameSaveVersion = value;
			if (string.IsNullOrWhiteSpace(value))
			{
				saveVersion = default(GameSaveVersion);
			}
			else
			{
				saveVersion = GameSaveVersion.Parse(value);
			}
		}
	}

	public GameSaveVersion SaveVersion => saveVersion;

	public WorldData WorldData => worldData;

	public static void SetupNewGameSave(GameSave save)
	{
		save.playerData = PlayerData.CreatePlayerData();
		save.talentSystemData = new TalentSystemData(GameBalance.Me.talentDefs);
		foreach (TechDef techDef in GameBalance.Me.techDefs)
		{
			if (techDef.availableAtStart)
			{
				save.knowledgeSystem.UnlockTech(techDef.id, silent: true);
			}
			if (techDef.hiddenAtStart)
			{
				save.knowledgeSystem.hiddenTechs.Add(techDef.id);
			}
		}
		foreach (TalentDef talentDef in GameBalance.Me.talentDefs)
		{
			save.knowledgeSystem.unlockedTalentIds.Add(talentDef.id);
		}
		save.knowledgeSystem.revealedTechs = new List<string>();
		save.knowledgeSystem.revealedTalentLevelUps = new List<string>();
		foreach (TalentLevelUpDef talentLevelUpDef in GameBalance.Me.talentLevelUpDefs)
		{
			if (talentLevelUpDef.isHidden)
			{
				save.knowledgeSystem.hiddenTalentLevelUps.Add(talentLevelUpDef.id);
			}
			if (talentLevelUpDef.isUnknown)
			{
				save.knowledgeSystem.unknownTalentLevelUps.Add(talentLevelUpDef.id);
			}
		}
		foreach (AlchemyFormulaDef alchemyFormulaDef in GameBalance.Me.alchemyFormulaDefs)
		{
			if (alchemyFormulaDef.hiddenAtStart)
			{
				save.knowledgeSystem.hiddenAlchemyFormulas.Add(alchemyFormulaDef.id);
			}
		}
		foreach (SurveyDef surveyDef in GameBalance.Me.surveyDefs)
		{
			if (surveyDef.surveyedAtStart && !save.knowledgeSystem.oneTimeCompletedCrafts.Contains(surveyDef.id))
			{
				save.knowledgeSystem.oneTimeCompletedCrafts.Add(surveyDef.id);
			}
		}
		for (int i = 0; i < GameBalance.Me.vendorDefs.Count; i++)
		{
			VendorDef vendorDef = GameBalance.Me.vendorDefs[i];
			save.vendorSystem.vendors.Add(new Vendor(vendorDef.id, 0));
			if (!vendorDef.lockedByDefaultInOrdersWindow)
			{
				save.knowledgeSystem.unlockedVendorsForOrders.Add(vendorDef.id);
			}
		}
		for (int j = 0; j < ConstDef.Get("start_orders_count").IntValue; j++)
		{
			save.vendorSystem.currentOrders.Add(SGuid.Empty);
		}
		foreach (NPCGroupPointOfInterestConfiguration allGroup in LazySingletonSO<NPCLifeSimulatorConfiguration>.Instance.AllGroups)
		{
			save.npcLifeSimulatorData.AddGroup(new NPCGroupPointOfInterestData(allGroup));
		}
		foreach (NPCPointOfInterestConfiguration allPont in LazySingletonSO<NPCLifeSimulatorConfiguration>.Instance.AllPonts)
		{
			save.npcLifeSimulatorData.AddPoint(new NPCPointOfInterestData(allPont));
		}
		save.worldData.InitFromSceneConfigs(MainGame.Instance.gameSceneConfigs);
		save.questSystemData = new QuestSystemData(GameBalance.Me.questDefs);
		for (int k = 0; k < 100; k++)
		{
			string text = $"zombie_name_{k + 1}";
			string text2 = LLBase.L(text);
			if (!(text == text2))
			{
				save.knowledgeSystem.freeZombieNames.Add(text);
			}
		}
		for (int l = 0; l < GameBalance.Me.inspirationDefs.Count; l++)
		{
			InspirationDef inspirationDef = GameBalance.Me.inspirationDefs[l];
			if (inspirationDef.inpsirationLocks.Count > 0 || inspirationDef.techLocks.Count > 0 || inspirationDef.questLocks.Count > 0)
			{
				string idWithoutLvl = inspirationDef.idWithoutLvl;
				if (!save.knowledgeSystem.hiddenInspirations.Contains(idWithoutLvl))
				{
					save.knowledgeSystem.hiddenInspirations.Add(idWithoutLvl);
				}
			}
		}
	}

	public void PrepareForGame()
	{
		environmentData.PrepareForGame(EnvironmentEngine.Instance.gameplayDayInMinutes);
		globalEventsSystem.PrepareForGame();
		questSystemData.PrepareForGame();
		movementSystemData.RestoreMovingObjects(worldData);
		worldData.PrepareForGame();
		craftSystemData.RestoreActiveCrafts(worldData);
		MainGame.Instance.fightingLevelSystem.PrepareForGame();
		talentSystemData.PrepareForGame();
		gameLogicSystemData.PrepareForGame();
		playerData.PrepareForGame();
		knowledgeSystem.PrepareForGame();
		vendorSystem.PrepareForGame();
		zombieSystemData.PrepareForGame();
		militaryBaseData.PrepareForGame();
		npcLifeSimulatorData.PrepareForGame();
		conveyorSystemData.PrepareForGame(worldData);
		if (riverDropSystemData == null)
		{
			riverDropSystemData = new RiverDropSystemData();
		}
		MainGame.Instance?.riverDropSystem?.ClearRuntimeState();
		foreach (string knownMixCraft in knowledgeSystem.knownMixCrafts)
		{
			GameBalance.GetAlchemyMixCraftDef(knownMixCraft);
		}
		(achievementsSystem ?? (achievementsSystem = new AchievementsSystem())).PrepareForGame();
	}

	public void UnPrepareFromGame()
	{
		MainGame.Instance?.fightingLevelSystem.UnprepareFromGame();
		MainGame.Instance?.riverDropSystem?.ClearRuntimeState();
		playerData.UnPrepareFromGame();
		worldData.UnPrepareFromGame();
		knowledgeSystem.UnPrepareForGame();
		talentSystemData?.UnPrepareFromGame();
	}

	public NetworkPlayer CreateClient(int clientId)
	{
		PlayerData playerData = PlayerData.CreatePlayerData();
		playerData.TryApplyStartState();
		NetworkPlayer networkPlayer = new NetworkPlayer(clientId, playerData);
		clientPlayers.Add(networkPlayer);
		return networkPlayer;
	}

	public bool GetClient(int clientId, out NetworkPlayer clientPlayer, bool considerHost = false)
	{
		clientPlayer = null;
		if (considerHost && clientId == hostPlayer.clientId)
		{
			clientPlayer = hostPlayer;
			return true;
		}
		foreach (NetworkPlayer clientPlayer2 in clientPlayers)
		{
			if (clientPlayer2.clientId == clientId)
			{
				clientPlayer = clientPlayer2;
				return true;
			}
		}
		Debug.LogError($"Client with clientId [{clientId}] not found");
		return false;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterSerialize()
	{
		if (this.wgoDelayedEventSystemData == null)
		{
			this.wgoDelayedEventSystemData = new WgoDelayedEventSystemData();
		}
		WgoDelayedEventSystemData wgoDelayedEventSystemData = this.wgoDelayedEventSystemData;
		if (wgoDelayedEventSystemData.wgoUniqueIds == null)
		{
			wgoDelayedEventSystemData.wgoUniqueIds = new List<SGuid>();
		}
		if (!string.IsNullOrEmpty(gameSaveVersion))
		{
			saveVersion = GameSaveVersion.Parse(gameSaveVersion);
		}
	}

	public void PrepareToSave(SaveSlotData slotData)
	{
		slotData.gameSaveVersion = GameSaveVer;
		slotData.day = environmentData.Day;
		slotData.platform = LazyAPI.Platform.GetPlatformName();
		slotData.serializedCulture = CultureInfo.CurrentCulture.Name;
		slotData.saveDateTime = DateTime.Now.ToString(CultureInfo.CurrentCulture);
		WorldZoneData worldZoneDataById = WorldData.GetWorldZoneDataById("graveyard");
		if (worldZoneDataById != null)
		{
			slotData.graveyardQuality = (int)worldZoneDataById.GetTotalQuality();
		}
		WorldZoneData worldZoneDataById2 = WorldData.GetWorldZoneDataById("church");
		if (worldZoneDataById2 != null)
		{
			slotData.churchQuality = (int)worldZoneDataById2.GetTotalQuality();
		}
		slotData.villageRep = playerData.GetNPCRep("village_REP");
		slotData.isDemoSave = true;
	}

	public void GivePreorderReward()
	{
		string text = "";
		string text2 = "";
		string text3 = "";
		text = "9007_bdy_01_clr_01_palette";
		text2 = "9007_bdy_02_clr_01_palette";
		text3 = "9007_bdy_03_clr_01_palette";
		playerData.customization.UnlockCustomizationPart("bdy_9007", CustomizablePartType.Body);
		playerData.customization.UnlockCustomizationColor(PlayerColorCustomizationType.Bdy1, text, 9007);
		playerData.customization.UnlockCustomizationColor(PlayerColorCustomizationType.Bdy2, text2, 9007);
		playerData.customization.UnlockCustomizationColor(PlayerColorCustomizationType.Bdy3, text3, 9007);
		GameScriptUtility.RunGlobalScript("DLC_BathhouseSpawn");
		Debug.Log("GivePreorderReward success");
	}

	public void TryRemovePreorderReward()
	{
		Debug.Log("No preorder available TryRemovePreorderReward");
		PlayerCustomizationData customization = playerData.customization;
		bool flag = IsCustomizationPartSelected(customization, CustomizablePartType.Body, "bdy_9007");
		bool flag2 = IsCustomizationPartSelected(customization, CustomizablePartType.Arms, "arm_9007");
		RemoveUnlockedPart(customization, CustomizablePartType.Body, "bdy_9007");
		RemoveUnlockedPart(customization, CustomizablePartType.Arms, "arm_9007");
		string[] preorderRewardPaletteNames = GetPreorderRewardPaletteNames();
		for (int i = 0; i < preorderRewardPaletteNames.Length; i++)
		{
			RemoveUnlockedColor(customization, PlayerColorCustomizationType.Bdy1, 9007, preorderRewardPaletteNames[i]);
		}
		if (flag)
		{
			SetCustomizationPart(customization, CustomizablePartType.Body, GetDefaultCustomizationPartId(CustomizablePartType.Body));
			SetCustomizationPart(customization, CustomizablePartType.Arms, GetDefaultCustomizationPartId(CustomizablePartType.Arms));
			customization.SetColorCustomizationIndexForType(PlayerColorCustomizationType.Bdy1, 0);
		}
		else if (flag2)
		{
			SetCustomizationPart(customization, CustomizablePartType.Arms, GetDefaultCustomizationPartId(CustomizablePartType.Arms));
		}
		if (WorldData.TryGetWgoData("bathhouse", out var foundWgoData, out var foundGameScene))
		{
			WorldData.RemoveWgoDataFromGameScene(foundWgoData);
			Debug.Log("RemovePreorderReward success");
		}
		if (WorldData.TryGetWgoData("tp_RT_bathhouse_enter", out var foundWgoData2, out foundGameScene))
		{
			WorldData.RemoveWgoDataFromGameScene(foundWgoData2);
		}
	}

	private static string[] GetPreorderRewardPaletteNames()
	{
		return new string[3] { "9007_bdy_01_clr_01_palette", "9007_bdy_02_clr_01_palette", "9007_bdy_03_clr_01_palette" };
	}

	private static void RemoveUnlockedPart(PlayerCustomizationData customization, CustomizablePartType type, string id)
	{
		UnlockedCustomizationPartData unlockedCustomizationPartData = customization.unlockedCustomizationPartsData.Find((UnlockedCustomizationPartData x) => x.type == type);
		if (unlockedCustomizationPartData != null)
		{
			unlockedCustomizationPartData.unlockedIds.Remove(id);
			if (unlockedCustomizationPartData.unlockedIds.Count == 0)
			{
				customization.unlockedCustomizationPartsData.Remove(unlockedCustomizationPartData);
			}
		}
	}

	private static void RemoveUnlockedColor(PlayerCustomizationData customization, PlayerColorCustomizationType type, int partSkinId, string paletteName)
	{
		UnlockedColorCustomizationData unlockedColorCustomizationData = customization.unlockedColorCustomizationData.Find((UnlockedColorCustomizationData x) => x.type == type && x.partSkinId == partSkinId);
		if (unlockedColorCustomizationData != null)
		{
			unlockedColorCustomizationData.unlockedNames.Remove(paletteName);
			if (unlockedColorCustomizationData.unlockedNames.Count == 0 && unlockedColorCustomizationData.unlockedIndices.Count == 0)
			{
				customization.unlockedColorCustomizationData.Remove(unlockedColorCustomizationData);
			}
		}
	}

	private static bool IsCustomizationPartSelected(PlayerCustomizationData customization, CustomizablePartType type, string id)
	{
		PlayerCustomizationPartData playerCustomizationPartData = customization.customizationPartsData.Find((PlayerCustomizationPartData x) => x.type == type);
		if (playerCustomizationPartData != null)
		{
			return playerCustomizationPartData.id == id;
		}
		return false;
	}

	private static void SetCustomizationPart(PlayerCustomizationData customization, CustomizablePartType type, string id)
	{
		PlayerCustomizationPartData playerCustomizationPartData = customization.customizationPartsData.Find((PlayerCustomizationPartData x) => x.type == type);
		if (playerCustomizationPartData != null)
		{
			playerCustomizationPartData.id = id;
		}
		else
		{
			customization.customizationPartsData.Add(new PlayerCustomizationPartData(id, type));
		}
	}

	private static string GetDefaultCustomizationPartId(CustomizablePartType type)
	{
		PlayerCustomizationPartData playerCustomizationPartData = PlayerSkinHelper.DefaultCustomizationParts.Find((PlayerCustomizationPartData x) => x.type == type);
		if (playerCustomizationPartData == null)
		{
			return string.Empty;
		}
		return playerCustomizationPartData.id;
	}
}
