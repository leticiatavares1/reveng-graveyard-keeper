using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UISaveSlotsWindowLimitedPopUp : LazyWindow<UISaveSlotsWindowLimitedPopUpData>
{
	[SerializeField]
	private ScrollRect scroll;

	[SerializeField]
	private UISaveSlot uiSaveSlotPrefab;

	private Stack<UISaveSlot> uiSaveSlotsElementPool = new Stack<UISaveSlot>();

	private List<UISaveSlot> showingSlotsElements = new List<UISaveSlot>();

	private List<SaveSlotData> importableSaveSlots = new List<SaveSlotData>();

	private static void SaveImportLog(string message)
	{
	}

	public override void Init()
	{
		uiSaveSlotPrefab.gameObject.SetActive(value: false);
		UISaveSlot.OnSaveSlotSelectedWithSlot += OnSaveSlotSelectedHandler;
		base.Init();
	}

	public override void Open(UISaveSlotsWindowLimitedPopUpData data)
	{
		base.Open(data);
		SaveImportLog($"Open targetSlotIndex:[{data.TargetSlotIndex}]");
		ReadSlotsAndDisplay();
		if (scroll != null)
		{
			scroll.verticalNormalizedPosition = 1f;
		}
		((RectTransform)base.transform).RefreshContentFitter();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	private void ReadSlotsAndDisplay()
	{
		SaveImportLog("ReadSlotsAndDisplay started");
		importableSaveSlots = SaveSystem.GetImportableSaveSlotsForLimitedSlots();
		SaveImportLog($"ReadSlotsAndDisplay importableCount:[{importableSaveSlots.Count}]");
		foreach (SaveSlotData importableSaveSlot in importableSaveSlots)
		{
			SaveImportLog($"ReadSlotsAndDisplay show importable slotName:[{importableSaveSlot?.slotName}] platform:[{importableSaveSlot?.platform}] isDemoSave:[{importableSaveSlot?.isDemoSave}] canLoad:[{SaveSystem.CanLoadSaveSlot(importableSaveSlot)}] sourceLabel:[{SaveSystem.GetLimitedSaveSlotSourceLabel(importableSaveSlot)}]");
			UISaveSlot uISaveSlotElement = GetUISaveSlotElement();
			showingSlotsElements.Add(uISaveSlotElement);
			uISaveSlotElement.Show(importableSaveSlot, canDelete: false);
			uISaveSlotElement.SetSourceLabel(SaveSystem.GetLimitedSaveSlotSourceLabel(importableSaveSlot));
		}
	}

	private void OnSaveSlotSelectedHandler(UISaveSlot slot, SaveSlotData slotData)
	{
		if (!base.IsShownAndTop || !showingSlotsElements.Contains(slot) || slotData == null)
		{
			SaveImportLog($"OnSaveSlotSelectedHandler ignored IsShownAndTop:[{base.IsShownAndTop}] containsSlot:[{showingSlotsElements.Contains(slot)}] slotDataNull:[{slotData == null}]");
			return;
		}
		SaveImportLog($"OnSaveSlotSelectedHandler selected slotName:[{slotData.slotName}] platform:[{slotData.platform}] isDemoSave:[{slotData.isDemoSave}] targetSlotIndex:[{data.TargetSlotIndex}]");
		UIDialogWindowData uIDialogWindowData = new UIDialogWindowData(LLBase.L("save_import_confirm"), LLBase.L("save_import_confirm_txt"), Yes, LazyUI.GetWindow<UIDialogWindow>().Close);
		uIDialogWindowData.ShowCloseButton = false;
		LazyUI.GetWindow<UIDialogWindow>().Open(uIDialogWindowData);
		void Yes()
		{
			LazyUI.GetWindow<UIDialogWindow>().Close();
			SaveImportLog($"Import confirmed slotName:[{slotData.slotName}] targetSlotIndex:[{data.TargetSlotIndex}]");
			StartCoroutine(ImportAfterOverlayShown(slotData));
		}
	}

	private IEnumerator ImportAfterOverlayShown(SaveSlotData slotData)
	{
		SaveImportLog($"ImportAfterOverlayShown started slotName:[{slotData?.slotName}] targetSlotIndex:[{data.TargetSlotIndex}]");
		SaveSystem.TriggerOnSaveStartEvent(instant: true);
		yield return null;
		SaveSystem.ImportSaveToLimitedSlot(slotData, data.TargetSlotIndex, delegate(bool success)
		{
			SaveImportLog($"ImportSaveToLimitedSlot callback slotName:[{slotData?.slotName}] targetSlotIndex:[{data.TargetSlotIndex}] success:[{success}]");
			if (success)
			{
				Close();
				data.OnImported?.Invoke();
			}
		}, triggerSaveWriteStarted: false);
	}

	private UISaveSlot GetUISaveSlotElement()
	{
		if (uiSaveSlotsElementPool.Count == 0)
		{
			uiSaveSlotsElementPool.Push(uiSaveSlotPrefab.Copy(null, activate: false));
		}
		return uiSaveSlotsElementPool.Pop();
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		lazyButtonTips.Print(LazyGameKeyTip.Select(), LazyGameKeyTip.Back());
	}

	protected override void InitCloseButton(LazyButton button)
	{
		base.InitCloseButton(button);
		button.onClick.AddListener(Close);
	}

	protected override bool OnPressedBack()
	{
		Close();
		return true;
	}

	public override void Hide()
	{
		for (int num = showingSlotsElements.Count - 1; num >= 0; num--)
		{
			showingSlotsElements[num].gameObject.SetActive(value: false);
			uiSaveSlotsElementPool.Push(showingSlotsElements[num]);
		}
		showingSlotsElements.Clear();
		base.Hide();
	}

	protected override void TestDraw()
	{
		Open(new UISaveSlotsWindowLimitedPopUpData(1, null));
	}
}
