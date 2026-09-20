using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class SermonDef : CraftDefBase
{
	[AutoParse("min_parishioners")]
	public int minParishioners;

	[AutoParse("sermon_difficulty")]
	public int sermonDifficulty;

	[AutoParse("base_reward_faith")]
	public LazyExpression baseFaithReward;

	[AutoParse("base_reward_money")]
	public LazyExpression baseMoneyReward;

	[AutoParse("success_reward_buff")]
	public string successRewardBuff;

	[AutoParse("success_reward_item")]
	public OutputItems successRewardItem = new OutputItems();

	[AutoParse("success_reward_cemetery_faith")]
	public LazyExpression successRewardCemeteryFaith;

	[AutoParse("success_reward_cemetery_money")]
	public LazyExpression successRewardCemeteryMoney;

	[AutoParse("success_reward_church_faith")]
	public LazyExpression successRewardChurchFaith;

	[AutoParse("success_reward_church_money")]
	public LazyExpression successRewardChurchMoney;

	[AutoParse("success_reward_smile_faith")]
	public LazyExpression successRewardSmileFaith;

	[AutoParse("success_reward_smile_money")]
	public LazyExpression successRewardSmileMoney;

	[AutoParse("pray_icon")]
	public string prayIcon;

	[AutoParse("pray_on_end_expression")]
	public List<LazyExpression> prayOnEndExpressions = new List<LazyExpression>();

	private OutputPreview outputPreview;

	private PerkWidgetData perkWidgetData;

	public Sprite PrayIcon => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(prayIcon, "i_b_base_act");

	public override OutputPreview GetOutputPreview(WgoData wgoData = null)
	{
		if (outputPreview == null)
		{
			outputPreview = new OutputPreview(id, "", isStarOutput: false, 1);
		}
		return outputPreview;
	}

	public PerkWidgetData GetBuffWidgetData()
	{
		if (perkWidgetData == null && !string.IsNullOrEmpty(successRewardBuff))
		{
			perkWidgetData = new PerkWidgetData(new PerkData(successRewardBuff), isActive: true, null, null, null);
		}
		return perkWidgetData;
	}
}
