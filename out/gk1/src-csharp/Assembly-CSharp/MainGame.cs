using System;
using System.Collections.Generic;
using CodeStage.AdvancedFPSCounter;
using Com.LuisPedroFonseca.ProCamera2D;
using DarkTonic.MasterAudio;
using FlowCanvas;
using LinqTools;
using ParadoxNotion.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;

public class MainGame : MonoBehaviour
{
	public enum GameMode
	{
		Normal,
		Building
	}

	private const float RES_DEATH_LOOSE = 0.1f;

	public const bool BUILD_FOR_TRAILER = false;

	public static MainGame me;

	public static bool loaded_from_scene_main;

	public BuildGrid build_grid;

	public GameObject build_grid_cell;

	public DockPointMarker dock_point_marker;

	public WorldGameObject player;

	[NonSerialized]
	public World world;

	[HideInInspector]
	public BaseCharacterComponent player_char;

	private Camera _camera;

	private bool _camera_set;

	public UIRoot ui_root;

	public ProCamera2D pro_camera;

	public Camera world_cam;

	public Camera gui_cam;

	public GameMode game_mode;

	public Transform world_root;

	private FloatingWorldGameObject _floating_obj;

	[NonSerialized]
	public GameSave save = new GameSave();

	[NonSerialized]
	public SaveSlotData save_slot = new SaveSlotData();

	public AstarPath astar;

	public GUIElements gui_elements;

	public static bool game_starting;

	private static bool _initial_preparations_done;

	public int gui_pixel_zoom = 2;

	[NonSerialized]
	public Vector3 player_pos = Vector3.zero;

	[NonSerialized]
	public BuildModeLogics build_mode_logics = new BuildModeLogics();

	private float _session_start;

	public static bool game_started;

	public static float camera_z;

	public static bool disable_all_game;

	private static float _start_time;

	public NoiseAndGrain grain_fx_component;

	private bool _editor_superfastforward_mode;

	public Texture2D mouse_cursor;

	public static bool paused;

	private TextureDrawer _dungeon_root;

	private bool _dungeon_root_set;

	private const string _STRANGER_SINS_POPUP_TEXT = "stranger_sins_popup_window";

	private const string _REFUGEES_POPUP_TEXT = "game_of_crone_popup_window";

	private const string _SOULS_POPUP_TEXT = "better_save_soul_popup_window";

	public static float game_time => (float)me.save.day + TimeOfDay.me.GetTimeK();

	public static float session_time => game_time - me._session_start;

	public TextureDrawer dungeon_root
	{
		get
		{
			if (!_dungeon_root_set)
			{
				_dungeon_root = UnityEngine.Object.FindObjectOfType<TextureDrawer>();
				_dungeon_root_set = true;
			}
			return _dungeon_root;
		}
	}

	public PlayerComponent player_component => player.GetComponentInChildren<PlayerComponent>();

	public void Awake()
	{
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
		if (_initial_preparations_done)
		{
			Debug.Log("Second MainGame scene awake");
			return;
		}
		_initial_preparations_done = true;
		game_starting = true;
		Preloader.OnMainGameLoaded();
		StartGameLoading();
	}

	private void StartGameLoading()
	{
		disable_all_game = false;
		_start_time = Time.realtimeSinceStartup;
		Debug.Log("StartGameLoading, time = " + Time.time);
		me = this;
		PlatformSpecific.Init();
		SmartResourceHelper.me.max_simultaneous_loading_files = 4;
		PreloadResources();
		AstarPath.can_scan_on_startup = false;
		GameSettings.me.ApplyLanguageChange();
		EasySpritesCollection.Load();
		camera_z = base.transform.position.z;
		grain_fx_component = GetComponent<NoiseAndGrain>();
		GameSettings.Init();
		Time.fixedDeltaTime = 1f / 60f;
		string text = base.gameObject.scene.name;
		if (text.Contains("DontDestroyOnLoad"))
		{
			text = SceneManager.GetActiveScene().name;
		}
		Debug.Log("Starting scene: " + text, base.gameObject);
		loaded_from_scene_main = true;
		if (loaded_from_scene_main)
		{
			GameLoader.InitGameFromGUIScene();
		}
		else
		{
			GameLoader.InitGameFromWorldScene();
		}
		DungeonRoomsContainer.FillRoomsDict();
	}

