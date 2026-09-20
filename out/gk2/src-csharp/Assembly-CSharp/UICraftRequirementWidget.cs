using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICraftRequirementWidget : LazyWidget<UICraftRequirementWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI requirementId;

	[SerializeField]
	private TextMeshProUGUI requirement;

	[SerializeField]
	private TextStyleComponent requirementTextStyle;

	[SerializeField]
	private TextStyle commonTextStyle;

	[SerializeField]
	private TextStyle enoughTextStyle;

	[SerializeField]
	private TextStyle notEnoughTextStyle;

	private bool isHovered;

	public override void Redraw()
	{
		base.Redraw();
		requirementId.text = data.IconId.FontIcon();
		requirement.text = data.RequirementValue;
		requirement.gameObject.SetActive(!string.IsNullOrEmpty(data.RequirementValue));
		if (data.IsRequirement)
		{
			requirementTextStyle.SetTextStyle(data.IsEnough ? enoughTextStyle : notEnoughTextStyle);
		}
		else
		{
			requirementTextStyle.SetTextStyle(commonTextStyle);
		}
	}

	public void OnOver()
	{
		isHovered = true;
		ShowUITooltip();
	}

	public void OnOut()
	{
		isHovered = false;
		HideUITooltip();
	}

	private void OnDisable()
	{
		if (isHovered && UITooltip.IsTooltipShowingAtTarget(base.transform as RectTransform))
		{
			HideUITooltip(immediately: true);
		}
	}

	public void ShowUITooltip()
	{
		isHovered = true;
		UITooltip.ShowCraftRequirementDescription(this, data.Id, data.CraftDef, data.LinkedActivePerks, data.ToolForWork);
	}

	public void HideUITooltip(bool immediately = false)
	{
		isHovered = false;
		if (immediately)
		{
			UITooltip.HideImmediately();
		}
		else
		{
			UITooltip.Hide();
		}
	}

	protected override void TestDraw()
	{
	}
}
