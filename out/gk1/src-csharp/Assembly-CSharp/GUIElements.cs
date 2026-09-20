using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(0)]
public class GUIElements : MonoBehaviour
{
	private static GUIElements _instance;

	public UIWidget screen_size_widget;

	public WidgetsBubbleGUI bubble_widgets_container;

	public GameGUI game_gui;

	public MainMenuGUI main_menu;

	public InGameMenuGUI ingame_menu;

	public OptionsMenuGUI options;

	public LiveStreamingGUI live_streaming;

	public SaveSlotsMenuGUI saves;

	public TechTreeGUI tech_tree;

	public TimeMachineGUI time_machine_gui;

	public GlobalCraftControlGUI global_craft_control_gui;

	public SleepGUI sleep_gui;

	public WaitingGUI waiting_gui;

	public DialogGUI dialog;

	public TechUnlockDialogGUI tech_dialog;

	public LoadingGUI loading;

	public UILabel time_label;

	public CraftGUI craft;

	public ResourceBasedCraftGUI resource_based_craft;

	public PrayCraftGUI pray_craft;

	public MixedCraftGUI mixed_craft;

	public MixedCraftGUI mixed_craft_tabbed;

	public BodyCraftGUI body_craft;

	public InventoryGUI inventory;

	public EquipToToolbarGUI equip_to_toolbar;

	public VendorGUI vendor;

	public ChestGUI chest;

	public ItemCountGUI item_count;

	public BuffsGUI buffs;

	public EffectBubblesManager effect_bubbles;

	public AutopsyGUI autopsy;

	public SoulExtractorGUI soul_extractor_gui;

	public SoulContainerGUI soul_container_gui;

	public SoulHealerGUI soul_healer_gui;

	public OrganEnhancerGUI organ_enhancer_gui;

	public TextWindowGUI text_window;

	public CinematicTextGUI cinematic_text;

	public HUD hud;

	public SpeechBubbleGUI speech_bubble;

	public TooltipBubbleGUI tooltip_bubble;

	public ContextMenuBubbleGUI context_menu_bubble;

	public InteractionBubbleGUI interaction_bubble;

	public MultiAnswerGUI multi_answer;

	public CornerTalkGUI corner_talk;

	public RelationGUI relation;

	public RelationGUI relation_additional;

	public BuildModeGUI build_mode_gui;

	public CraftResourcesSelectGUI resource_picker;

	public DropResHint drop_res_hint;

	public UIPanel drop_hint_panel;

	public GraveGUI grave;

	public BuildsGUI builds;

	public QuestListGUI quest_list;

	public OverheadGUI overhead;

	public TutorialArrowGUI tutorial_arrow;

	public BuffsBarGUI buffs_bar;

	public TutorialGUI tutorial;

	public FishingGUI fishing;

	public DungeonWindowGUI dungeon_window;

	public GameObject mixer_voting;

	public UI2DSprite mixer_progress_bar;

	public NPCsListGUI npcs_list;

	public UIPanel overhead_panel;

	public PrayReportGUI pray_report;

	public TavernEventReportGUI tavern_event_report;

	public TechPointsSpawner tech_points_spawner;

	public BodyStorageGUI body_storage;

	public MapGUI map;

	private bool _initialized;

	public bool hud_enabled = true;

	public static bool gui_is_initializing;

	public FlyingObject flying_buff_prefab;

	public DiskIndicatorGUI disk_indicator;

	public UIPanel buffs_panel;

	public CreditsGUI credits;

	public UIPanel black_background;

	public Camera half_resolution_camera;

	public MeshRenderer half_resolution_mesh;

	public PorterStationGUI porter_station;

	public ResurrectionGUI resurrection_gui;

	public RatCellGUI rat_cell_gui;

	public IllustrationsGUI illustrations_gui;

	public TutorialWindowsGUI tutorial_windows_gui;

	public NewBodyArrivedGUI body_arrived_gui;