	public void GeneralInit()
	{
		Debug.Log("GeneralInit, time = " + Time.time);
		Stats.Init();
		LazyEngine.Init(world_root, new EngineCallbacks());
		ChunkManager.Init();
		GameAwakenerEngine.Init();
		DynamicLights.Init();
		LazyInput.Init();
		Sounds.Init();
		MonoManager.Create();
		GameSettings.me.ApplyVolume();
		if (!Preloader.awake_was_done)
		{
			GameSettings.me.ApplyScreenMode();
		}
		ShadowsPool.Init();
		Time.fixedDeltaTime = 1f / 60f;
		player_char = player.components.character;
		gui_elements.Init();
		GameBalance.LoadGameBalance();
		ObjectDynamicShadow.InstantiateAllAdditionalShadows();
		save.obj_crafts.AddCraft("p:grave_place");
		save.obj_crafts.AddCraft("p:working_table");
		player.GetComponent<ChunkedGameObject>().always_active = true;
		gui_elements.Init();
		gui_elements.InitAtGameStart();
		TechDefinition.LinkTechs();
		CameraTools.ReCache();
		_session_start = game_time;
		if (!ui_root)
		{
			ui_root = UnityEngine.Object.FindObjectOfType<UIRoot>();
		}
		if (!pro_camera)
		{
			pro_camera = UnityEngine.Object.FindObjectOfType<ProCamera2D>();
		}
		OnScreenSizeChanged();
		player.GetComponent<WorldGameObject>().ApplyObjectGroup(GameBalance.me.GetData<ObjectGroupDefinition>("_char"));
		player.gameObject.SetActive(value: true);
		FogObject.InitFog(Resources.Load<FogObject>("fog object prefab"));
		game_starting = false;
		CameraTools.MoveToPos(player.tf.position);
		CustomUpdateManager.Init();
		if (UnityEngine.Object.FindObjectOfType<GameDebugConfiguration>().multiplayer)
		{
			CustomNetworkManager.is_running = true;
		}
		else
		{
			CustomNetworkManager.is_running = false;
		}
		ChunkManager.RescanAllObjects();
		RoundAndSortComponent.DisableComponentOnStaticObjects();
		Debug.Log("GeneralInit - Done, time = " + Time.time);
		MixerLightIntegration.Init();
		GJTimer.AddTimer(0f, delegate
		{
			EasySpritesCollection.LoadAllAtlasesAsync();
			MasterAudio.StopAllPlaylists();
			MasterAudio.TriggerPlaylistClip("menu", "menu");
			ShowDLCPopUpWindowIfNeed();
		});
	}

	public void InitGUIScene(SceneDescription world_scene)
	{
		if (world_scene == null)
		{
			Debug.LogError("world_scene is null");
		}
		if (world_scene.main_camera == null)
		{
			Debug.LogError("world_scene.main_camera is null");
		}
		Debug.Log("InitGUIScene, this: " + base.name + ", world: " + world_scene.main_camera.name);
		me = this;
		player = world_scene.main_camera.player;
		UnityEngine.Object.Destroy(world_scene.main_camera.gameObject.GetFirstParent());
		World.InitWorldOnApplicationStart();
		GeneralInit();
	}

