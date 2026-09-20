using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInfoWidget : LazyWidget<UIInfoWidgetData>
{
	private const string FIRE_ITEM_ID = "fire";

	[SerializeField]
	private TextMeshProUGUI header;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private TextMeshProUGUI descriptionFuel;

	[SerializeField]
	private TextMeshProUGUI descriptionFuel2;

	[SerializeField]
	private TextMeshProUGUI descriptionTick;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image background;

	[SerializeField]
	private UIWorkerIcon workerIcon;

	[SerializeField]
	private UIWorkerIcon workerIconGamepad;

	[SerializeField]
	private TextStyle worldZoneQualityStyle;

	[Space]
	[SerializeField]
	private Sprite defaultBgIcon;

	[SerializeField]
	private Sprite conveyorBgIcon;

	private bool subscribedWgoEvents;

	public static bool IsDisabled { get; set; }

	public Sprite Icon => icon.sprite;

	public override void Draw(UIInfoWidgetData data)
	{
		if (IsDisabled)
		{
			Hide();
		}
		else
		{
			base.Draw(data);
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		SubscrubeToWgoEvents();
		header.text = data.Header;
		icon.sprite = data.Icon;
		UpdateBackgroundIcon();
		UpdateDescription();
		UpdateWorkerIcon();
	}

	public override void Hide()
	{
		base.Hide();
		UnsubscribeFromWgoEvents();
	}

	private void SubscrubeToWgoEvents()
	{
		if (data != null && data.CraftComponent != null && data.CraftComponent.CraftableObject?.CraftableObjectInventory != null && !subscribedWgoEvents)
		{
			data.CraftComponent.CraftableObject.CraftableObjectInventory.OnItemsAdd += UpdateDescription;
			data.CraftComponent.CraftableObject.CraftableObjectInventory.OnItemsRemove += UpdateDescription;
			subscribedWgoEvents = true;
		}
	}

	private void UnsubscribeFromWgoEvents()
	{
		if (data != null && data.CraftComponent != null && data.CraftComponent.CraftableObject.CraftableObjectInventory != null && subscribedWgoEvents)
		{
			data.CraftComponent.CraftableObject.CraftableObjectInventory.OnItemsAdd -= UpdateDescription;
			data.CraftComponent.CraftableObject.CraftableObjectInventory.OnItemsRemove -= UpdateDescription;
			subscribedWgoEvents = false;
		}
	}

	private void UpdateWorkerIcon()
	{
		workerIcon.transform.parent.parent.gameObject.SetActive(value: false);
		workerIconGamepad.transform.parent.parent.gameObject.SetActive(value: false);
		workerIcon.Hide();
		workerIconGamepad.Hide();
		if (data.CraftComponent == null || !data.CraftComponent.HasCraftsByBalance || data.Worker == null)
		{
			return;
		}
		SkinPresetGK2 skinPreset = null;
		if (data.Worker is ZombieWgoData zombieWgoData)
		{
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(zombieWgoData.UniqueId);
			if (wgoViewGlobal != null)
			{
				skinPreset = wgoViewGlobal.MainWgoPart.AnimationComponent.SkinPreset;
			}
		}
		else if (data.Worker is PlayerController playerController)
		{
			skinPreset = playerController.View.PlayerAnimation.SkinPreset;
		}
		if (LazyInput.IsGamepadActive)
		{
			workerIconGamepad.Show(data.Worker, skinPreset, GameBalance.Me.GetData<TalentDef>(data.WgoData.Definition.talent));
			workerIconGamepad.transform.parent.parent.gameObject.SetActive(value: true);
		}
		else
		{
			workerIcon.Show(data.Worker, skinPreset, GameBalance.Me.GetData<TalentDef>(data.WgoData.Definition.talent));
			workerIcon.transform.parent.parent.gameObject.SetActive(value: true);
		}
	}

	public void UpdateDescription(List<Item> items = null)
	{
		bool flag = TryShowLabeledWorldZoneQuality();
		if (!flag)
		{
			description.text = data.Description;
			if (string.IsNullOrEmpty(data.Description))
			{
				if (!string.IsNullOrEmpty(data.WorldZoneQuality))
				{
					description.text += worldZoneQualityStyle.ApplyStyleToString(data.WorldZoneQuality);
				}
			}
			else if (!string.IsNullOrEmpty(data.WorldZoneQuality))
			{
				TextMeshProUGUI textMeshProUGUI = description;
				textMeshProUGUI.text = textMeshProUGUI.text + ": " + worldZoneQualityStyle.ApplyStyleToString(data.WorldZoneQuality);
			}
		}
		descriptionFuel.transform.parent.gameObject.SetActive(value: false);
		descriptionFuel2.transform.parent.gameObject.SetActive(value: false);
		descriptionTick.transform.parent.gameObject.SetActive(value: false);
		description.gameObject.SetActive(!string.IsNullOrEmpty(description.text));
		descriptionFuel.transform.parent.parent.gameObject.SetActive(value: false);
		descriptionFuel2.transform.parent.parent.gameObject.SetActive(value: false);
		if (data.CraftComponent == null)
		{
			return;
		}
		bool flag2 = flag;
		if (!flag && (data.CraftComponent.HasCraftsByBalance || data.WgoData.Definition.interactionType == WGODef.InteractionType.Survey || data.WgoData.Definition.interactionType == WGODef.InteractionType.Alchemy || data.WgoData.Definition.interactionType == WGODef.InteractionType.Autopsy))
		{
			if (data.CraftComponent.AvailableCrafts.Count > 0 && data.CraftComponent.AvailableCrafts[0].isFuelCraft)
			{
				ItemDef fuelItemDef = data.CraftComponent.AvailableCrafts[0].FuelItemDef;
				string text = $"{fuelItemDef.id.FontIcon()}{data.WgoData.Inventory.Data.GetTotalCountInInventory(fuelItemDef.id)}/{data.WgoData.Definition.emptyCellStackCount * data.WgoData.Inventory.Data.InventorySize}";
				flag2 = TryShowLabeledFuelHeader(fuelItemDef, text);
				if (!flag2)
				{
					descriptionFuel.transform.parent.gameObject.SetActive(value: true);
					descriptionFuel.transform.parent.parent.gameObject.SetActive(value: true);
					descriptionFuel.text = text;
					description.gameObject.SetActive(value: false);
					AttachHeaderFuelTooltip(fuelItemDef, descriptionFuel.transform.parent.gameObject);
				}
			}
			else if (!string.IsNullOrEmpty(data.WgoData.Definition.fuelItemId))
			{
				ItemDef fuelItemDef = data.WgoData.Definition.FuelItemDef;
				string text2 = $"{fuelItemDef.id.FontIcon()}{data.WgoData.GetCraftableMultiInventory(data.ExcludePlayerFromMultiinventoryWhenCountItemsForFuel).GetTotalCount(fuelItemDef.id)}";
				flag2 = TryShowLabeledFuelHeader(fuelItemDef, text2);
				if (!flag2)
				{
					descriptionFuel.transform.parent.gameObject.SetActive(value: true);
					descriptionFuel.transform.parent.parent.gameObject.SetActive(value: true);
					descriptionFuel.text = text2;
					description.gameObject.SetActive(value: false);
					AttachHeaderFuelTooltip(fuelItemDef, descriptionFuel.transform.parent.gameObject);
					if (!string.IsNullOrEmpty(data.WgoData.Definition.fuelItemId2))
					{
						fuelItemDef = data.WgoData.Definition.FuelItemDef2;
						descriptionFuel2.transform.parent.gameObject.SetActive(value: true);
						descriptionFuel2.transform.parent.parent.gameObject.SetActive(value: true);
						descriptionFuel2.text = $"{fuelItemDef.id.FontIcon()}{data.WgoData.GetCraftableMultiInventory(data.ExcludePlayerFromMultiinventoryWhenCountItemsForFuel).GetTotalCount(fuelItemDef.id)}";
						description.gameObject.SetActive(value: false);
						AttachHeaderFuelTooltip(fuelItemDef, descriptionFuel2.transform.parent.gameObject);
					}
				}
			}
		}
		if (!data.ShowTickDuration)
		{
			return;
		}
		if (data.ShowTickDuration && data.CraftComponent != null && data.CraftComponent.HasCraftsByBalance && data.CraftComponent.AvailableCrafts[0].isAuto)
		{
			descriptionTick.transform.parent.gameObject.SetActive(value: true);
			TimeSpan timeSpan = TimeSpan.FromSeconds(data.WgoData.Definition.autocraftTickDuration.EvaluateFloat(data.WgoData));
			descriptionTick.text = "cell_time".FontIcon() + timeSpan.ToString("m\\:ss");
			descriptionFuel.transform.parent.parent.gameObject.SetActive(value: true);
			if (!flag2)
			{
				description.gameObject.SetActive(value: false);
			}
			AttachTickLengthTooltip();
		}
		else
		{
			descriptionTick.transform.parent.gameObject.SetActive(value: false);
		}
	}

	private bool TryShowLabeledWorldZoneQuality()
	{
		if (string.IsNullOrEmpty(data.WorldZoneQuality))
		{
			return false;
		}
		string worldZoneQualityHeaderId = GetWorldZoneQualityHeaderId();
		if (string.IsNullOrEmpty(worldZoneQualityHeaderId))
		{
			return false;
		}
		string text = worldZoneQualityStyle.ApplyStyleToString(data.WorldZoneQuality);
		description.text = (string.IsNullOrEmpty(data.Description) ? (FormatLabeledHeader(worldZoneQualityHeaderId) + " " + text) : (data.Description + ": " + text));
		description.gameObject.SetActive(value: true);
		AttachLabeledHeaderTooltip(worldZoneQualityHeaderId);
		return true;
	}

	private bool TryShowLabeledFuelHeader(ItemDef fuelItemDef, string fuelValue)
	{
		string labeledFuelHeaderId = GetLabeledFuelHeaderId(fuelItemDef);
		if (string.IsNullOrEmpty(labeledFuelHeaderId) || string.IsNullOrEmpty(fuelValue))
		{
			return false;
		}
		description.text = FormatLabeledHeader(labeledFuelHeaderId) + " " + worldZoneQualityStyle.ApplyStyleToString(fuelValue);
		description.gameObject.SetActive(value: true);
		AttachLabeledHeaderTooltip(labeledFuelHeaderId);
		return true;
	}

	private void AttachLabeledHeaderTooltip(string headerLocId)
	{
		if (!(headerLocId != "hdr_factory_gears"))
		{
			UIMouseTooltip component = description.transform.parent.GetComponent<UIMouseTooltip>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			UIMouseTooltip.Attach(description.gameObject, "tt_factory_gears", null, addRaycastTarget: true);
		}
	}

	private static string FormatLabeledHeader(string headerLocId)
	{
		return LLBase.L(headerLocId).TrimEnd(' ', ':') + ":";
	}

	private string GetWorldZoneQualityHeaderId()
	{
		WorldZoneData worldZoneData = data.WgoData?.WorldZoneData;
		if (worldZoneData == null)
		{
			data.WgoData?.TryGetNearestBuilderWorldZone(out worldZoneData);
		}
		string qualityIconHeaderId = GetQualityIconHeaderId(worldZoneData?.Definition?.qualityIcon);
		if (!string.IsNullOrEmpty(qualityIconHeaderId))
		{
			return qualityIconHeaderId;
		}
		WgoData wgoData = data.WgoData;
		if (wgoData != null && wgoData.Definition?.interactionType == WGODef.InteractionType.Autopsy)
		{
			return "hdr_autopsy_bodies";
		}
		return null;
	}

	private string GetLabeledFuelHeaderId(ItemDef fuelItemDef)
	{
		WgoData wgoData = data.WgoData;
		if (wgoData != null && wgoData.Definition?.interactionType == WGODef.InteractionType.Autopsy)
		{
			return "hdr_autopsy_bodies";
		}
		string qualityIconHeaderId = GetQualityIconHeaderId(fuelItemDef?.id);
		if (!string.IsNullOrEmpty(qualityIconHeaderId))
		{
			return qualityIconHeaderId;
		}
		return GetQualityIconHeaderId(fuelItemDef?.qualityIcon);
	}

	private static string GetQualityIconHeaderId(string icon)
	{
		switch (icon)
		{
		case "gear":
			return "hdr_factory_gears";
		case "corpse":
		case "body":
			return "hdr_autopsy_bodies";
		default:
			return null;
		}
	}

	private void AttachHeaderFuelTooltip(ItemDef fuelItemDef, GameObject chip)
	{
		if (fuelItemDef == null || chip == null)
		{
			return;
		}
		if (fuelItemDef.id == "fire")
		{
			UIMouseTooltip.Attach(chip, "tt_craft_fire", null, addRaycastTarget: true);
			return;
		}
		if (fuelItemDef.id == "alchemy_flask")
		{
			UIMouseTooltip.Attach(chip, "tt_alchemy_flasks", null, addRaycastTarget: true);
		}
		WgoData wgoData = data.WgoData;
		if (wgoData != null && wgoData.Definition?.interactionType == WGODef.InteractionType.Survey)
		{
			if (fuelItemDef.id == "faith")
			{
				UIMouseTooltip.Attach(chip, "tt_study_faith", null, addRaycastTarget: true, disableChildRaycasts: false, default(UIMouseTooltipEdges), default(Vector2), "faith");
			}
			else if (fuelItemDef.id == "science")
			{
				UIMouseTooltip.Attach(chip, "tt_study_science", null, addRaycastTarget: true, disableChildRaycasts: false, default(UIMouseTooltipEdges), default(Vector2), "science");
			}
		}
	}

	private void AttachTickLengthTooltip()
	{
		UIMouseTooltip.Attach(descriptionTick.transform.parent.gameObject, "tt_craft_tick_length", null, addRaycastTarget: true);
	}

	public void TurnOnTickDuration()
	{
		descriptionTick.transform.parent.gameObject.SetActive(value: true);
		descriptionFuel.transform.parent.parent.gameObject.SetActive(value: true);
		description.gameObject.SetActive(value: false);
		AttachTickLengthTooltip();
	}

	private void UpdateBackgroundIcon()
	{
		if (data == null || data.WgoData == null || data.WgoData.Definition == null || !data.DefineIconBackgroundFromWgo || !(background != null))
		{
			return;
		}
		if (data.WgoData != null)
		{
			if (data.WgoData.Definition.craftIconColor > 0 || data.WgoData.Definition.conveyorType != 0)
			{
				background.sprite = conveyorBgIcon;
			}
			else
			{
				background.sprite = defaultBgIcon;
			}
		}
		else
		{
			background.sprite = defaultBgIcon;
		}
	}

	protected override void TestDraw()
	{
	}
}
