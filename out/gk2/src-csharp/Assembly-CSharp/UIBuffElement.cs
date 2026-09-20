using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuffElement : LazyWidget<UIBuffElementData>
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI buffDurationLabel;

	private GameObject hudFxInstance;

	public string Id { get; private set; }

	protected override void SetData(UIBuffElementData data)
	{
		Id = data.PerkData.id;
		base.SetData(data);
	}

	public void PlayHudFx()
	{
		ClearHudFx();
		string text = data?.PerkData?.Definition?.hudFxPrefabId;
		if (!string.IsNullOrEmpty(text))
		{
			hudFxInstance = HudFX.Spawn(icon.rectTransform, text);
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		icon.sprite = data.PerkData.Definition.Icon;
		icon.SetNativeSize();
		buffDurationLabel.gameObject.SetActive(!data.HasHiddenTimer);
		buffDurationLabel.text = PerkSystemData.GetFormattedDuration(data.Duration);
		Vector2 vector = (buffDurationLabel.gameObject.activeSelf ? Vector2.zero : new Vector2(0f, 8f));
		GameObject go = base.gameObject;
		Action show = ShowBuffTooltip;
		Vector2 appearOffset = vector;
		UIMouseTooltip.AttachCustom(go, show, addRaycastTarget: true, disableChildRaycasts: false, default(UIMouseTooltipEdges), appearOffset);
	}

	private void OnDisable()
	{
		ClearHudFx();
	}

	private void ClearHudFx()
	{
		if (!(hudFxInstance == null))
		{
			UnityEngine.Object.Destroy(hudFxInstance);
			hudFxInstance = null;
		}
	}

	private void ShowBuffTooltip()
	{
		if (data?.PerkData?.Definition != null)
		{
			Vector2 appearOffset = Vector2.zero;
			UIMouseTooltip component = GetComponent<UIMouseTooltip>();
			if (component != null)
			{
				appearOffset = component.GetResolvedAppearOffset();
			}
			UITooltip.ShowPerk(data.PerkData.Definition, base.transform as RectTransform, appearOffset);
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new UIBuffElementData(new PerkData("test1"), 20f, hasHiddenTimer: false, isInfinite: false));
	}
}