	public void OnScreenSizeChanged(int w = -1, int h = -1)
	{
		if (w == -1)
		{
			w = Screen.width;
		}
		if (h == -1)
		{
			h = Screen.height;
		}
		Vector2 vector = new Vector2(w, h);
		Debug.Log($"OnScreenSizeChanged {vector}");
		if (GameSettings.current_resolution == null)
		{
			Debug.LogError($"Unsupported resolution {w}x{h}!");
			GameSettings.current_resolution = new ResolutionConfig(w, h);
		}
		Vector2 vector2 = vector * 2f / GameSettings.current_resolution.pixel_size;
		Debug.Log($"Setting camera size: {vector2} for pixel size: {GameSettings.current_resolution.pixel_size}");
		pro_camera.GetComponent<ProCamera2DPixelPerfect>().ViewportAutoScale = AutoScaleMode.None;
		GetComponent<Camera>().orthographicSize = h / GameSettings.current_resolution.pixel_size;
		ui_root.manualHeight = (int)vector.y / gui_pixel_zoom;
		if (GUIElements.me != null)
		{
			GUIElements.me.RecalcScreenResolution(w, h);
		}
		ChunkManager.RecalculateResolution(w, h);
	}

	public GameObject InstantiatePrefab(string prefab_name)
	{
		GameObject gameObject = Resources.Load<GameObject>(prefab_name);
		if (gameObject == null)
		{
			Debug.LogError("Error loading prefab: " + prefab_name);
			return null;
		}
		GameObject obj = UnityEngine.Object.Instantiate(gameObject);
		obj.transform.SetParent(world_root, worldPositionStays: false);
		obj.transform.localPosition = Vector3.zero;
		return obj;
	}

	public void Update()
	{
		QualitySettings.vSyncCount = 1;
		if (game_started)
		{
			InGameUpdate();
		}
		if (GamePadController.cheat_combination_pressed)
		{
			UnityEngine.Object.FindObjectOfType<AFPSCounter>().SwitchCounter();
		}
		CustomDebugInfoPanel.Update();
	}

	private void InGameUpdate()
	{
		save.game_logics.Update();
		if (!paused)
		{
			BuffsLogics.RecalculateBuffs();
		}
		if (game_mode == GameMode.Building)
		{
			build_mode_logics.Update();
		}
		if (BaseGUI.all_guis_closed && !_editor_superfastforward_mode)
		{
			UtilityStuff.ProcessInGameUpdate();
		}
	}

	private void OnGameLoaded(GameSave save_data)
	{
		Debug.Log("OnGameLoaded");
		save = save_data;
		save.map.RestoreScene();
		Debug.Log("OnGameLoaded: complete, time = " + Time.time);
	}

	public void RescanPathMap()
	{
		astar.Scan();
	}

	public List<WorldGameObject> GetListOfWorldObjects()
	{
		return UnityEngine.Object.FindObjectsOfType<WorldGameObject>().ToList();
	}

