using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingWidget : LazyWidget<UIBuildingWidgetData>
{
	private Action<List<NeedItemData>> onPress;

	private Action onOver;

	private Action onOut;

	[SerializeField]
	private Image resultIcon;

	[SerializeField]
	private Transform ingredientsContainer;

	[SerializeField]
	private Image selectionFrame;

	[SerializeField]
	private TextMeshProUGUI nameLabel;

	[SerializeField]
	private TextMeshProUGUI descriptionLabel;

	[SerializeField]
	private TextStyle canBuildStyle;

	[SerializeField]
	private TextStyle canNotBuildStyle;

	[SerializeField]
	private TextStyle descriptionDefaultStyle;

	[SerializeField]
	private TextStyle descriptionModulesStyle;

	[SerializeField]
	private VerticalLayoutGroup verticalLayoutGroup;

	[SerializeField]
	private float defaultSpaceBetweenNameAndDescription = -4f;

	[SerializeField]
	private float asianSpaceBetweenNameAndDescription = -2f;

	private LazyButton widgetButton;

	private List<UICraftItemCell> displayedIngredients = new List<UICraftItemCell>();

	public List<UICraftItemCell> DisplayedIngredients => displayedIngredients;

	public override void Init()
	{
		base.Init();
		TryGetComponent<LazyButton>(out widgetButton);
		widgetButton.onEnter.AddListener(OnOver);
		widgetButton.onExit.AddListener(OnOut);
		widgetButton.onClick.AddListener(OnPress);
	}

	public override void DeInit()
	{
		base.DeInit();
		widgetButton.onEnter.RemoveAllListeners();
		widgetButton.onExit.RemoveAllListeners();
		widgetButton.onClick.RemoveAllListeners();
	}

	public override void Redraw()
	{
		base.Redraw();
		onPress = data.OnPress;
		onOver = data.OnOver;
		onOut = data.OnOut;
		resultIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(data.BuildData.IconId);
		bool flag = data.BuildData.Definition?.HasLimits ?? false;
		foreach (UICraftItemCellData craftItemCellsDatum in data.CraftItemCellsData)
		{
			UICraftItemCell elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UICraftItemCell>(ingredientsContainer.transform);
			elementFromPool.Draw(craftItemCellsDatum, null);
			elementFromPool.GamepadNavigationItem.group = 1;
			elementFromPool.ItemCell.OnItemCellPress = delegate
			{
				OnPress();
			};
			displayedIngredients.Add(elementFromPool);
		}
		nameLabel.text = LLBase.L(data.Name);
		if (flag)
		{
			TextMeshProUGUI textMeshProUGUI = nameLabel;
			textMeshProUGUI.text = textMeshProUGUI.text + " (" + data.BuildData.Definition.GetLimitsString() + ")";
		}
		TextStyle textStyle = (data.CanBuild(data.GetCurrentNeedItems()) ? canBuildStyle : canNotBuildStyle);
		textStyle.ApplyStyle(nameLabel);
		if (!string.IsNullOrEmpty(data.DescriptionModules))
		{
			descriptionLabel.text = data.DescriptionModules;
			descriptionModulesStyle.ApplyStyle(descriptionLabel);
		}
		else if (!string.IsNullOrEmpty(data.Description))
		{
			descriptionLabel.text = data.Description;
			descriptionDefaultStyle.ApplyStyle(descriptionLabel);
		}
		else if (IsWorkbenchExtension())
		{
			descriptionLabel.text = LLBase.L("ui_workbench_extention");
			textStyle.ApplyStyle(descriptionLabel);
		}
		else
		{
			descriptionLabel.text = string.Empty;
		}
		descriptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(descriptionLabel.text));
		if (LL.IsCurrentLangAsian)
		{
			verticalLayoutGroup.spacing = asianSpaceBetweenNameAndDescription;
		}
		else
		{
			verticalLayoutGroup.spacing = defaultSpaceBetweenNameAndDescription;
		}
		Fold();
	}

	public void Fold()
	{
		foreach (UICraftItemCell displayedIngredient in displayedIngredients)
		{
			displayedIngredient.GamepadNavigationItem.Active = false;
		}
	}

	public void Unfold()
	{
		foreach (UICraftItemCell displayedIngredient in displayedIngredients)
		{
			displayedIngredient.GamepadNavigationItem.Active = true;
		}
	}

	public override void Hide()
	{
		foreach (UICraftItemCell displayedIngredient in displayedIngredients)
		{
			displayedIngredient.Flush();
			displayedIngredient.GamepadNavigationItem.group = 1;
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedIngredient);
		}
		displayedIngredients.Clear();
		selectionFrame.gameObject.SetActive(value: false);
		base.Hide();
	}

	private bool IsWorkbenchExtension()
	{
		if (data?.BuildData == null || GameBalance.Me == null)
		{
			return false;
		}
		string wgoId = data.BuildData.WgoId;
		if (GameBalance.Me.IsWorkbenchExtensionId(wgoId))
		{
			return true;
		}
		WGODef workbenchExtensionLogicDef = GameBalance.Me.GetWorkbenchExtensionLogicDef(wgoId);
		if (workbenchExtensionLogicDef != null)
		{
			return GameBalance.Me.IsWorkbenchExtensionId(workbenchExtensionLogicDef.id);
		}
		return false;
	}

	private void OnOver()
	{
		selectionFrame.gameObject.SetActive(value: true);
		onOver?.Invoke();
	}

	private void OnOut()
	{
		selectionFrame.gameObject.SetActive(value: false);
		onOut?.Invoke();
	}

	private void OnPress()
	{
		List<NeedItemData> currentNeedItems = data.GetCurrentNeedItems();
		if (data.CanBuild(currentNeedItems))
		{
			onPress?.Invoke(data.GetCurrentNeedItems());
		}
	}

	protected override void TestDraw()
	{
	}
}
