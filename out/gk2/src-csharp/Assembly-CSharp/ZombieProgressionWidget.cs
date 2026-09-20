using LazyBearTechnology;
using UnityEngine;

public class ZombieProgressionWidget : LazyWidget<ZombieProgressionWidgetData>
{
	[SerializeField]
	private TalentLevelUpsWidget talentLevelUpsWidget;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private TalentTabButtonsContainer talentTabButtonsContainer;

	private TalentLevelUpsWidgetData talentLevelUpsWidgetData;

	public TalentTabButtonsContainer TalentTabButtonsContainer => talentTabButtonsContainer;

	public override void Init()
	{
		base.Init();
		talentTabButtonsContainer.Init(delegate(string talentId)
		{
			data.SwitchTalent(talentId);
			RedrawTalentLevelUpsWidget();
		}, canvas);
	}

	public override void Redraw()
	{
		base.Redraw();
		talentTabButtonsContainer.Draw(data.ZombieTalentData.id);
		RedrawTalentLevelUpsWidget();
	}

	private void RedrawTalentLevelUpsWidget()
	{
		talentLevelUpsWidgetData = new TalentLevelUpsWidgetData(data.ZombieWgoData, data.ZombieTalentData);
		talentLevelUpsWidget.Draw(talentLevelUpsWidgetData);
	}

	protected override void TestDraw()
	{
	}
}