	public void OnPlayerDied()
	{
		Debug.Log("OnPlayerDied");
		EnvironmentEngine.State engineGlobalState = EnvironmentEngine.me.data.state;
		GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag("gd_player_respawn");
		GameObject target;
		if (gDPointByGDTag == null)
		{
			Debug.LogError("Can't find GD point: gd_player_respawn");
			target = me.player.gameObject;
		}
		else
		{
			target = gDPointByGDTag.gameObject;
			engineGlobalState = EnvironmentEngine.State.Inside;
		}
		me.player.is_dead = true;
		GUIElements.me.overhead.Deactivate();
		CameraTools.RemoveFromCameraTargets(player.transform, 0f);
		GS.SetPlayerEnable(player_enabled: false, affect_cinematic: true);
		EffectBubblesManager.RemoveAllBubbles();
		player_char.SetAnimationState(CharAnimState.Idle);
		player_char.LookAt(Direction.Down);
		player.transform.position = target.transform.position;
		player.RefreshPositionCache();
		EnvironmentEngine.me.SetEngineGlobalState(engineGlobalState);
		string text = GJL.L("player_died");
		string text2 = "";
		string[] array = new string[3] { "_r", "_g", "_b" };
		foreach (string text3 in array)
		{
			int num = Mathf.RoundToInt(player.GetParam(text3) * 0.1f);
			if (num != 0)
			{
				player.SubParam(text3, num);
				if (text2.Length > 0)
				{
					text2 += " ";
				}
				text2 = text2 + "(" + text3 + ")" + num;
			}
		}
		if (!string.IsNullOrEmpty(text2))
		{
			text = text + "\n" + GJL.L("you_lost") + " " + text2.Replace("_", "");
		}
		if (me.dungeon_root.dungeon_is_loaded_now)
		{
			if (me.dungeon_root.TrySaveDungeon())
			{
				Debug.Log("Successfully saved dungeon.");
			}
			me.dungeon_root.DestroyTiles();
			MasterAudio.StopPlaylist("ambient");
			SmartAudioEngine.me.StopOvrMusic("dungeon", force_immediate: true);
			EnvironmentEngine.me.SetEngineGlobalState(EnvironmentEngine.State.Inside);
		}
		GUIElements.me.dialog.OpenOK(text, delegate
		{
			CameraTools.Fade(delegate
			{
				player.hp = save.max_hp;
				player.AddToParams("deaths_count", 1f);
				save.quests.CheckKeyQuests("player_dead");
				save.quests.CheckKeyQuests("player_dead_" + player.GetParamInt("deaths_count"));
				CameraTools.MoveToPos(target.transform.position);
				CameraTools.AddToCameraTargets(player.transform);
				GJTimer.AddTimer(0.5f, delegate
				{
					CameraTools.UnFade(delegate
					{
						player.is_dead = false;
						GS.SetPlayerEnable(player_enabled: true, affect_cinematic: true);
						save.quests.CheckKeyQuests("player_respawn");
						save.quests.CheckKeyQuests("player_respawn_" + player.GetParamInt("deaths_count"));
					}, 0.8f);
				});
			}, 1.5f);
		});
	}

	public void RestartDemoBuild()
	{
		me.player.gameObject.SetActive(value: false);
		Time.timeScale = 1f;
		UnityEngine.Object.Destroy(GameObject.Find("UI Root"));
		CameraTools.Fade(delegate
		{
			Time.timeScale = 0f;
			Application.Quit();
		});
	}

	public void SetMainPlayer(PlayerComponent p)
	{
		player = p.GetComponent<WorldGameObject>();
		player_char = p.wgo.components.character;
		player_char.can_be_locally_controlled = true;
		SetCameraPlayerFollow(p.gameObject.transform);
	}

	public void SetCameraPlayerFollow(Transform t)
	{
		CameraTools.AddToCameraTargets(t);
	}

	public void EnterBuildMode(bool removing_mode = false)
	{
		game_mode = GameMode.Building;
		Debug.Log("EnterBuildMode");
		player.components.character.control_enabled = false;
		if (removing_mode)
		{
			build_mode_logics.EnterRemoveMode();
		}
		else
		{
			build_mode_logics.EnterBuildMode();
		}
	}

	public void EnterScriptBuilding()
	{
		game_mode = GameMode.Building;
		Debug.Log("EnterWaitingScriptCallbackMode");
		build_mode_logics.EnterScriptBuilding();
	}

	public void ExitBuildMode()
	{
		game_mode = GameMode.Normal;
		Debug.Log("ExitBuildMode");
		player.components.character.control_enabled = true;
	}

