using LazyBearTechnology;
using UnityEngine;

public class HpBarWidget : LazyWidget<HpBarWidgetData>, IBubbleLayoutAlwaysActive
{
	[SerializeField]
	private ProgressBarWiget_Simplified progressBarWidget;

	public override void Redraw()
	{
		base.Redraw();
		ApplyProgress();
	}

	public override void CustomUpdate()
	{
		ApplyProgress();
	}

	public override void Hide()
	{
		base.Hide();
		progressBarWidget.Hide();
	}

	private void ApplyProgress()
	{
		int maxHpValue = data.hpComponent.MaxHpValue;
		int greenValue = maxHpValue - data.hpComponent.Hp;
		progressBarWidget.Apply(maxHpValue, greenValue);
	}

	protected override void TestDraw()
	{
	}
}
