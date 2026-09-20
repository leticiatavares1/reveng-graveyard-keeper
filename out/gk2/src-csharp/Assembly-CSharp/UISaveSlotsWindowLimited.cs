using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UISaveSlotsWindowLimited : LazyWindow<LazyWidgetDataBase>
{
	[SerializeField]
	private UISaveSlot[] saveSlots = new UISaveSlot[3];

	[SerializeField]
	private GameKey importSaveGameKey = GameKey.SaveImport;

	[SerializeField]
	private string importSaveTipLocale = "ui_save_import";

	private List<SaveSlotData> saveSlotDataList = new List<SaveSlotData>();

	private UISaveSlot focusedSaveSlot;

	private bool hasImportableSaveSlots;

	private static void SaveImportLog(string message)
	{
	}

	public override void Init()
	{
		UISaveSlot.OnSaveSlotSelectedWithSlot += OnSaveSlotSelectedHandler;
		UISaveSlot.OnSaveSlotDelete += OnSaveSlotDeletedHandler;
		UISaveSlot.OnSaveSlotImport += OnSaveSlotImportHandler;
		UISaveSlot.OnSaveSlotEntered += OnSaveSlotEnteredHandler;
		base.Init();
	}

	public override void Open(LazyWidgetDataBase data)
	{
		base.Open(data);
		SaveImportLog("Open");
		ReadSlotsAndDisplay();
		focusedSaveSlot = GetFirstAvailableSaveSlot();
		SaveImportLog($"Open focusedSaveSlotNull:[{focusedSaveSlot == null}]");
		((RectTransform)base.transform).RefreshContentFitter();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	private void ReadSlotsAndDisplay()
	{
		SaveImportLog("ReadSlotsAndDisplay started");
		saveSlotDataList = SaveSystem.GetLimitedSaveSlotsData();
		hasImportableSaveSlots = SaveSystem.HasImportableSaveSlotsForLimitedSlots();
		SaveImportLog($"ReadSlotsAndDisplay limitedSlotsCount:[{saveSlotDataList?.Count ?? 0}] hasImportableSaveSlots:[{hasImportableSaveSlots}]");
		for (int i = 0; i < saveSlots.Length && i < 3; i++)
		{
			if (saveSlots[i] == null)
			{
				SaveImportLog($"ReadSlotsAndDisplay ui slot[{i}] is null");
				continue;
			}
			SaveSlotData saveSlotData = saveSlotDataList[i];
			SaveImportLog($"ReadSlotsAndDisplay show ui slot[{i}] saveSlotName:[{saveSlotData?.slotName}] platform:[{saveSlotData?.platform}] isDemoSave:[{saveSlotData?.isDemoSave}] canImport:[{hasImportableSaveSlots}]");
			saveSlots[i].Show(saveSlotDataList[i], canDelete: true, hasImportableSaveSlots);
		}
	}

	private void OnSaveSlotSelectedHandler(UISaveSlot slot, SaveSlotData slotData)
	{
		if (!base.IsShownAndTop || !IsLimitedWindowSlot(slot))
		{
			return;
		}
		focusedSaveSlot = slot;
		int slotIndex = GetSlotIndex(slot);
		if (slotIndex < 1)
		{
			return;
		}
		if (slotData == null)
		{
			Close();
			MainGame.Instance.StartNewGameInLimitedSaveSlot(slotIndex);
			return;
		}
		Close();
		LazyTimer.AddTimer(0f, delegate
		{
			UILoadingOverlay overlay = LazyUI.Get<UILoadingOverlay>();
			overlay.Draw(new LoadingWindowData(MainGame.EntrySceneToLoad, delegate
			{
				SaveSystem.Load(slotData, delegate(GameSave save)
				{
					if (save != null)
					{
						MainGame.Instance.ContinueGame(slotData, save);
					}
					else
					{
						overlay.Hide();
						Open(null);
					}
				});
			}));
		});
	}

	private void OnSaveSlotDeletedHandler(UISaveSlot slot, SaveSlotData slotData)
	{
		if (base.IsShownAndTop && IsLimitedWindowSlot(slot) && slotData != null)
		{
			UIDialogWindowData uIDialogWindowData = new UIDialogWindowData(LLBase.L("ui_save_remove_header"), LLBase.L("ui_save_remove_info"), Yes, LazyUI.GetWindow<UIDialogWindow>().Close);
			uIDialogWindowData.ShowCloseButton = false;
			LazyUI.GetWindow<UIDialogWindow>().Open(uIDialogWindowData);
		}
		void Yes()
		{
			LazyUI.GetWindow<UIDialogWindow>().Close();
			if (SaveSystem.Remove(slotData, null))
			{
				int num = GetSlotIndex(slot) - 1;
				if (num >= 0 && num < saveSlotDataList.Count)
				{
					saveSlotDataList[num] = null;
				}
				slot.Show(null, canDelete: true, hasImportableSaveSlots);
				((RectTransform)base.transform).RefreshContentFitter();
				if (LazyInput.IsGamepadActive)
				{
					base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
				}
			}
		}
	}

	private void OnSaveSlotEnteredHandler(UISaveSlot slot)
	{
		if (base.IsShown && IsLimitedWindowSlot(slot))
		{
			focusedSaveSlot = slot;
		}
	}

	private void OnSaveSlotImportHandler(UISaveSlot slot)
	{
		if (!base.IsShownAndTop || !IsLimitedWindowSlot(slot))
		{
			SaveImportLog($"OnSaveSlotImportHandler ignored IsShownAndTop:[{base.IsShownAndTop}] isLimitedWindowSlot:[{IsLimitedWindowSlot(slot)}]");
			return;
		}
		focusedSaveSlot = slot;
		SaveImportLog($"OnSaveSlotImportHandler accepted targetSlotIndex:[{GetSlotIndex(slot)}]");
		OnPressedImportSave();
	}

	private bool OnPressedImportSave()
	{
		if (!base.IsShownAndTop || !hasImportableSaveSlots)
		{
			SaveImportLog($"OnPressedImportSave rejected IsShownAndTop:[{base.IsShownAndTop}] hasImportableSaveSlots:[{hasImportableSaveSlots}]");
			return false;
		}
		UISaveSlot uISaveSlot = ((focusedSaveSlot != null) ? focusedSaveSlot : GetFirstAvailableSaveSlot());
		if (uISaveSlot == null || !uISaveSlot.CanImportSaveSlot())
		{
			SaveImportLog($"OnPressedImportSave rejected targetSlotNull:[{uISaveSlot == null}] targetCanImport:[{uISaveSlot != null && uISaveSlot.CanImportSaveSlot()}]");
			return false;
		}
		int slotIndex = GetSlotIndex(uISaveSlot);
		if (slotIndex < 1)
		{
			SaveImportLog($"OnPressedImportSave rejected invalid targetSlotIndex:[{slotIndex}]");
			return false;
		}
		SaveImportLog($"OnPressedImportSave opening popup targetSlotIndex:[{slotIndex}]");
		LazyUI.GetWindow<UISaveSlotsWindowLimitedPopUp>().Open(new UISaveSlotsWindowLimitedPopUpData(slotIndex, RefreshAfterImport));
		return true;
	}

	private void RefreshAfterImport()
	{
		SaveImportLog("RefreshAfterImport");
		ReadSlotsAndDisplay();
		((RectTransform)base.transform).RefreshContentFitter();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	private UISaveSlot GetFirstAvailableSaveSlot()
	{
		for (int i = 0; i < saveSlots.Length; i++)
		{
			if (saveSlots[i] != null)
			{
				return saveSlots[i];
			}
		}
		return null;
	}

	private int GetSlotIndex(UISaveSlot slot)
	{
		for (int i = 0; i < saveSlots.Length && i < 3; i++)
		{
			if (saveSlots[i] == slot)
			{
				return i + 1;
			}
		}
		return -1;
	}

	private bool IsLimitedWindowSlot(UISaveSlot slot)
	{
		return GetSlotIndex(slot) > 0;
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		base.PrintTips(gamepadNavigationItem);
		if (gamepadNavigationItem != null && gamepadNavigationItem.TryGetComponent<UISaveSlot>(out var component) && IsLimitedWindowSlot(component))
		{
			focusedSaveSlot = component;
			List<LazyGameKeyTip> list = new List<LazyGameKeyTip>
			{
				LazyGameKeyTip.Select(),
				LazyGameKeyTip.Back()
			};
			if (component.CanDeleteSaveSlot())
			{
				list.Add(new LazyGameKeyTip(GameKey.SaveDelete, "ui_save_slot_delete"));
			}
			if (component.CanImportSaveSlot())
			{
				list.Add(new LazyGameKeyTip(importSaveGameKey, importSaveTipLocale));
			}
			lazyButtonTips.Print(list);
		}
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		if (importSaveGameKey != null)
		{
			gameKeyDelegates.TryAdd(importSaveGameKey, OnPressedImportSave);
		}
		return gameKeyDelegates;
	}

	protected override void InitCloseButton(LazyButton button)
	{
		base.InitCloseButton(button);
		button.onClick.AddListener(ReturnToPreviousWindow);
	}

	protected override bool OnPressedBack()
	{
		bool result = base.OnPressedBack();
		ReturnToPreviousWindow();
		return result;
	}

	private void ReturnToPreviousWindow()
	{
		LazyUI.GetWindow<UIMainMenuWindow>().Open(null);
	}

	public override void Hide()
	{
		base.Hide();
	}

	protected override void TestDraw()
	{
		Open(null);
	}
}