	public DungeonPreset TeleportToDungeonLevel(int level)
	{
		DungeonPreset preset = Resources.Load<DungeonPreset>("Dungeon/DungeonPresets/" + level);
		if (preset == null)
		{
			Debug.LogError("Dungeon preset, level = " + level + " not found.");
			me.player.Say("No more levels in alpha.", null, null, SpeechBubbleGUI.SpeechBubbleType.InfoBox);
			return null;
		}
		Stats.DesignEvent("Dungeon:" + preset.dungeon_level + ":Load");
		if (dungeon_root.dungeon_is_loaded_now)
		{
			GJTimer.AddTimer(0.01f, delegate
			{
				if (dungeon_root.TrySaveDungeon())
				{
					Debug.Log("Successfully saved dungeon.");
				}
				dungeon_root.DestroyTiles();
				dungeon_root.DrawTexture(preset);
				player.transform.position = dungeon_root.enter_to_dunge.transform.position;
				CameraTools.MoveToPos(player.transform.position);
			});
		}
		else
		{
			MasterAudio.PlaySound("dungeon_enter");
			MasterAudio.TriggerPlaylistClip("ambient", "dungeon_loop");
			SmartAudioEngine.me.PlayOvrMusic("dungeon");
			EnvironmentEngine.me.SetEngineGlobalState(EnvironmentEngine.State.Inside);
			if (preset.environment_preset != null)
			{
				EnvironmentEngine.me.ApplyEnvironmentPreset(preset.environment_preset);
			}
			dungeon_root.DrawTexture(preset);
			player.transform.position = dungeon_root.enter_to_dunge.transform.position;
			CameraTools.MoveToPos(player.transform.position);
		}
		return preset;
	}

	public DungeonPreset TeleportToDungeonLevelCustom(int level, bool is_without_sound = true)
	{
		DungeonPreset preset = Resources.Load<DungeonPreset>("Dungeon/DungeonPresets/" + level);
		if (preset == null)
		{
			Debug.LogError("Dungeon preset, level = " + level + " not found.");
			me.player.Say("No more levels in alpha.", null, null, SpeechBubbleGUI.SpeechBubbleType.InfoBox);
			return null;
		}
		Stats.DesignEvent("Dungeon:" + preset.dungeon_level + ":Load");
		if (dungeon_root.dungeon_is_loaded_now)
		{
			GJTimer.AddTimer(0.01f, delegate
			{
				if (dungeon_root.TrySaveDungeon())
				{
					Debug.Log("Successfully saved dungeon.");
				}
				dungeon_root.DestroyTiles();
				dungeon_root.DrawTexture(preset);
				player.transform.position = dungeon_root.enter_to_dunge.transform.position;
				CameraTools.MoveToPos(player.transform.position);
			});
		}
		else
		{
			if (!is_without_sound)
			{
				MasterAudio.PlaySound("dungeon_enter");
				MasterAudio.TriggerPlaylistClip("ambient", "dungeon_loop");
				SmartAudioEngine.me.PlayOvrMusic("dungeon");
			}
			EnvironmentEngine.me.SetEngineGlobalState(EnvironmentEngine.State.Inside);
			if (preset.environment_preset != null)
			{
				EnvironmentEngine.me.ApplyEnvironmentPreset(preset.environment_preset);
			}
			dungeon_root.DrawTexture(preset);
			player.transform.position = dungeon_root.enter_to_dunge.transform.position;
			CameraTools.MoveToPos(player.transform.position);
		}
		return preset;
	}

	public void OnExitDungeon()
	{
		MasterAudio.PlaySound("door");
		MasterAudio.StopPlaylist("ambient");
		SmartAudioEngine.me.StopOvrMusic("dungeon", force_immediate: true);
		EnvironmentEngine.me.SetEngineGlobalState(EnvironmentEngine.State.Inside);
		string text = "mortuary";
		Debug.Log("ApplyCurrentEnvironmentPreset, id = " + text);
		EnvironmentPreset preset = EnvironmentPreset.Load(text);
		EnvironmentEngine.me.ApplyEnvironmentPreset(preset);
	}

	public void OpenBuildObjectGUI(WorldGameObject build_desk)
	{
		if (player.GetParamInt("in_tutorial") == 1 && player.GetParamInt("tut_shown_tut_1") == 0)
		{
			player.Say("cant_do_it_now");
			return;
		}
		BuildModeLogics.last_build_desk = build_desk;
		string id = build_desk.obj_def.id;
		CraftsInventory craftsInventory = new CraftsInventory();
		foreach (ObjectCraftDefinition craft_obj_datum in GameBalance.me.craft_obj_data)
		{
			if (craft_obj_datum.builder_ids.Contains(id) && save.IsCraftVisible(craft_obj_datum))
			{
				craftsInventory.AddCraft(craft_obj_datum.id);
			}
		}
		build_mode_logics.SetCurrentBuildZone(build_desk.obj_def.zone_id);
		gui_elements.craft.OpenAsBuild(build_desk, craftsInventory);
		paused = true;
	}

