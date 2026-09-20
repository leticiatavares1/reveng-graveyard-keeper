using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPrefightSquadWidget : LazyWidget<UIPrefightSquadWidgetData>
{
	[SerializeField]
	private LazyButton buttonDefault;

	[SerializeField]
	private LazyButton buttonTurnedOn;

	[SerializeField]
	private GameObject turnedOnMercenaries;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image iconLocked;

	[SerializeField]
	private TextMeshProUGUI powerLabel;

	[SerializeField]
	private TextMeshProUGUI squadWeaponsLabel;

	[SerializeField]
	private GameObject[] turnedOnSelection;

	[SerializeField]
	private GameObject shading;

	[SerializeField]
	private GamepadNavigationItem gamepadNavigationItem;

	public UIPrefightSquadWidgetData Data => data;

	public override void Init()
	{
		base.Init();
		buttonDefault.onClick.AddListener(OnPressedDefault);
		buttonTurnedOn.onClick.AddListener(OnPressedTurnedOn);
		UIMouseTooltip.Attach(powerLabel.gameObject, "tt_prefight_3", null, addRaycastTarget: true);
	}

	public override void Redraw()
	{
		base.Redraw();
		data.OnRedraw = Redraw;
		gamepadNavigationItem.SetCallbacks(Empty, Empty, Empty);
		powerLabel.text = string.Format("{0}{1}", "barracks".FontIcon(), data.SquadPower);
		if (data.WgoData == null || !data.HasAnyFighterInSquad)
		{
			shading.gameObject.SetActive(value: true);
			icon.gameObject.SetActive(value: false);
			iconLocked.gameObject.SetActive(value: true);
			squadWeaponsLabel.text = "squad_equip_icon-no_equip".FontIcon() ?? "";
			buttonDefault.interactable = false;
			buttonDefault.gameObject.SetActive(value: true);
			buttonTurnedOn.gameObject.SetActive(value: false);
			GameObject[] array = turnedOnSelection;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			if (data.IsMercenary)
			{
				buttonDefault.gameObject.SetActive(value: false);
				turnedOnMercenaries.SetActive(value: true);
			}
			return;
		}
		icon.gameObject.SetActive(value: true);
		iconLocked.gameObject.SetActive(value: false);
		shading.gameObject.SetActive(value: false);
		squadWeaponsLabel.text = string.Empty;
		for (int j = 0; j < data.FightersWeapons.Count; j++)
		{
			if (data.FightersWeapons[j] == ItemType.None || data.FightersArmors[j] == ItemType.None)
			{
				squadWeaponsLabel.text += "squad_equip_icon-no_equip".FontIcon();
			}
			else if (data.FightersWeapons[j] == ItemType.Pike)
			{
				squadWeaponsLabel.text += "squad_equip_icon-spear".FontIcon();
			}
			else if (data.FightersWeapons[j] == ItemType.Bow)
			{
				squadWeaponsLabel.text += "squad_equip_icon-arrow".FontIcon();
			}
		}
		if (data.IsTurnedOn)
		{
			buttonTurnedOn.interactable = !data.IsMercenary;
			if (buttonTurnedOn.interactable)
			{
				gamepadNavigationItem.SetCallbacks(buttonTurnedOn.ForceOnEnter, buttonTurnedOn.ForceOnExit, buttonTurnedOn.ForceOnClick);
			}
			buttonDefault.gameObject.SetActive(value: false);
			buttonTurnedOn.gameObject.SetActive(value: true);
			GameObject[] array = turnedOnSelection;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
		}
		else
		{
			buttonDefault.interactable = data.CanBeTurnedOn;
			if (buttonDefault.interactable)
			{
				gamepadNavigationItem.SetCallbacks(buttonDefault.ForceOnEnter, buttonDefault.ForceOnExit, buttonDefault.ForceOnClick);
			}
			buttonTurnedOn.gameObject.SetActive(value: false);
			buttonDefault.gameObject.SetActive(value: true);
			GameObject[] array = turnedOnSelection;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
		}
		if (data.IsMercenary)
		{
			buttonDefault.gameObject.SetActive(value: false);
			buttonTurnedOn.gameObject.SetActive(value: false);
			turnedOnMercenaries.SetActive(value: true);
		}
	}

	private void Empty()
	{
	}

	private void OnPressedDefault()
	{
		data.OnPressedDefault?.Invoke(this);
	}

	private void OnPressedTurnedOn()
	{
		data.OnPressedTurnedOn?.Invoke(this);
	}

	protected override void TestDraw()
	{
	}
}
