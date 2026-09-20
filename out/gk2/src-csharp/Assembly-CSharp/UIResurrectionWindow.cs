using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResurrectionWindow : LazyWindow<UIResurrectionWindowData>
{
	[SerializeField]
	private UIInfoWidget infoWidget;

	[SerializeField]
	private UIWorkerIcon workerIcon;

	[SerializeField]
	private GameObject noBodyPortrait;

	[SerializeField]
	private UICorpseWidget corpseWidget;

	[SerializeField]
	private BodyOrgansInventoryWidget bodyOrgansInventoryWidget;

	[SerializeField]
	private NeedItemsWidget needItemsWidget;

	[SerializeField]
	private UIItemCell collarCell;

	[SerializeField]
	private Image collarSlot;

	[SerializeField]
	private LazyButton prepareResurrectionButton;

	[SerializeField]
	private LazyButton diceBtn;

	[SerializeField]
	private TextMeshProUGUI zombieNameLabel;

	[SerializeField]
	private TextMeshProUGUI prepareResurrectionLabel;

	[SerializeField]
	private TextMeshProUGUI resurrectionGameKeyTip;

	[SerializeField]
	private TextMeshProUGUI redSkullsValue;

	[SerializeField]
	private TextMeshProUGUI whiteSkullsValue;

	[SerializeField]
	private TextStyle skullsActiveStyle;

	[SerializeField]
	private TextStyle skullsInactiveStyle;

	[SerializeField]
	private TextStyle nameActiveStyle;

	[SerializeField]
	private TextStyle nameInactiveStyle;

	[SerializeField]
	private GameObject emptyOverlay;

	private Action onPrepareResurrectionButtonPressed;

	public override void Init()
	{
		base.Init();
		prepareResurrectionButton.onClick.AddListener(delegate
		{
			StartResurrectionButtonPressed();
		});
		diceBtn.onClick.AddListener(delegate
		{
			OnDiceButtonPressed();
		});
		prepareResurrectionButton.onNotInteractableEnter.AddListener(OnPrepareResurrectionNotInteractableOver);
		prepareResurrectionButton.onNotInteractableExit.AddListener(OnOut);
		UIMouseTooltip.Attach(collarSlot.gameObject, "tt_resur_collar");
	}

	public override void Redraw()
	{
		base.Redraw();
		data.onCollarUpdated = RedrawLite;
		infoWidget.Draw(data.InfoWidgetData);
		prepareResurrectionLabel.text = LLBase.L("ui_prepare");
		onPrepareResurrectionButtonPressed = data.onPrepareResurrectionButtonPressed;
		redSkullsValue.text = string.Format("{0}{1}", "rskull".FontIcon(), data.RedSkulls);
		whiteSkullsValue.text = string.Format("{0}{1}", "skull".FontIcon(), data.WhiteSkulls);
		RedrawLite();
		corpseWidget.Draw(data.CorpseWidgetData);
		bodyOrgansInventoryWidget.Draw(data.BodyOrgansInventoryWidgetData);
		needItemsWidget.Draw(data.NeedItemsWidgetData);
		if (data.IsEmpty)
		{
			diceBtn.interactable = false;
			skullsInactiveStyle.ApplyStyle(whiteSkullsValue);
			skullsInactiveStyle.ApplyStyle(redSkullsValue);
			nameInactiveStyle.ApplyStyle(zombieNameLabel);
			emptyOverlay.SetActive(value: true);
			zombieNameLabel.text = LLBase.L("ui_no_body");
			workerIcon.gameObject.SetActive(value: false);
			noBodyPortrait.SetActive(value: true);
			collarCell.LazyButton.interactable = false;
		}
		else
		{
			diceBtn.interactable = true;
			skullsActiveStyle.ApplyStyle(whiteSkullsValue);
			skullsActiveStyle.ApplyStyle(redSkullsValue);
			nameActiveStyle.ApplyStyle(zombieNameLabel);
			emptyOverlay.SetActive(value: false);
			zombieNameLabel.text = LLBase.L(data.ZombieName);
			RedrawWorkerIcon();
			workerIcon.gameObject.SetActive(value: true);
			noBodyPortrait.SetActive(value: false);
			collarCell.LazyButton.interactable = true;
		}
		collarCell.OnItemCellPress = data.OnCollarPressed;
		if (LazyInput.IsGamepadActive)
		{
			if (data.IsEmpty)
			{
				base.GamepadNavigationController.Disable();
			}
			else
			{
				base.GamepadNavigationController.Enable();
				base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			}
			UpdateResurrectionTip();
		}
	}

	private void RedrawLite()
	{
		RedrawCollar();
		prepareResurrectionButton.interactable = data.CanStartResurrection();
		collarCell.Draw(data.Collar);
		collarCell.OnItemCellPress = data.OnCollarPressed;
		if (LazyInput.IsGamepadActive)
		{
			resurrectionGameKeyTip.text = new LazyGameKeyTip(GameKey.StartResurrection, prepareResurrectionLabel.text, prepareResurrectionButton.interactable, gamepadOnly: false).ToString();
		}
	}

	private void RedrawWorkerIcon()
	{
		workerIcon.ShowWithoutTalent(null, ZombieSkinHelper.GetPresetForCustomizationData("zombie_worker", data.RolledSkin.body, data.RolledSkin.head, string.Empty, data.RolledSkin.headLut));
	}

	private void RedrawCollar()
	{
		if (data.Collar.IsEmpty)
		{
			collarSlot.gameObject.SetActive(value: true);
			collarCell.LazyButton.interactable = false;
		}
		else
		{
			collarSlot.gameObject.SetActive(value: false);
			collarCell.LazyButton.interactable = true;
		}
	}

	public override void Hide()
	{
		base.Hide();
		prepareResurrectionButton?.onExit?.Invoke();
		if (data != null)
		{
			corpseWidget.Hide();
			bodyOrgansInventoryWidget.Hide();
			needItemsWidget.Hide();
		}
	}

	protected override void HideWindow()
	{
		if (data != null && !string.IsNullOrEmpty(data.ZombieName))
		{
			MainGame.Instance.GameSave.knowledgeSystem.freeZombieNames.Add(data.ZombieName);
			data.ZombieName = string.Empty;
		}
		base.HideWindow();
	}

	private bool StartResurrectionButtonPressed()
	{
		if (!prepareResurrectionButton.interactable)
		{
			return false;
		}
		onPrepareResurrectionButtonPressed?.Invoke();
		return true;
	}

	private bool OnTakeBodyButtonPressed()
	{
		corpseWidget.OnButtonPressed();
		return true;
	}

	private bool OnDiceButtonPressed()
	{
		KnowledgeSystem knowledgeSystem = MainGame.Instance.GameSave.knowledgeSystem;
		string zombieName = data.ZombieName;
		string previousLocalizedName = (string.IsNullOrEmpty(zombieName) ? string.Empty : LLBase.L(zombieName));
		string text = zombieNameLabel.text;
		int count = knowledgeSystem.freeZombieNames.Count;
		bool freeNamesContainedPreviousName = !string.IsNullOrEmpty(zombieName) && knowledgeSystem.freeZombieNames.Contains(zombieName);
		data.ZombieName = knowledgeSystem.GetZombieName();
		string text2 = (string.IsNullOrEmpty(data.ZombieName) ? string.Empty : LLBase.L(data.ZombieName));
		int count2 = knowledgeSystem.freeZombieNames.Count;
		if (!string.IsNullOrEmpty(zombieName))
		{
			knowledgeSystem.freeZombieNames.Add(zombieName);
		}
		zombieNameLabel.text = text2;
		LogZombieNameRollResult(zombieName, data.ZombieName, previousLocalizedName, text2, text, count, count2, knowledgeSystem.freeZombieNames.Count, freeNamesContainedPreviousName);
		RedrawWorkerIcon();
		return true;
	}

	private void LogZombieNameRollResult(string previousName, string rolledName, string previousLocalizedName, string rolledLocalizedName, string previousLabelText, int freeNamesCountBeforeRoll, int freeNamesCountAfterGet, int freeNamesCountAfterReturn, bool freeNamesContainedPreviousName)
	{
		string zombieNameNotChangedReason = GetZombieNameNotChangedReason(previousName, rolledName, previousLocalizedName, rolledLocalizedName, freeNamesCountBeforeRoll, freeNamesContainedPreviousName);
		Debug.LogWarning("[UIResurrectionWindow][ZombieNameRoll] Name did not change after roll. reason:[" + zombieNameNotChangedReason + "] previousRaw:[" + previousName + "] rolledRaw:[" + rolledName + "] previousLocalized:[" + previousLocalizedName + "] rolledLocalized:[" + rolledLocalizedName + "] " + $"previousLabel:[{previousLabelText}] freeNamesBeforeRoll:[{freeNamesCountBeforeRoll}] " + $"freeNamesAfterGet:[{freeNamesCountAfterGet}] freeNamesAfterReturn:[{freeNamesCountAfterReturn}] " + $"freeNamesContainedPreviousNameBeforeRoll:[{freeNamesContainedPreviousName}]");
	}

	private string GetZombieNameNotChangedReason(string previousName, string rolledName, string previousLocalizedName, string rolledLocalizedName, int freeNamesCountBeforeRoll, bool freeNamesContainedPreviousName)
	{
		if (freeNamesCountBeforeRoll == 0)
		{
			return "freeZombieNames was empty, KnowledgeSystem.GetZombieName used fallback zombie_name_1";
		}
		if (previousName == rolledName)
		{
			if (!freeNamesContainedPreviousName)
			{
				return "rolled raw name equals previous raw name";
			}
			return "freeZombieNames contained the current raw name before roll, random selected it again";
		}
		if (previousLocalizedName == rolledLocalizedName)
		{
			return "raw name changed, but localized display text is the same";
		}
		return "label text stayed the same for an unknown reason";
	}

	private void OnPrepareResurrectionNotInteractableOver()
	{
		if (!data.IsEmpty && !prepareResurrectionButton.interactable)
		{
			data.onPrepareResurrectionNonInteractableButtonOver?.Invoke(prepareResurrectionButton);
		}
	}

	private void OnOut()
	{
		UITooltip.Hide();
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Print(LazyGameKeyTip.Select(), new LazyGameKeyTip(GameKey.ZombieRollName, "tip_roll_zombie_name"), LazyGameKeyTip.Back());
		UpdateResurrectionTip();
	}

	private void UpdateResurrectionTip()
	{
		if (data != null && data.CanStartResurrection != null)
		{
			resurrectionGameKeyTip.text = new LazyGameKeyTip(GameKey.StartResurrection, prepareResurrectionLabel.text, data.CanStartResurrection(), gamepadOnly: false).ToString();
		}
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.ExtractBody, OnTakeBodyButtonPressed);
		gameKeyDelegates.Add(GameKey.ZombieRollName, OnDiceButtonPressed);
		gameKeyDelegates.Add(GameKey.StartResurrection, StartResurrectionButtonPressed);
		return gameKeyDelegates;
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Wgo wgo = Wgo.Spawn(new WgoData("resurrection_table_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId), MainGame.PlayerController.CurrentGameScene.transform);
		CustomInteractionHandler customInteractionHandler = new CustomInteractionHandler();
		customInteractionHandler.Init(wgo);
		customInteractionHandler.HasInteraction(MainGame.PlayerController);
		customInteractionHandler.Interact(MainGame.PlayerController);
	}

	[LazyUITest]
	protected void TestDrawBody()
	{
		WgoData wgoData = new WgoData("resurrection_table_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		Wgo wgo = Wgo.Spawn(wgoData, MainGame.PlayerController.CurrentGameScene.transform);
		CustomInteractionHandler customInteractionHandler = new CustomInteractionHandler();
		customInteractionHandler.Init(wgo);
		Item item = GameBalance.Me.GetData<BodyDef>("body_0_2").GenerateItem();
		wgoData.Inventory.AddItemToInventory(item);
		customInteractionHandler.HasInteraction(MainGame.PlayerController);
		customInteractionHandler.Interact(MainGame.PlayerController);
	}
}
