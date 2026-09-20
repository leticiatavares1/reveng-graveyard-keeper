using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UITooltipProgressTicksWidget : LazyWidget<UITooltipProgressTicksWidgetData>
{
	private const int MAX_CELLS_AMOUNT = 4;

	[SerializeField]
	private ProgressBarWiget_Simplified progressBarWidget;

	[SerializeField]
	private UITalentIcon talentIcon;

	[SerializeField]
	private TextMeshProUGUI chanceLabel;

	[SerializeField]
	private TextMeshProUGUI additionalCellPlusLabel;

	public override void Redraw()
	{
		talentIcon.Draw(data.TalentDef, $"{data.MasteryValue}/{data.MasteryLock}");
		int masteryValue = data.MasteryValue;
		int masteryLock = data.MasteryLock;
		int num = 0;
		float num2 = 0f;
		if (!data.IsStarCraft)
		{
			if (masteryValue < masteryLock)
			{
				num = 0;
				num2 = 0f;
			}
			else
			{
				float num3 = 100f / (float)masteryLock;
				int num4 = masteryValue - masteryLock;
				float num5 = num3 * (float)num4;
				num = 1 + (int)(num5 / 100f);
				num2 = num5 % 100f;
			}
		}
		else
		{
			float num6 = 100f / (float)masteryLock;
			if (masteryValue < masteryLock)
			{
				num2 = num6 * (float)masteryValue;
			}
			else
			{
				float num7 = 100f / (float)masteryLock;
				int num8 = masteryValue - masteryLock;
				float num9 = num7 * (float)num8;
				num = 1 + (int)(num9 / 100f);
				num2 = num9 % 100f;
			}
		}
		bool num10 = num > 0 && num2 > 0f;
		num = Mathf.Clamp(num, 0, 4);
		int num11 = num + ((num2 > 0f) ? 1 : 0);
		if (num11 > 0)
		{
			progressBarWidget.Apply(num11, num);
		}
		else
		{
			progressBarWidget.Hide();
		}
		chanceLabel.text = $"{(int)num2}%";
		chanceLabel.gameObject.SetActive(num2 > 0f);
		if (num10)
		{
			additionalCellPlusLabel.gameObject.transform.SetSiblingIndex(additionalCellPlusLabel.gameObject.transform.parent.transform.childCount - 2);
			additionalCellPlusLabel.gameObject.SetActive(value: true);
		}
		else
		{
			additionalCellPlusLabel.gameObject.SetActive(value: false);
		}
	}

	public override void Hide()
	{
		progressBarWidget.Hide();
		base.Hide();
	}

	protected override void TestDraw()
	{
	}
}
