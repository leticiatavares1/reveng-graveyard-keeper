using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFishingWindow : LazyWindow<UIFishingWindowData>
{
	[SerializeField]
	private FishingSettings fishingSettings;

	[SerializeField]
	private RectTransform baitSelection;

	[SerializeField]
	private GameObject contentParent;

	[SerializeField]
	private TextMeshProUGUI header;

	[SerializeField]
	private LazyButton leftButton;

	[SerializeField]
	private LazyButton rightButton;

	[SerializeField]
	private UIDialogWindowButton submutButton;

	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private RectTransform possibleFishesParent;

	[SerializeField]
	private Vector2 posBig;

	[SerializeField]
	private Vector2 posSmall;

	[SerializeField]
	private GameObject mouseAndKeyboardHints;

	[SerializeField]
	private TextMeshProUGUI hintRodLabel;

	[SerializeField]
	private TextMeshProUGUI hintExitLabel;

	[SerializeField]
	private int selectionPointer;

	private PlayerFishingComponent fishingComponent;

	private List<UIPossibleFishCell> possibleFishes = new List<UIPossibleFishCell>();

	private bool IsFishingMiniGameActive
	{
		get
		{
			if (fishingComponent != null)
			{
				return fishingComponent.IsMiniGameActive;
			}
			return false;
		}
	}

	public bool IsBaitSelectionVisible
	{
		get
		{
			if (baitSelection != null)
			{
				return baitSelection.gameObject.activeInHierarchy;
			}
			return false;
		}
	}

	public void OnDestroy()
	{
		StopAllCoroutines();
	}

	public override void Init()
	{
		base.Init();
		leftButton.onClick.AddListener(delegate
		{
			SelectNextItem(reverseDirection: true);
		});
		rightButton.onClick.AddListener(delegate
		{
			SelectNextItem();
		});
	}

	public override void Open(UIFishingWindowData data)
	{
		if (GUIElements.Instance.UIWindowSizeType == UIWindowSizeType.Big)
		{
			baitSelection.anchoredPosition = posBig;
		}
		else
		{
			baitSelection.anchoredPosition = posSmall;
		}
		selectionPointer = 0;
		baitSelection.gameObject.SetActive(value: true);
		fishingComponent = MainGame.PlayerController.FishingComponent;
		if (fishingComponent != null)
		{
			fishingComponent.StartActivity(data.Reservoir);
		}
		else
		{
			Debug.LogError("[UIFishingWindow] Can't start fishing. No PlayerFishingComponent found");
		}
		for (int i = 0; i < data.ReservoirFishings.Count; i++)
		{
			UIPossibleFishCell elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UIPossibleFishCell>(possibleFishesParent);
			possibleFishes.Add(elementFromPool);
		}
		base.Open(data);
		SetWindowVisible(isVisible: true);
		((RectTransform)base.transform).RefreshContentFitter();
	}

	public override void Close()
	{
		selectionPointer = 0;
		base.Close();
		foreach (UIPossibleFishCell possibleFish in possibleFishes)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(possibleFish);
		}
		possibleFishes.Clear();
	}

	public override void Redraw()
	{
		base.Redraw();
		if (!IsFishingMiniGameActive && baitSelection != null && baitSelection.gameObject.activeInHierarchy && HasSelectableBait())
		{
			RedrawInternal();
		}
	}

	protected override void InitCloseButton(LazyButton button)
	{
		button.onClick.AddListener(delegate
		{
			Close();
			MainGame.PlayerController?.FishingComponent.StopActivity();
		});
	}

	private void RedrawInternal()
	{
		if (!HasSelectableBait())
		{
			return;
		}
		Item currentBait = data.BaitItems[selectionPointer];
		if (currentBait == null || data.ReservoirFishings == null || data.Reservoir?.Data == null)
		{
			return;
		}
		leftButton.interactable = data.BaitItems.Count > 1 && selectionPointer > 0;
		rightButton.interactable = data.BaitItems.Count > 1 && selectionPointer + 1 < data.BaitItems.Count;
		header.text = LLBase.L(currentBait.id);
		int count = currentBait.Count;
		int value = 1;
		if (currentBait.id == "no_bait")
		{
			itemCell.Draw(currentBait);
		}
		else
		{
			itemCell.Draw(new Item(currentBait.id, value), isNeedItem: true, count);
		}
		bool anyCatchableFish = false;
		int num = Mathf.Min(data.ReservoirFishings.Count, possibleFishes.Count);
		for (int i = 0; i < num; i++)
		{
			FishingDef fishingDef = data.ReservoirFishings[i];
			UIPossibleFishCell uIPossibleFishCell = possibleFishes[i];
			bool flag = false;
			for (int j = 0; j < fishingDef.baitMod.List.Count; j++)
			{
				if (fishingDef.baitMod.List[j].type == currentBait.id)
				{
					flag = true;
				}
			}
			if (data.Reservoir.Data.GetGameRes(fishingDef.fishId + "_caught") > 0f)
			{
				if (flag && data.Reservoir.Data.GetGameRes(fishingDef.fishId) > 0f)
				{
					uIPossibleFishCell.Draw(new Item(fishingDef.fishId));
					anyCatchableFish = true;
				}
				else
				{
					uIPossibleFishCell.DrawInactive(new Item(fishingDef.fishId));
				}
			}
			else if (flag && data.Reservoir.Data.GetGameRes(fishingDef.fishId) > 0f)
			{
				uIPossibleFishCell.DrawLocked();
				anyCatchableFish = true;
			}
			else
			{
				uIPossibleFishCell.DrawLockedInactive();
			}
		}
		submutButton.Draw(new UIDialogWindowData.ButtonData(OnSubmitPressed, LLBase.L("ui_submit_bait"), delegate
		{
			if (!anyCatchableFish)
			{
				return false;
			}
			return currentBait.id == "no_bait" || currentBait.Count > 0;
		}, replaceForGamepad: true, GameKey.SubmitBait));
		UpdateGamepadDependentStuff();
	}

	public void SetWindowVisible(bool isVisible)
	{
		contentParent.SetActive(isVisible);
		if (isVisible && LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		UpdateGamepadDependentStuff();
	}

	public void DisplayChoicePanel()
	{
		UpdateGamepadDependentStuff();
		string currentBaitId = GetCurrentBaitId();
		data.UpdateAvailableFishingAndBaits();
		selectionPointer = FindBaitSelectionIndex(currentBaitId);
		baitSelection.gameObject.SetActive(value: true);
		RedrawInternal();
	}

	private string GetCurrentBaitId()
	{
		if (data?.BaitItems == null || selectionPointer < 0 || selectionPointer >= data.BaitItems.Count)
		{
			return null;
		}
		return data.BaitItems[selectionPointer].id;
	}

	private int FindBaitSelectionIndex(string baitId)
	{
		if (string.IsNullOrEmpty(baitId) || data.BaitItems == null)
		{
			return 0;
		}
		for (int i = 0; i < data.BaitItems.Count; i++)
		{
			Item item = data.BaitItems[i];
			if (item.id == baitId && (item.id == "no_bait" || item.Count > 0))
			{
				return i;
			}
		}
		return 0;
	}

	private bool SelectNextItem(bool reverseDirection = false)
	{
		if (baitSelection == null || !baitSelection.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (data?.BaitItems == null)
		{
			Debug.LogError("[UIFishingWindow]: baitItems is null");
			return false;
		}
		if (!CanSelectNextItem(reverseDirection))
		{
			return false;
		}
		if (!reverseDirection)
		{
			selectionPointer++;
		}
		else
		{
			selectionPointer--;
		}
		Redraw();
		UpdateGamepadDependentStuff();
		return true;
	}

	private bool CanSelectNextItem(bool reverseDirection = false)
	{
		if (baitSelection == null || !baitSelection.gameObject.activeInHierarchy || data?.BaitItems == null || data.BaitItems.Count <= 1)
		{
			return false;
		}
		if (!reverseDirection)
		{
			return selectionPointer + 1 < data.BaitItems.Count;
		}
		return selectionPointer > 0;
	}

	private bool HasSelectableBait()
	{
		if (data?.BaitItems != null && selectionPointer >= 0)
		{
			return selectionPointer < data.BaitItems.Count;
		}
		return false;
	}

	private void OnSubmitPressed()
	{
		OnSubmitBait();
	}

	private bool OnSubmitBait()
	{
		if (fishingComponent == null || IsFishingMiniGameActive || !HasSelectableBait() || data?.Reservoir?.Data == null)
		{
			return false;
		}
		Item item = data.BaitItems[selectionPointer];
		List<FishingDef> list = GameBalance.Me.fishingDefs.FindAll((FishingDef x) => x.reservoirId == data.Reservoir.Data.id);
		List<float> list2 = new List<float>();
		if (list.Count == 0)
		{
			Debug.LogError("[UIFishingWindow] Can't start fishing. No FishingDef found for [" + data.Reservoir.Data.id + "] wgoData");
			return false;
		}
		float num = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			float num2 = 0f;
			if (item != null && list[i].baitMod.Get(item.id) != 0f)
			{
				num2 = list[i].baitMod.Get(item.id);
			}
			else if (item == null)
			{
				num2 = list[i].baitMod.Get("no_bait");
			}
			float dayTimeMod = list[i].GetDayTimeMod();
			float num3 = data.Reservoir.Data.GetGameResInt(list[i].fishId);
			float num4 = (float)list[i].baseWeight * num2 * dayTimeMod * num3;
			list2.Add(num4);
			num += num4;
		}
		float num5 = 0f;
		float num6 = UnityEngine.Random.Range(0f, num);
		for (int j = 0; j < list2.Count; j++)
		{
			num5 += list2[j];
			if (num6 <= num5)
			{
				if (item != null && item.id != "no_bait")
				{
					MainGame.PlayerData.inventory.RemoveItemById(item.id, 1);
				}
				baitSelection.gameObject.SetActive(value: false);
				fishingComponent.RunMiniGame(list[j], data.Reservoir.Data, data.FishingRodDef);
				return true;
			}
		}
		Debug.LogError("[UIFishingWindow] Can't start fishing. Can't choose FishingDef");
		return false;
	}

	protected override bool OnPressedBack()
	{
		if (!contentParent.activeSelf)
		{
			return false;
		}
		MainGame.PlayerController?.FishingComponent.StopActivity();
		return true;
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		if (LazyInput.IsGamepadActive)
		{
			mouseAndKeyboardHints.SetActive(value: false);
			if (!IsFishingMiniGameActive)
			{
				List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
				if (CanSelectNextItem(reverseDirection: true))
				{
					list.Add(new LazyGameKeyTip(GameKey.PrevTab, "tip_prev"));
				}
				if (CanSelectNextItem())
				{
					list.Add(new LazyGameKeyTip(GameKey.NextTab, "tip_next"));
				}
				list.Add(LazyGameKeyTip.Back());
				lazyButtonTips.Print(list);
			}
			else
			{
				lazyButtonTips.Print(new LazyGameKeyTip(GameKey.Select, "btn_pull_rod"), new LazyGameKeyTip(GameKey.Back, "ui_menu_exit"));
			}
		}
		else
		{
			lazyButtonTips.Clear();
			bool isFishingMiniGameActive = IsFishingMiniGameActive;
			mouseAndKeyboardHints.SetActive(isFishingMiniGameActive);
			if (isFishingMiniGameActive)
			{
				hintRodLabel.text = LLBase.L("btn_pull_rod");
				hintExitLabel.text = LLBase.L("ui_menu_exit");
				((RectTransform)mouseAndKeyboardHints.transform).RefreshContentFitter();
			}
		}
	}

	protected override void PrintTips()
	{
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.TryAdd(GameKey.PrevTab, () => SelectNextItem(reverseDirection: true));
		gameKeyDelegates.TryAdd(GameKey.NextTab, () => SelectNextItem());
		return gameKeyDelegates;
	}

	protected override void TestDraw()
	{
	}
}
