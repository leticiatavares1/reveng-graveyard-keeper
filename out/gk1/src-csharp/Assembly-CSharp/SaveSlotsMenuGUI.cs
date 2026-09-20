using System.Collections.Generic;
using DG.Tweening;
using DLCRefugees;
using DarkTonic.MasterAudio;
using NodeCanvas.Framework;
using SmartPools;
using UnityEngine;

public class SaveSlotsMenuGUI : BaseMenuGUI
{
	private SaveSlotGUI _save_slot_prefab;

	private UIScrollView _scroll_view;

	private SimpleUITable _slots_table;

	private List<SaveSlotGUI> _slots = new List<SaveSlotGUI>();

	private List<SaveSlotData> _slot_datas = new List<SaveSlotData>();

	private SaveSlotData _gamepad_focused_slot;

	public override void Init()
	{
		_save_slot_prefab = GetComponentInChildren<SaveSlotGUI>(includeInactive: true);
		_save_slot_prefab.InitPrefab(this);
		_scroll_view = GetComponentInChildren<UIScrollView>(includeInactive: true);
		_slots_table = GetComponentInChildren<SimpleUITable>(includeInactive: true);
		_slots_table.Reposition();
		_scroll_view.ResetPosition();
		base.Init();
	}

	public override void Open()
	{
		_gamepad_focused_slot = null;
		base.Open();
		GUIElements.me.main_menu.Hide();
		base.button_tips.Clear();
		OnSlotsLoaded(new List<SaveSlotData>());
		PlatformSpecific.ReadSaveSlots(OnSlotsLoaded);
	}

	private new void LateUpdate()
	{
		if (DOTween.IsTweening(_scroll_view.transform) && _scroll_view.RestrictWithinBounds(instant: false))
		{
			_scroll_view.transform.DOKill();
		}
	}

	private void OnSlotsLoaded(List<SaveSlotData> slots)
	{
		Debug.Log("loaded slots count: " + slots.Count);
		RedrawSlots(slots, focus_on_first: true);
	}