	public void OnEquippedToolBroken(Item equipped_tool)
	{
		Debug.Log("OnEquippedToolBroken : " + equipped_tool.id);
		me.player.UnEquipItem(equipped_tool);
	}

	public void OnGameStartedPlaying()
	{
		float f = Time.realtimeSinceStartup - _start_time;
		Debug.Log("<color=green>OnGameStartedPlaying</color>, time elapsed = " + Mathf.Round(f));
	}

	private static void PreloadResources()
	{
		Debug.Log("SmartResourceHelper.PreloadResources");
		ResourceFileList resourceFileList = Resources.Load<ResourceFileList>("res_scripts");
		SmartResourceHelper.me.LoadListAsync<FlowScript>(resourceFileList.files);
		ResourceFileList resourceFileList2 = Resources.Load<ResourceFileList>("res_wops");
		SmartResourceHelper.me.LoadListAsync<MonoBehaviour>(resourceFileList2.files);
	}

	private void LateUpdate()
	{
	}

	private void EnableHalfResolutionMode()
	{
		RenderTexture renderTexture = new RenderTexture(Screen.width / 2, Screen.height / 2, 0, RenderTextureFormat.Default);
		renderTexture.filterMode = FilterMode.Point;
		GUIElements.me.half_resolution_camera.gameObject.SetActive(value: true);
		GUIElements.me.half_resolution_mesh.sharedMaterial.mainTexture = renderTexture;
		GetComponent<Camera>().targetTexture = renderTexture;
		GetComponent<Camera>().orthographicSize = renderTexture.height;
		GetComponent<ProCamera2DPixelPerfect>().enabled = false;
	}

	public static void SetPausedMode(bool is_paused)
	{
		if (paused != is_paused)
		{
			Debug.Log("SetPausedMode: " + is_paused);
			paused = is_paused;
		}
	}

	private void ShowDLCPopUpWindowIfNeed()
	{
		bool flag = !GameSettings.me.is_stranger_sins_popup_window_shown && DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Stories);
		bool flag2 = !GameSettings.me.is_refugees_popup_window_shown && DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Refugees);
		bool flag3 = !GameSettings.me.is_souls_popup_window_shown && DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Souls);
		if (flag)
		{
			GameSettings.me.is_stranger_sins_popup_window_shown = true;
		}
		if (flag2)
		{
			GameSettings.me.is_refugees_popup_window_shown = true;
		}
		if (flag3)
		{
			GameSettings.me.is_souls_popup_window_shown = true;
		}
		if (flag2 || flag3 || flag)
		{
			GameSettings.Save();
		}
		if (flag && flag2 && flag3)
		{
			GUIElements.me.dialog.OpenOK("stranger_sins_popup_window", null, "game_of_crone_popup_window", separate_with_stars: true, "better_save_soul_popup_window");
		}
		else if (flag && flag2)
		{
			GUIElements.me.dialog.OpenOK("stranger_sins_popup_window", null, "game_of_crone_popup_window", separate_with_stars: true);
		}
		else if (flag && flag3)
		{
			GUIElements.me.dialog.OpenOK("stranger_sins_popup_window", null, "better_save_soul_popup_window", separate_with_stars: true);
		}
		else if (flag2 && flag3)
		{
			GUIElements.me.dialog.OpenOK("game_of_crone_popup_window", null, "better_save_soul_popup_window", separate_with_stars: true);
		}
		else if (flag)
		{
			GUIElements.me.dialog.OpenOK("stranger_sins_popup_window");
		}
		else if (flag2)
		{
			GUIElements.me.dialog.OpenOK("game_of_crone_popup_window");
		}
		else if (flag3)
		{
			GUIElements.me.dialog.OpenOK("better_save_soul_popup_window");
		}
	}
}
