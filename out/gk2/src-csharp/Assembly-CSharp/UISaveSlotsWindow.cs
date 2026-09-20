using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UISaveSlotsWindow : LazyWindow<LazyWidgetDataBase>
{
	[SerializeField]
	private ScrollRect scroll;

	[SerializeField]
	private UISaveSlot uiSaveSlotPrefab;

	private Stack<UISaveSlot> uiSaveSlotsElementPool = new Stack<UISaveSlot>();

	private List<UISaveSlot> showingSlotsElements = new List<UISaveSlot>();

	private UISaveSlot newSaveSlot;

	private List<SaveSlotData> saveSlotDataList;

	private SaveSlotData pickedSaveSlot;

	public override void Init()
	{
		uiSaveSlotPrefab.gameObject.SetActive(value: false);
		newSaveSlot = GetUISaveSlotElement();
		newSaveSlot.Show(null);
		UISaveSlot.OnSaveSlotSelected += OnSaveSlotSelectedHandler;
		UISaveSlot.OnSaveSlotDelete += OnSaveSlotDeletedHandler;
		base.Init();
	}

	public override void Open(LazyWidgetDataBase data)
	{
		base.Open(data);
		ReadSlotsAndDisplay();
		pickedSaveSlot = null;
		scroll.verticalNormalizedPosition = 1f;
		newSaveSlot.gameObject.SetActive(value: true);
		((RectTransform)base.transform).RefreshContentFitter();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	private void ReadSlotsAndDisplay()
	{
		saveSlotDataList = SaveSystem.SaveSlotDataList;
		newSaveSlot.gameObject.SetActive(value: true);
		foreach (SaveSlotData saveSlotData in saveSlotDataList)
		{
			if (!saveSlotData.IsDemoSlotDeleted)
			{
				UISaveSlot uISaveSlotElement = GetUISaveSlotElement();
				showingSlotsElements.Add(uISaveSlotElement);
				uISaveSlotElement.Show(saveSlotData);
			}
		}
		SortSlotsByDateTime();
	}

	private void OnSaveSlotSelectedHandler(SaveSlotData slotData)
	{
		if (!base.IsShownAndTop)
		{
			return;
		}
		if (slotData == null)
		{
			pickedSaveSlot = null;
			Close();
			MainGame.Instance.StartNewGame();
			return;
		}
		Close();
		LazyTimer.AddTimer(0f, delegate
		{
			UILoadingOverlay overlay = LazyUI.Get<UILoadingOverlay>();
			overlay.Draw(new LoadingWindowData(MainGame.EntrySceneToLoad, delegate
			{
				SaveSystem.Load(slotData, delegate(GameSave s)
				{
					if (s != null)
					{
						pickedSaveSlot = slotData;
						MainGame.Instance.ContinueGame(pickedSaveSlot, s);
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
		if (base.IsShownAndTop)
		{
			LazyUI.GetWindow<UIDialogWindow>().Open(new UIDialogWindowData(LLBase.L("ui_save_remove_header"), LLBase.L("ui_save_remove_info"), Yes, LazyUI.GetWindow<UIDialogWindow>().Close)
			{
				ShowCloseButton = false
			});
		}
		void RemoveActions()
		{
			if (!P_0.removeCalled)
			{
				P_0.removeCalled = true;
				slot.gameObject.SetActive(value: false);
				showingSlotsElements.Remove(slot);
				uiSaveSlotsElementPool.Push(slot);
				((RectTransform)base.transform).RefreshContentFitter();
				if (LazyInput.IsGamepadActive)
				{
					base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
				}
			}
		}
		void Yes()
		{
			LazyUI.GetWindow<UIDialogWindow>().Close();
			bool removeCalled = false;
			if (!removeCalled && SaveSystem.Remove(slotData, null))
			{
				RemoveActions();
			}
		}
	}

	private void SortSlotsByDateTime()
	{
		showingSlotsElements.Sort((UISaveSlot x, UISaveSlot y) => DateTime.Compare(y.LinkedSaveSlot.GetSaveDateTime(), x.LinkedSaveSlot.GetSaveDateTime()));
		for (int i = 0; i < showingSlotsElements.Count; i++)
		{
			showingSlotsElements[i].transform.SetSiblingIndex(i);
		}
		newSaveSlot.transform.SetAsFirstSibling();
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
		if (gamepadNavigationItem.TryGetComponent<UISaveSlot>(out var component))
		{
			if (component.CanDeleteSaveSlot())
			{
				lazyButtonTips.Print(LazyGameKeyTip.Select(), LazyGameKeyTip.Back(), new LazyGameKeyTip(GameKey.SaveDelete, "ui_save_slot_delete"));
			}
			else
			{
				lazyButtonTips.Print(LazyGameKeyTip.Select(), LazyGameKeyTip.Back());
			}
		}
		else
		{
			base.PrintTips(gamepadNavigationItem);
		}
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
		for (int num = showingSlotsElements.Count - 1; num >= 0; num--)
		{
			showingSlotsElements[num].gameObject.SetActive(value: false);
			uiSaveSlotsElementPool.Push(showingSlotsElements[num]);
		}
		showingSlotsElements.Clear();
		newSaveSlot.gameObject.SetActive(value: false);
	}

	protected override void TestDraw()
	{
		Open(null);
	}
}