	private void RedrawSlots(List<SaveSlotData> slot_datas, bool focus_on_first = false)
	{
		Clear();
		List<SaveSlotData> list = new List<SaveSlotData>();
		foreach (SaveSlotData slot_data in slot_datas)
		{
			if (slot_data.game_time.EqualsTo(0f))
			{
				list.Add(slot_data);
			}
		}
		foreach (SaveSlotData item in list)
		{
			slot_datas.Remove(item);
			PlatformSpecific.DeleteSlot(item, delegate
			{
			});
		}
		_slot_datas = slot_datas;
		SaveSlotGUI saveSlotGUI = _save_slot_prefab.Copy();
		saveSlotGUI.Show(null);
		_slots.Add(saveSlotGUI);
		foreach (SaveSlotData slot_data2 in slot_datas)
		{
			SaveSlotGUI saveSlotGUI2 = _save_slot_prefab.Copy();
			saveSlotGUI2.Show(slot_data2);
			_slots.Add(saveSlotGUI2);
		}
		_slots_table.Reposition();
		_scroll_view.ResetPosition();
		if (BaseGUI.for_gamepad && base.is_shown)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
			if (focus_on_first)
			{
				base.gamepad_controller.FocusOnFirstActive();
			}
		}
	}

	private void Clear()
	{
		foreach (SaveSlotGUI slot in _slots)
		{
			slot.Deactivate();
			slot.DestroyGO();
		}
		_slots.Clear();
		_scroll_view.transform.DOKill();
		_scroll_view.ResetPosition();
	}

	public void OnSlotGamepadOvered(SaveSlotData slot_data, SaveSlotGUI slot_gui)
	{
		if (BaseGUI.for_gamepad)
		{
			_gamepad_focused_slot = slot_data;
			List<GameKeyTip> list = new List<GameKeyTip>();
			list.Add(GameKeyTip.Select());
			if (slot_data != null)
			{
				list.Add(GameKeyTip.Option2("delete"));
			}
			list.Add(GameKeyTip.Close());
			base.button_tips.Print(list);
		}
	}

	public void OnSelectSlotPressed(SaveSlotData slot)
	{
		Debug.Log("OnSelectSlotPressed, null = " + (slot == null));
		Hide();
		if (slot == null)
		{
			LoadingGUI.Show(delegate
			{
				LoadingGUI.ShowBlackBackground(vis: true);
				HideWithMainMenu();
				GUIElements.me.hud.Hide();
				OnNewGame(StartPlayingGame);
			});
			LoadingGUI.LinkAsyncProcess(null);
			LoadingGUI.ShowProgressBar();
		}
		else
		{
			LoadingGUI.Show(delegate
			{
				LoadingGUI.ShowBlackBackground(vis: true);
				OnLoadSlot(slot, StartPlayingGame);
			});
			LoadingGUI.LinkAsyncProcess(null);
			LoadingGUI.ShowProgressBar();
		}
	}

	public static void PrepareScene()
	{
		Debug.Log("PrepareScene");
		CraftComponent.ClearCraftsListOnGameStart();
		MainGame.game_started = false;
	}

	public void StartPlayingGame()
	{
		Debug.Log("Loading: StartPlayingGame");
		LoadingGUI.SetProgressBar(0.40000004f);
		if (MainGame.me.save.unique_id_iterator == -1)
		{
			MainGame.me.save.unique_id_iterator = 500000L;
		}
		UniqueID.SetIterator(MainGame.me.save.unique_id_iterator);
		MainGame.SetPausedMode(is_paused: false);
		MainGame.game_started = false;
		GUIElements.me.hud_enabled = true;
		GUIElements.me.hud.gameObject.GetComponent<UIPanel>().alpha = 1f;
		EnvironmentEngine.me.Init();
		KickComponent.ResetAtGameStart();
		ChunkManager.ClearChunksList();
		MainGame.me.world.FindAndRemovePlayerPrefab();
		MainGame.me.save.dungeons.SetGlobalSeed(MainGame.me.save.dungeon_seed);
		GUIElements.me.quest_list.ResetAtGameStart();
		MainGame.me.save.known_npcs.GetOrCreateNPC("player");
		MimicAnimationController.Init();
		Graph.globally_enabled = true;
		EasySpriteCollectionManager.EnsureAllAtlasesLoaded(delegate
		{
			LoadingGUI.SetProgressBar(0.45000002f);
			GJTimer.AddTimer(0f, delegate
			{
				Debug.Log("Loading: StartPlayingGame 2");
				LoadingGUI.SetProgressBar(0.5f);
				GJTimer.AddTimer(0f, delegate
				{
					GameAwakenerEngine.ScanMap();
					LoadingGUI.SetProgressBar(0.6f);
					GameAwakenerEngine.PreWarm();
					GJTimer.AddConditionalChecker(() => GameAwakenerEngine.prewarm_finished, delegate
					{
						if (GameAwakenerEngine.was_objects_to_prewarm != 0)
						{
							float num = 1f - (float)GameAwakenerEngine.left_objects_to_prewarm / (float)GameAwakenerEngine.was_objects_to_prewarm;
							LoadingGUI.SetProgressBar(0.6f + 0.19999999f * num);
						}
					}, delegate
					{
						LoadingGUI.SetProgressBar(0.8f);
						GJTimer.AddTimer(0f, delegate
						{
							AStarTools.InitialAstarScan();
							LoadingGUI.IncreaseProgressBar();
							GJTimer.AddTimer(0f, delegate
							{
								AstarPath.active.FlushGraphUpdates();
								DropsList.me.RemoveAllDropsFromTheScene();
								WorldMap.RescanGDPoints();
								GDPoint.RestoreGDPointsState();
								MainGame.me.save.PrepareAfterLoad();
								MainGame.me.world_root.gameObject.SetActive(value: true);
								WorldZone.InitZonesSystem();
								ChunkManager.RescanAllObjects();
								WorldGameObject.InitAllWorldWGOs();
								WorldZone.RecalculateAllZones();
								PlayerComponent.SpawnPlayer(is_local_player: true, MainGame.me.save.GetSavedPlayerInventory());
								MainGame.me.save.quests.InitQuestSystem();
								MainGame.me.save.ApplyCurrentEnvironmentPreset();
								HideWithMainMenu();
								GUIElements.me.hud.Hide();
								MainGame.game_started = true;
								WorldMap.RescanWGOsList();
								WorldMap.RescanSpawnersList();
								WorldMap.FromGameSave(MainGame.me.save);
								WorldMap.RescanDropItemsList();
								GUIElements.me.quest_list.Redraw();
								LeaveTrailComponent.RemoveAllTrailsFromTheScene();
								ObjectDynamicShadow.InitOnGameStart();
								WorldMap.RestoreBubbles();
								MainGame.game_started = false;
								WorldMap.DeserializeAllLinkedWorkers();
								SaveGameFixer.OnAfterAllInits();
								CameraTools.Init();
								MainGame.me.save.map.DeserializeTechPoints();
								MainGame.me.save.players_tavern_engine.Init();
								RefugeesCampEngine.instance.Init();
								GJTimer.AddTimer(0f, delegate
								{
									Debug.Log("Loading: StartPlayingGame 3");
									LoadingGUI.SetProgressBar(1f);
									LoadingGUI.Hide();
									Intro.ShowIntro(delegate
									{
										Debug.Log("Loading: StartPlayingGame 4");
										MainGame.game_started = true;
										GUIElements.me.hud.Open();
										GUIElements.me.buffs.Redraw();
										if (MainGame.loaded_from_scene_main)
										{
											string text = "gd_player_respawn";
											if (MainGame.me.player.GetParamInt("sleep_bed_number") > 0)
											{
												text = text + "_" + MainGame.me.player.GetParamInt("sleep_bed_number");
											}
											MainGame.me.player.TeleportToGDPoint(text);
										}
										GameAwakenerEngine.StartRestoringSimplifiedObjects();
										GJTimer.AddTimer(0.05f, delegate
										{
											bool flag = true;
											MasterAudio.StopAllPlaylists();
											MasterAudio.TriggerPlaylistClip("music", "main");
											if (flag)
											{
												MainGame.me.save.quests.CheckKeyQuests("game_start");
											}
											MainGame.me.save.game_logics.Init();
											MainGame.me.player.data.SetParam("speed", LazyConsts.PLAYER_SPEED);
											MainGame.me.OnGameStartedPlaying();
											SmartPooler.PausePool<WorldGameObject>();
											PlatformSpecific.SetGameStatus(GameEvents.GameStatus.InGame);
											MixerLightIntegration.OnStartPlayingGame();
											FlowScriptEngine.StartAllBehaviours();
											GJTimer.AddTimer(0.1f, delegate
											{
												LoadingGUI.ShowBlackBackground(vis: false, animated: true);
											});
											MainGame.me.save.LateSaveFixer();
											MainGame.me.save.GlobalEventsCheck();
										});
									});
								});
							});
						});
					});
				});
			});
		});
	}

	public void StopPlayingGame()
	{
		Debug.Log("StopPlayingGame");
		GUIElements.me.tutorial_arrow.AttachToWGO(null);
		GUIElements.me.overhead.SetActive(is_active: false);
		MainGame.game_started = false;
		Graph.globally_enabled = false;
		GameAwakenerEngine.Stop();
		MultiAnswerGUI.HideAnyctive();
		GUIElements.me.buffs.Redraw();
		ItemsDurabilityManager.Stop();
		DropResHint.DestroyAll();
		InteractionBubbleGUI.DestroyAll();
		DropsList.me.RemoveAllDropsFromTheScene();
		TechPointsDrop.DestroyAllTechspointsBeforeGameExit();
		EnvironmentEngine.me.ResetStates();
		SmartPooler.ResumePool<WorldGameObject>();
		TechPointDrop.DestroyAll();
		MixerLightIntegration.OnStopPlayingGame();
		FlowScriptEngine.StopAllBehaviours();
		SubsceneLoadManager.UnloadAllScenes();
		SmartAudioEngine.me.StopAllSmartSounds();
	}

	public override void Hide(bool play_sound = true)
	{
		_scroll_view.StopScrolling();
		Clear();
		base.Hide(play_sound);
	}

	public override void OnClosePressed()
	{
		base.OnClosePressed();
		GUIElements.me.hud.Hide();
		GUIElements.me.main_menu.Open(switch_music: false);
	}

	private void HideWithMainMenu()
	{
		Hide(play_sound: false);
		GUIElements.me.main_menu.Hide(play_sound: false);
	}

	public void OnDeleteSlotPressed(SaveSlotData slot)
	{
		if (slot == null)
		{
			return;
		}
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.Disable();
		}
		GUIElements.me.dialog.OpenYesNo(GJL.L("delete_slot"), delegate
		{
			PlatformSpecific.DeleteSlot(slot, delegate
			{
				_slot_datas.Remove(slot);
				RedrawSlots(_slot_datas, focus_on_first: true);
			});
		}, null, delegate
		{
			if (BaseGUI.for_gamepad)
			{
				base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
			}
		});
	}

	protected override bool OnPressedSelect()
	{
		if (BaseGUI.for_gamepad)
		{
			OnSelectSlotPressed(_gamepad_focused_slot);
		}
		return BaseGUI.for_gamepad;
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		GUIElements.me.main_menu.Open();
		return BaseGUI.for_gamepad;
	}

	protected override bool OnPressedOption2()
	{
		if (BaseGUI.for_gamepad)
		{
			OnDeleteSlotPressed(_gamepad_focused_slot);
		}
		return BaseGUI.for_gamepad;
	}

	public void OnSaveSlot()
	{
		PlatformSpecific.SaveGame(null, MainGame.me.save, delegate
		{
			PlatformSpecific.ReadSaveSlots(OnSlotsLoaded);
		});
	}

	private void OnNewGame(GJCommons.VoidDelegate on_loaded)
	{
		GameSave.CreateNewSave(delegate
		{
			MainGame.me.player = null;
			Intro.need_show_first_intro = true;
			PlatformSpecific.SaveGame(null, MainGame.me.save, delegate(SaveSlotData slot)
			{
				MainGame.me.save_slot = slot;
				GUIElements.me.hud.Hide();
				on_loaded();
			});
		});
	}

	private void OnLoadSlot(SaveSlotData slot, GJCommons.VoidDelegate on_loaded)
	{
		Intro.need_show_first_intro = false;
		PlatformSpecific.LoadGame(slot, delegate(GameSave save)
		{
			if (save.unlocked_phrases.Contains("@cognac_about_1") && !DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Stories))
			{
				Hide();
				LoadingGUI.Hide();
				LoadingGUI.ShowBlackBackground(vis: false);
				GUIElements.me.dialog.OpenOK("you_need_dlc_stories", delegate
				{
					GUIElements.me.main_menu.Open();
				});
			}
			else if (save.unlocked_phrases.Contains("@zone_refugees_camp_tp") && !DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Refugees))
			{
				Hide();
				LoadingGUI.Hide();
				LoadingGUI.ShowBlackBackground(vis: false);
				GUIElements.me.dialog.OpenOK("you_need_dlc_stories", delegate
				{
					GUIElements.me.main_menu.Open();
				});
			}
			else if (save.unlocked_phrases.Contains("@souls_s_s4_ask_1") && !DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Souls))
			{
				Hide();
				LoadingGUI.Hide();
				LoadingGUI.ShowBlackBackground(vis: false);
				GUIElements.me.dialog.OpenOK("you_need_dlc_stories", delegate
				{
					GUIElements.me.main_menu.Open();
				});
			}
			else
			{
				MainGame.me.save = save;
				GJTimer.AddTimer(0f, delegate
				{
					LoadingGUI.IncreaseProgressBar();
					MainGame.me.save.map.RestoreScene();
					GJTimer.AddTimer(0f, delegate
					{
						LoadingGUI.IncreaseProgressBar();
						on_loaded();
					});
				});
			}
		});
	}

	private void OnDeleteSlot(SaveSlotData slot, GJCommons.VoidDelegate on_deleted)
	{
		PlatformSpecific.DeleteSlot(slot, delegate
		{
			on_deleted();
		});
	}
}