	public static GUIElements me
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType<GUIElements>();
			}
			return _instance;
		}
	}

	public static bool dont_allow_change_input_method
	{
		get
		{
			if (BaseGUI.all_guis_closed)
			{
				return false;
			}
			if (BaseGUI.opened_windows.Count <= 1)
			{
				if (BaseGUI.opened_windows.Count == 1)
				{
					return BaseGUI.opened_windows[0] != me.main_menu;
				}
				return false;
			}
			return true;
		}
	}

	private void OnEnable()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		AlwaysActiveOrInactive[] componentsInChildren = GetComponentsInChildren<AlwaysActiveOrInactive>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateState();
		}
	}

	public void Init()
	{
		AlwaysActiveOrInactive[] componentsInChildren = GetComponentsInChildren<AlwaysActiveOrInactive>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateState();
		}
		if (!_initialized)
		{
			_initialized = true;
			RecalcScreenResolution();
			disk_indicator.gameObject.SetActive(value: false);
			gui_is_initializing = true;
			ReLinkElements();
			LazyInput.on_input_changed += BaseGUI.UpdateSourceType;
			_instance = this;
			text_window.Init();
			cinematic_text.Init();
			sleep_gui.Init();
			waiting_gui.Init();
			craft.Init();
			global_craft_control_gui.Init();
			resource_based_craft.Init();
			pray_craft.Init();
			mixed_craft.Init();
			body_craft.Init();
			inventory.Init();
			equip_to_toolbar.Init();
			vendor.Init();
			chest.Init();
			item_count.Init();
			effect_bubbles.Init();
			buffs.Init();
			hud.Init();
			DropCollectGUI componentInChildren = GetComponentInChildren<DropCollectGUI>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.Start();
			}
			autopsy.Init();
			soul_extractor_gui.Init();
			soul_container_gui.Init();
			soul_healer_gui.Init();
			organ_enhancer_gui.Init();
			if (tech_tree != null)
			{
				tech_tree.Init();
			}
			if (build_mode_gui != null)
			{
				build_mode_gui.Init();
			}
			if (quest_list != null)
			{
				quest_list.Init();
			}
			if (tutorial_arrow != null)
			{
				tutorial_arrow.Init();
			}
			if (drop_res_hint != null)
			{
				drop_res_hint.Init();
			}
			if (body_storage != null)
			{
				body_storage.Init();
			}
			if (mixed_craft_tabbed != null)
			{
				mixed_craft_tabbed.Init();
			}
			bubble_widgets_container.InitWidgetsContainer();
			tooltip_bubble.Init();
			context_menu_bubble.Init();
			interaction_bubble.Init();
			speech_bubble.Init();
			multi_answer.Init();
			corner_talk.Init();
			relation.Init();
			relation_additional.Init();
			game_gui.Init();
			main_menu.Init();
			ingame_menu.Init();
			options.Init();
			saves.Init();
			grave.Init();
			resource_picker.Init();
			live_streaming.Init();
			dialog.Init();
			tech_dialog.Init();
			tutorial.Init();
			loading.Init();
			pray_report.Init();
			tavern_event_report.Init();
			builds?.Init();
			buffs_bar?.Init();
			fishing?.Init();
			map?.Init();
			porter_station?.Init();
			resurrection_gui?.Init();
			rat_cell_gui?.Init();
			illustrations_gui?.Init();
			tutorial_windows_gui?.Init();
			npcs_list.Init();
			time_machine_gui.Init();
			hud.Open();
			UpdateGUISizeSettings();
			if (MainGame.loaded_from_scene_main)
			{
				main_menu.Open();
			}
			UITextStyles.me.Deactivate();
			if (drop_hint_panel != null)
			{
				drop_hint_panel.gameObject.SetActive(value: true);
			}
			if (flying_buff_prefab != null)
			{
				flying_buff_prefab.gameObject.SetActive(value: false);
			}
			if (buffs_panel != null)
			{
				buffs_panel.gameObject.SetActive(value: false);
			}
			credits.Init();
			if (credits != null)
			{
				credits.SetActive(active: false);
			}
			if (black_background != null)
			{
				black_background.gameObject.SetActive(value: false);
			}
			dungeon_window.Init();
			gui_is_initializing = false;
			PlatformSpecific.ApplyScreenSafeZones();
			UpdateLanguageChangeForAllBaseGUI();
		}
	}

	public void RecalcScreenResolution(int w = -1, int h = -1)
	{
		if (!(screen_size_widget == null))
		{
			if (w == -1)
			{
				w = Screen.width;
			}
			if (h == -1)
			{
				h = Screen.height;
			}
			screen_size_widget.leftAnchor.target = (screen_size_widget.rightAnchor.target = (screen_size_widget.topAnchor.target = (screen_size_widget.bottomAnchor.target = null)));
			screen_size_widget.width = w / 2;
			screen_size_widget.height = h / 2;
			ResolutionConfig resolutionConfigOrNull = ResolutionConfig.GetResolutionConfigOrNull(w, h);
			if (resolutionConfigOrNull != null)
			{
				Transform obj = game_gui.transform;
				Transform obj2 = inventory.transform;
				Transform obj3 = map.transform;
				Transform obj4 = npcs_list.transform;
				Vector3 vector2 = (tech_tree.transform.localScale = Vector3.one * resolutionConfigOrNull.large_gui_scale);
				Vector3 vector4 = (obj4.localScale = vector2);
				Vector3 vector6 = (obj3.localScale = vector4);
				Vector3 localScale = (obj2.localScale = vector6);
				obj.localScale = localScale;
			}
		}
	}

	public void Update()
	{
		if (!(MainGame.me == null) && MainGame.me.save != null)
		{
			time_label.text = "Time: " + MainGame.me.save.cur_time;
		}
	}

	public void LateUpdate()
	{
		if (overhead != null)
		{
			overhead.CustomUpdate();
		}
	}

	public void InitAtGameStart()
	{
	}

	public void OpenCraftGUI(WorldGameObject craftery_wgo)
	{
		if (craftery_wgo.components.craft.enabled && craftery_wgo.components.craft.is_crafting && !craftery_wgo.obj_def.can_insert_zombie)
		{
			return;
		}
		if (craftery_wgo.is_body_storage)
		{
			body_storage.Open(craftery_wgo);
			return;
		}
		if (craftery_wgo.is_autopsy_table)
		{
			autopsy.Open(craftery_wgo);
			return;
		}
		if (craftery_wgo.obj_def.HasObjectGroupWithID("pulpit"))
		{
			pray_craft.Open(craftery_wgo);
			return;
		}
		if (craftery_wgo.obj_def.HasObjectGroupWithID("balsamation"))
		{
			body_craft.Open(craftery_wgo);
			return;
		}
		if (craftery_wgo.is_rat_cell)
		{
			rat_cell_gui.Open(craftery_wgo);
			return;
		}
		if (craftery_wgo.is_soul_extractor_table)
		{
			soul_extractor_gui.Open(craftery_wgo);
			return;
		}
		if (craftery_wgo.obj_id == "soul_healer")
		{
			soul_healer_gui.Open(craftery_wgo);
			return;
		}
		if (craftery_wgo.obj_id == "soul_workbench")
		{
			craftery_wgo.components.craft.FillCraftsList();
		}
		foreach (CraftDefinition craft in craftery_wgo.components.craft.crafts)
		{
			switch (craft.craft_type)
			{
			case CraftDefinition.CraftType.ResourcesBasedCraft:
			case CraftDefinition.CraftType.Survey:
			case CraftDefinition.CraftType.AlchemyDecompose:
				resource_based_craft.Open(craftery_wgo, craft.craft_type);
				return;
			case CraftDefinition.CraftType.PrayCraft:
				pray_craft.Open(craftery_wgo);
				return;
			case CraftDefinition.CraftType.MixedCraft:
			{
				if (craftery_wgo.obj_def.filter_craft_subtype == CraftDefinition.CraftSubType.Alchemy)
				{
					this.craft.OpenAsAlchemy(craftery_wgo);
					return;
				}
				List<string> prodlist = craftery_wgo.obj_def.res_product_types;
				mixed_craft.Open(craftery_wgo, craftery_wgo.obj_def.craft_preset, allow_empty: false, delegate(Item item, InventoryWidget widget)
				{
					if (item.definition == null)
					{
						return InventoryWidget.ItemFilterResult.Hide;
					}
					if (prodlist.Count == 0)
					{
						return InventoryWidget.ItemFilterResult.Active;
					}
					foreach (string product_type in item.definition.product_types)
					{
						if (prodlist.Contains(product_type))
						{
							return InventoryWidget.ItemFilterResult.Active;
						}
					}
					return InventoryWidget.ItemFilterResult.Inactive;
				});
				return;
			}
			}
		}
		this.craft.OpenCraftList(craftery_wgo);
	}

	public void ReLinkElements()
	{
		craft = GetComponentInChildren<CraftGUI>(includeInactive: true);
		resource_based_craft = GetComponentInChildren<ResourceBasedCraftGUI>(includeInactive: true);
		pray_craft = GetComponentInChildren<PrayCraftGUI>(includeInactive: true);
		mixed_craft = GetComponentInChildren<MixedCraftGUI>(includeInactive: true);
		sleep_gui = GetComponentInChildren<SleepGUI>(includeInactive: true);
		waiting_gui = GetComponentInChildren<WaitingGUI>(includeInactive: true);
		inventory = GetComponentInChildren<InventoryGUI>(includeInactive: true);
		equip_to_toolbar = GetComponentInChildren<EquipToToolbarGUI>(includeInactive: true);
		vendor = GetComponentInChildren<VendorGUI>(includeInactive: true);
		chest = GetComponentInChildren<ChestGUI>(includeInactive: true);
		item_count = GetComponentInChildren<ItemCountGUI>(includeInactive: true);
		buffs = GetComponentInChildren<BuffsGUI>(includeInactive: true);
		effect_bubbles = GetComponentInChildren<EffectBubblesManager>(includeInactive: true);
		autopsy = GetComponentInChildren<AutopsyGUI>(includeInactive: true);
		hud = GetComponentInChildren<HUD>(includeInactive: true);
		text_window = GetComponentInChildren<TextWindowGUI>(includeInactive: true);
		cinematic_text = GetComponentInChildren<CinematicTextGUI>(includeInactive: true);
		speech_bubble = GetComponentInChildren<SpeechBubbleGUI>(includeInactive: true);
		tooltip_bubble = GetComponentInChildren<TooltipBubbleGUI>(includeInactive: true);
		context_menu_bubble = GetComponentInChildren<ContextMenuBubbleGUI>(includeInactive: true);
		interaction_bubble = GetComponentInChildren<InteractionBubbleGUI>(includeInactive: true);
		multi_answer = GetComponentInChildren<MultiAnswerGUI>(includeInactive: true);
		corner_talk = GetComponentInChildren<CornerTalkGUI>(includeInactive: true);
		relation = GetComponentInChildren<RelationGUI>(includeInactive: true);
		game_gui = GetComponentInChildren<GameGUI>(includeInactive: true);
		main_menu = GetComponentInChildren<MainMenuGUI>(includeInactive: true);
		ingame_menu = GetComponentInChildren<InGameMenuGUI>(includeInactive: true);
		options = GetComponentInChildren<OptionsMenuGUI>(includeInactive: true);
		saves = GetComponentInChildren<SaveSlotsMenuGUI>(includeInactive: true);
		dialog = GetComponentInChildren<DialogGUI>(includeInactive: true);
		tech_dialog = GetComponentInChildren<TechUnlockDialogGUI>(includeInactive: true);
		loading = GetComponentInChildren<LoadingGUI>(includeInactive: true);
		builds = GetComponentInChildren<BuildsGUI>(includeInactive: true);
		tech_tree = GetComponentInChildren<TechTreeGUI>(includeInactive: true);
		build_mode_gui = GetComponentInChildren<BuildModeGUI>(includeInactive: true);
		overhead = GetComponentInChildren<OverheadGUI>(includeInactive: true);
		quest_list = GetComponentInChildren<QuestListGUI>(includeInactive: true);
		tutorial_arrow = GetComponentInChildren<TutorialArrowGUI>(includeInactive: true);
		grave = GetComponentInChildren<GraveGUI>(includeInactive: true);
		resource_picker = GetComponentInChildren<CraftResourcesSelectGUI>(includeInactive: true);
		drop_res_hint = GetComponentInChildren<DropResHint>(includeInactive: true);
		buffs_bar = GetComponentInChildren<BuffsBarGUI>(includeInactive: true);
		tutorial = GetComponentInChildren<TutorialGUI>(includeInactive: true);
		fishing = GetComponentInChildren<FishingGUI>(includeInactive: true);
		dungeon_window = GetComponentInChildren<DungeonWindowGUI>(includeInactive: true);
		body_storage = GetComponentInChildren<BodyStorageGUI>(includeInactive: true);
		porter_station = GetComponentInChildren<PorterStationGUI>(includeInactive: true);
		resurrection_gui = GetComponentInChildren<ResurrectionGUI>(includeInactive: true);
		rat_cell_gui = GetComponentInChildren<RatCellGUI>(includeInactive: true);
		illustrations_gui = GetComponentInChildren<IllustrationsGUI>(includeInactive: true);
		tutorial_windows_gui = GetComponentInChildren<TutorialWindowsGUI>(includeInactive: true);
	}

	public void UpdateGUISizeSettings()
	{
		GetComponentInChildren<Camera>().orthographicSize = 0.5f;
	}

	public static void ChangeHUDAlpha(bool show, bool animated)
	{
		me.hud.gameObject.TryFinishAlphaTween();
		me.buffs.buffs_hud.TryFinishAlphaTween();
		if (!animated)
		{
			me.hud.panel.alpha = (show ? 1f : 0f);
			me.buffs.buffs_hud_panel.alpha = (show ? 1f : 0f);
		}
		else
		{
			me.hud.panel.ChangeAlpha(me.hud.panel.alpha, show ? 1f : 0f, 0.2f);
			me.buffs.buffs_hud_panel.ChangeAlpha(me.buffs.buffs_hud_panel.alpha, show ? 1f : 0f, 0.2f);
		}
	}

	public static void ChangeBubblesVisibility(bool show)
	{
		EffectBubblesManager.ChangeBubblesVisibility(show);
		InteractionBubbleGUI.ChangeBubblesVisibility(show);
	}

	public void EnableHUD(bool enable)
	{
		Debug.Log("EnableHUD: " + enable);
		hud.gameObject.SetActive(enable);
		hud_enabled = enable;
	}

	public bool IsAnyMassiveWindowOpened()
	{
		if (!craft.is_shown && !grave.is_shown && !inventory.is_shown && !resource_based_craft.is_shown && !resource_picker.is_shown && !chest.is_shown && !pray_craft.gameObject.activeSelf && !fishing.is_shown)
		{
			return MainGame.me.gui_elements.build_mode_gui.is_shown;
		}
		return true;
	}

	public void ShowSavingStatus(bool show)
	{
		Debug.Log("ShowSavingStatus: " + show);
		disk_indicator.SetActive(show);
		LocalizedLabel[] componentsInChildren = disk_indicator.GetComponentsInChildren<LocalizedLabel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Localize();
		}
	}

	public static void UpdateLanguageChangeForAllBaseGUI()
	{
		if (!(me == null))
		{
			if (me.options.is_shown)
			{
				me.options.UpdateLocalizedLabels();
			}
			BaseGUI[] componentsInChildren = me.GetComponentsInChildren<BaseGUI>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				GJL.EnsureChildLabelsHasCorrectFont(componentsInChildren[i].gameObject);
			}
			GJL.EnsureChildLabelsHasCorrectFont(me.hud.gameObject);
		}
	}

	public void CloseAllInGameWindows()
	{
		InteractionBubbleGUI.DestroyAll();
	}

	public void OpenPorterStationGUI(WorldGameObject wgo)
	{
		porter_station.Open(wgo);
	}

	public void OpenResurrectionGUI(WorldGameObject wgo)
	{
		resurrection_gui.Open(wgo);
	}
}
