using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class WGODef : BalanceBaseObject
{
	public enum InteractionType
	{
		None = 0,
		Work = 1,
		Craft = 2,
		Script = 3,
		CustomInteraction = 4,
		Builder = 5,
		Chest = 6,
		Grave = 7,
		Ladder = 8,
		PrayerStand = 10,
		Autopsy = 11,
		Garden = 13,
		Zombie = 15,
		Survey = 17,
		Alchemy = 18,
		Reservoir = 19,
		Barricade = 20,
		Flag = 21,
		Station = 22,
		TownBuildingPlace = 23,
		ConveyorCell = 24,
		PowerSource = 25,
		TakeAll = 26,
		FlagStand = 27,
		Embalm = 28,
		FighterContainer = 29,
		FightBuilder = 30,
		TownPalette = 31,
		ChoirPlace = 32,
		ZombieCarrier = 33,
		ZombieSawmill = 34,
		TeleportMilestone = 35,
		PorterStation = 36,
		ZombieMine = 37,
		ZombieClay = 38,
		ZombieSand = 39,
		GardenStation = 40,
		CargoLift = 41,
		Crematorium = 42,
		ConveyorTransporterStation = 43,
		PanicReductionMachine = 44,
		ResurrectionTable = 45,
		WellUpgrade = 46,
		RiverDump = 47
	}

	public enum QualityDisplayType
	{
		Hidden,
		Show
	}

	public enum ForceSetNavigationHoleType
	{
		InsideWorldZone = -1,
		DontSpawnHole,
		SpawnHoleForce
	}

	[AutoParse("custom_visual_id")]
	public string customVisualId;

	[AutoParse("custom_asset_id")]
	[LazyExpressionPureValueType(PureValueType.String)]
	public LazyExpression customAssetId = new LazyExpression();

	[AutoParse("zombie_roll_data_id")]
	public string zombieRollDataId;

	[AutoParse("interaction_type")]
	public InteractionType interactionType;

	public CustomInteraction customInteraction = new CustomInteraction();

	public CustomInteraction customInteraction2 = new CustomInteraction();

	[AutoParse("autocraft_tick_duration")]
	public LazyExpression autocraftTickDuration = new LazyExpression();

	[AutoParse("attached_workbenches")]
	public List<string> attachedWorkbenchExtensionIds = new List<string>();

	[AutoParse("wgo_group")]
	public string wgoGroup;

	[AutoParse("attached_script")]
	public string attachedScript;

	[SerializeField]
	public List<string> teleportDestinationWgoIds = new List<string>();

	public ToolAction toolAction = new ToolAction();

	[AutoParse("inventory_size")]
	public int inventorySize;

	[SerializeField]
	[AutoParse("start_items")]
	public List<NeedItemData> startItems = new List<NeedItemData>();

	public WhiteListItemFilter inventoryWhiteList = new WhiteListItemFilter();

	public BlackListItemFilter inventoryBlackList = new BlackListItemFilter();

	[AutoParse("empty_cell_stack_count")]
	public int emptyCellStackCount = 1;

	[AutoParse("craft_inventory_size")]
	public int craftInventorySize;

	[AutoParse("hp")]
	public int hp;

	[AutoParse("start_hp_val")]
	public int startHpValue = -1;

	[AutoParse("run_le_on_hp_reached")]
	public HPAction hpAction = new HPAction();

	[AutoParse("player_hp_activity_mod")]
	public int playerHpActivityMod = 1;

	[AutoParse("dev_inf_hp")]
	public bool hasInfiniteHp;

	[AutoParse("mastery_lock")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression masteryLock = new LazyExpression();

	public bool noMasteryLock;

	[AutoParse("energy")]
	public LazyExpression energyPerTick;

	[AutoParse("insanity")]
	public LazyExpression insanityPerTick;

	public string worldFxOnHpActionTick;

	[AutoParse("sfx_on_action_tick")]
	public string sfxOnActionTick;

	public Vector3 worldFxOnHpActionSize = Vector3.one;

	public string worldFxOnHpFirstHit;

	public Vector3 worldFxOnHpFirstHitActionSize = Vector3.one;

	[AutoParse("replace_to_wgo_on_die")]
	[LazyExpressionPureValueType(PureValueType.String)]
	public LazyExpression replaceToWgoOnDie = new LazyExpression();

	[AutoParse("revive_on_die")]
	public bool reviveOnDie;

	[AutoParse("transfer_data")]
	public bool transferDataToNewWgo;

	[AutoParse("drop_res_on_die")]
	public OutputItems deathChanceItems;

	[AutoParse("drop_inventory_on_death")]
	public bool dropInventoryOnDeath;

	[AutoParse("expression_on_die")]
	public List<LazyExpression> executeOnDeath = new List<LazyExpression>();

	[AutoParse("expression_on_replace")]
	public List<LazyExpression> executeOnReplace = new List<LazyExpression>();

	[AutoParse("rep_gameres")]
	public string repResName;

	[AutoParse("portrait_asset_id")]
	public string portrait;

	[AutoParse("use_portrait_in_dialogues")]
	public bool usePortraitInDialogues;

	[AutoParse("tech_r")]
	public int techRed;

	[AutoParse("tech_g")]
	public int techGreen;

	[AutoParse("tech_b")]
	public int techBlue;

	[AutoParse("inspiration_on_die")]
	public LazyExpression inspirationOnDeath = new LazyExpression();

	[AutoParse("talent")]
	public string talent;

	public string worldFxOnDie;

	[AutoParse("sfx_on_die")]
	public string sfxOnDie;

	public bool getFxOnDieSizeFromBuildCollider;

	public Vector3 fxOnDieSize = Vector3.one;

	public float customDeathTime = -1f;

	[AutoParse("quality_display_type")]
	public QualityDisplayType qualityDisplayType;

	[AutoParse("quality")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression quality;

	[AutoParse("town_quality")]
	public int townQuality;

	[AutoParse("add_inventory_quality")]
	public bool considerInventoryQuality;

	[AutoParse("dev_label")]
	public string devLabelStr;

	[AutoParse("open_in_multi_inventory")]
	[SerializeField]
	private bool openInMultiInventory;

	[AutoParse("movable")]
	public bool isMovable;

	[AutoParse("can_insert_zombie")]
	public bool canInsertZombie;

	[AutoParse("force_make_nav_hole")]
	public ForceSetNavigationHoleType forceSetNavigationHoleType = ForceSetNavigationHoleType.InsideWorldZone;

	[AutoParse("conveyor_type")]
	public ConveyorElementType conveyorType;

	[AutoParse("conveyor_connectors_setup")]
	public ConveyorConnectorsSetup conveyorConnectorsSetup = new ConveyorConnectorsSetup();

	[AutoParse("npc_life_sim_group")]
	public string npcLifeSimGroup;

	[AutoParse("npc_life_sim_home")]
	public string npcLifeSimHome;

	[AutoParse("fuel_item_id")]
	public string fuelItemId;

	[AutoParse("fuel_item_id2")]
	public string fuelItemId2;

	[AutoParse("ref_to_other_wgo_inventory")]
	public string refToOtherWgoInventory;

	[AutoParse("craft_icon")]
	public string craftIconId;

	[AutoParse("craft_icon_color")]
	public int craftIconColor;

	public bool isAutoCrafter;

	public bool hasCustomVisualId;

	public bool hasCustomAssetId;

	public bool isFuelContainer;

	public bool hasRefToOtherWgoInventory;

	public VoiceID voiceId;

	public ItemDef FuelItemDef => GameBalance.Me.GetData<ItemDef>(fuelItemId);

	public ItemDef FuelItemDef2 => GameBalance.Me.GetData<ItemDef>(fuelItemId2);

	public Sprite Portrait => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(portrait, "portrait_icon_hero");

	public bool OpenInMultiInventory => openInMultiInventory;

	public int MasteryLock
	{
		get
		{
			if (!masteryLock.HasExpression)
			{
				return 1;
			}
			return masteryLock.EvaluateInt();
		}
	}

	public string ResolveAssetId(string fallbackId, WgoData wgoData = null)
	{
		if (!hasCustomAssetId)
		{
			return fallbackId;
		}
		string text = ((wgoData != null) ? customAssetId.Evaluate(wgoData) : customAssetId.Evaluate());
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return fallbackId;
	}

	public bool TryGetBuildingDefForWgo(out BuildingDef buildingDef)
	{
		buildingDef = GameBalance.Me.GetDataOrNull<BuildingDef>(id + "_p");
		if (buildingDef == null)
		{
			buildingDef = GameBalance.Me.GetDataOrNull<BuildingDef>(id + "_s");
			if (buildingDef == null)
			{
				buildingDef = GameBalance.Me.GetDataOrNull<BuildingDef>(id + "_cp");
				if (buildingDef == null)
				{
					buildingDef = GameBalance.Me.GetDataOrNull<BuildingDef>(id + "_fp");
					if (buildingDef == null)
					{
						buildingDef = GameBalance.Me.GetDataOrNull<BuildingDef>(id + "_fp");
					}
				}
			}
		}
		return buildingDef != null;
	}
}
