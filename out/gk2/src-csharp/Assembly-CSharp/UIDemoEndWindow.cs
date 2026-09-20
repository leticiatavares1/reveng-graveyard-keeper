using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDemoEndWindow : LazyWindow<LazyWidgetDataBase>
{
	private const string DEMO_COMPLETED_SAVE_ENVIRONMENT_PRESET = "indoor";

	[SerializeField]
	private RectTransform movable;

	[SerializeField]
	private RectTransform[] lineTargets;

	[SerializeField]
	private LazyButton wishList;

	[SerializeField]
	private LazyButton menuButton;

	[SerializeField]
	private LazyButton leftArrow;

	[SerializeField]
	private LazyButton rightArrow;

	[SerializeField]
	private float moveTime = 0.5f;

	[SerializeField]
	private float timeBetweenAutoscrolls = 3f;

	[SerializeField]
	private LazyButtonTipsStr bigSizeTips;

	[SerializeField]
	private LazyButtonTipsStr smallSizeTips;

	[SerializeField]
	private string demoCompletedSaveGdPointId;

	[SerializeField]
	private Image skinImage;

	[SerializeField]
	private Sprite skinPC;

	[SerializeField]
	private Sprite skinPlaystation;

	[SerializeField]
	private Sprite skinXbox;

	[SerializeField]
	private Sprite skinSwitch;

	[SerializeField]
	private List<TextMeshProUGUI> hints;

	[SerializeField]
	private TextMeshProUGUI storeLabel;

	private int currentLine;

	private bool isAutoscrollActive = true;

	private float lastAutoscrollTime;

	private int autoScrollDirection = 1;

	private int linePreviewDirection = 1;

	private readonly List<RectTransform> activeLineTargets = new List<RectTransform>();

	public override void Init()
	{
		base.Init();
		leftArrow.onClick.AddListener(OnLeftArrowPressed);
		rightArrow.onClick.AddListener(OnRightArrowPressed);
		menuButton.onClick.AddListener(OnMenuPressed);
		wishList.onClick.AddListener(OnWishListPressed);
		menuButton.SetCallbacksIntoGamepadNavigationItem();
		wishList.SetCallbacksIntoGamepadNavigationItem();
	}

	public override void Open(LazyWidgetDataBase data)
	{
		base.Open(data);
		foreach (TextMeshProUGUI hint in hints)
		{
			hint.text = ControllerIconLibrary.GetIconId(GameKey.Action, null, trailingSpace: false);
		}
		UpdateStoreLabel();
		isAutoscrollActive = true;
		lastAutoscrollTime = Time.time;
		currentLine = 1;
		autoScrollDirection = 1;
		linePreviewDirection = autoScrollDirection;
		RefreshLineTargets();
		if (!HasLineTargets())
		{
			Debug.LogError("UIDemoEndWindow has no line targets");
			UpdateArrowsState();
			return;
		}
		skinImage.sprite = skinPC;
		UpdateVisibleLineTargetsForCurrentLine();
		movable.anchoredPosition = GetCurrentLineTarget().anchoredPosition;
		UpdateArrowsState();
		if (LazyInput.IsGamepadActive)
		{
			PrintTips();
		}
	}

	private void OnLeftArrowPressed()
	{
		if (leftArrow.interactable)
		{
			isAutoscrollActive = false;
			MoveBy(-1);
		}
	}

	private bool OnRightArrowPressedGamepad()
	{
		OnRightArrowPressed();
		return true;
	}

	private bool OnLeftArrowPressedGamepad()
	{
		OnLeftArrowPressed();
		return true;
	}

	private void OnRightArrowPressed()
	{
		if (rightArrow.interactable)
		{
			isAutoscrollActive = false;
			MoveBy(1);
		}
	}

	private void MoveBy(int direction)
	{
		RefreshLineTargets();
		if (HasLineTargets())
		{
			int firstLine = currentLine;
			int num = Mathf.Clamp(currentLine + direction, 1, activeLineTargets.Count);
			if (num != currentLine)
			{
				linePreviewDirection = direction;
				SetVisibleLineTargets(firstLine, num);
				currentLine = num;
				MoveTo(GetCurrentLineTarget());
			}
		}
	}

	private void MoveTo(RectTransform target)
	{
		UpdateArrowsState(isInteractable: false);
		TweenerCore<Vector2, Vector2, VectorOptions> tweenerCore = movable.DOAnchorPos(target.anchoredPosition, moveTime);
		tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)delegate
		{
			UpdateVisibleLineTargetsForCurrentLine();
			UpdateArrowsState();
		});
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			PrintTips();
		}
	}

	private void UpdateArrowsState(bool isInteractable = true)
	{
		leftArrow.gameObject.SetActive(value: true);
		rightArrow.gameObject.SetActive(value: true);
		leftArrow.interactable = isInteractable && currentLine > 1;
		rightArrow.interactable = isInteractable && currentLine < activeLineTargets.Count;
		if (LazyInput.IsGamepadActive)
		{
			PrintTips();
		}
	}

	private void OnMenuPressed()
	{
		SaveDemoCompletedAndGoToMenu();
	}

	private void GoToMenu()
	{
		Close();
		MainGame.Instance.GoToMenu(delegate
		{
			MainGame.PlayerController.MovementComponent.ForceStop();
			MainGame.PlayerController.PlayerLocalAreaMovement.StopMovement(force: true);
			LazyUI.Get<UICinematic>()?.DisableCinematic(null, instant: true);
			UIMultiAnswer.ForceDisableAll();
		});
	}

	private void SaveDemoCompletedAndGoToMenu()
	{
		if (!TryGetDemoCompletedSaveGdPoint(out var _))
		{
			GoToMenu();
			return;
		}
		SaveSlotData slotDataForDemoCompletedSave = GetSlotDataForDemoCompletedSave();
		GameSave gameSave = MainGame.Instance.GameSave;
		if (slotDataForDemoCompletedSave == null || gameSave == null)
		{
			Debug.LogError("Can't create completed demo save");
			GoToMenu();
		}
		else
		{
			SaveSystem.Save(slotDataForDemoCompletedSave, gameSave, GoToMenu, GoToMenu, autoOnSaveStart: true, ApplyDemoCompletedSaveData);
		}
	}

	private SaveSlotData GetSlotDataForDemoCompletedSave()
	{
		if (SaveSystem.IsLimitedSaveSlotsEnabled)
		{
			List<SaveSlotData> limitedSaveSlotsData = SaveSystem.GetLimitedSaveSlotsData();
			for (int i = 0; i < limitedSaveSlotsData.Count; i++)
			{
				if (limitedSaveSlotsData[i] == null)
				{
					return new SaveSlotData
					{
						slotName = SaveSystem.GetNameForLimitedSaveSlot(i + 1)
					};
				}
			}
		}
		SaveSlotData saveSlotData = MainGame.Instance.SaveSlotData?.Copy();
		if (saveSlotData == null)
		{
			saveSlotData = new SaveSlotData
			{
				slotName = SaveSystem.GetNameForNewSlot(SaveSystem.SaveSlotDataList)
			};
		}
		return saveSlotData;
	}

	private void ApplyDemoCompletedSaveData(SaveSlotData slotData, GameSave gameSave)
	{
		if (TryGetDemoCompletedSaveGdPoint(out var gdPointData))
		{
			gameSave.WorldData.GetWgoData("npc_workshop_foreman")?.MovementComponent.ForceStop();
			gameSave.playerData.position.Value = gdPointData.Position;
			gameSave.playerData.Direction = gdPointData.Direction.ConvertToVector2XZ();
			gameSave.playerData.currentGameSceneId = gdPointData.GameSceneDataId;
			gameSave.environmentData.timeOfDayPresetName = "indoor";
			WgoData wgoDataByCustomTag = gameSave.WorldData.GetWgoDataByCustomTag("npc_donkey_demo");
			if (wgoDataByCustomTag != null)
			{
				gameSave.WorldData.RemoveWgoDataFromGameScene(wgoDataByCustomTag);
			}
			slotData.repValue = true;
			ApplyCustomDemoCompletedSaveChanges(gameSave, slotData);
		}
	}

	protected virtual void ApplyCustomDemoCompletedSaveChanges(GameSave gameSave, SaveSlotData slotData)
	{
	}

	private bool TryGetDemoCompletedSaveGdPoint(out GDPointData gdPointData)
	{
		gdPointData = null;
		if (string.IsNullOrEmpty(demoCompletedSaveGdPointId))
		{
			Debug.LogError("Can't create completed demo save. GD point id is empty");
			return false;
		}
		gdPointData = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(demoCompletedSaveGdPointId);
		if (gdPointData == null)
		{
			Debug.LogError("Can't create completed demo save. GD point [" + demoCompletedSaveGdPointId + "] not found");
			return false;
		}
		return true;
	}

	public void OnWishListPressed()
	{
		OpenFullGameInStore();
	}

	public static void OpenFullGameInStore()
	{
		LazyAPI.Platform.OpenProductInStore(new StoreProductInfo
		{
			steamUrl = "steam://advertise/4358690",
			epicGamesUrl = "",
			gogUrl = "",
			xboxProductId = "9PFZQ5GNM8TJ",
			nintendoApplicationId = "0100005027a18000",
			nintendo2ApplicationId = "0400543027ffc000",
			ps4ProductLabel = "GRAVEYARDKEEPER2",
			ps5ProductLabel = "GRAVEYARDKEEPER2"
		});
	}

	public void UpdateStoreLabel()
	{
		if (!(storeLabel == null))
		{
			storeLabel.text = LLBase.L(GetStoreLocaleId());
		}
	}

	private string GetStoreLocaleId()
	{
		return "store_name_steam";
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.SetFocusedItem(wishList.GetComponent<GamepadNavigationItem>());
		}
	}

	protected override void PrintTips()
	{
		if (LazyInput.IsGamepadActive)
		{
			List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
			list.Add(LazyGameKeyTip.Select());
			list.Add(new LazyGameKeyTip(GameKey.DecSlider, "tip_prev", leftArrow.interactable));
			list.Add(new LazyGameKeyTip(GameKey.IncSlider, "tip_next", rightArrow.interactable));
			LazyButtonTipsStr lazyButtonTipsStr;
			if (GUIElements.Instance.UIWindowSizeType == UIWindowSizeType.Big)
			{
				bigSizeTips.gameObject.SetActive(value: true);
				smallSizeTips.gameObject.SetActive(value: false);
				lazyButtonTipsStr = bigSizeTips;
			}
			else
			{
				bigSizeTips.gameObject.SetActive(value: false);
				smallSizeTips.gameObject.SetActive(value: true);
				lazyButtonTipsStr = smallSizeTips;
			}
			lazyButtonTipsStr.Print(list, (GUIElements.Instance.UIWindowSizeType == UIWindowSizeType.Small) ? "\n" : "  ");
		}
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Right, OnRightArrowPressedGamepad);
		gameKeyDelegates.Add(GameKey.DpadRight, OnRightArrowPressedGamepad);
		gameKeyDelegates.Add(GameKey.Left, OnLeftArrowPressedGamepad);
		gameKeyDelegates.Add(GameKey.DpadLeft, OnLeftArrowPressedGamepad);
		return gameKeyDelegates;
	}

	protected override void Update()
	{
		base.Update();
		if (!isAutoscrollActive || !(Time.time - timeBetweenAutoscrolls >= lastAutoscrollTime))
		{
			return;
		}
		lastAutoscrollTime = Time.time;
		if (autoScrollDirection == 1)
		{
			RefreshLineTargets();
			if (currentLine < activeLineTargets.Count)
			{
				MoveBy(1);
				if (currentLine == activeLineTargets.Count)
				{
					autoScrollDirection = -1;
				}
			}
		}
		else if (currentLine > 1)
		{
			MoveBy(-1);
			if (currentLine == 1)
			{
				autoScrollDirection = 1;
			}
		}
		if (LazyInput.IsGamepadActive)
		{
			PrintTips();
		}
	}

	private void RefreshLineTargets()
	{
		activeLineTargets.Clear();
		if (lineTargets != null && lineTargets.Length != 0)
		{
			for (int i = 0; i < lineTargets.Length; i++)
			{
				AddLineTarget(lineTargets[i]);
			}
		}
		currentLine = ((!HasLineTargets()) ? 1 : Mathf.Clamp(currentLine, 1, activeLineTargets.Count));
	}

	private void AddLineTarget(RectTransform target)
	{
		if (target != null)
		{
			activeLineTargets.Add(target);
		}
	}

	private RectTransform GetCurrentLineTarget()
	{
		return activeLineTargets[currentLine - 1];
	}

	private bool HasLineTargets()
	{
		return activeLineTargets.Count > 0;
	}

	private void UpdateVisibleLineTargetsForCurrentLine()
	{
		if (HasLineTargets())
		{
			SetVisibleLineTargets(currentLine, GetPreviewLine());
		}
	}

	private int GetPreviewLine()
	{
		if (activeLineTargets.Count <= 1)
		{
			return -1;
		}
		int num = (isAutoscrollActive ? autoScrollDirection : linePreviewDirection);
		int num2 = currentLine + num;
		if (num2 < 1 || num2 > activeLineTargets.Count)
		{
			num2 = currentLine - num;
		}
		if (num2 != currentLine)
		{
			return num2;
		}
		return -1;
	}

	private void SetVisibleLineTargets(int firstLine, int secondLine = -1)
	{
		for (int i = 0; i < activeLineTargets.Count; i++)
		{
			int num = i + 1;
			activeLineTargets[i].gameObject.SetActive(num == firstLine || num == secondLine);
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		LazyUI.GetWindow<UIDemoEndWindow>().Open(null);
	}
}
