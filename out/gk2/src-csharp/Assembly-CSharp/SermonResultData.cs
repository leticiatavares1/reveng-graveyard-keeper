using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class SermonResultData : ObjectLinkedToDefinition<SermonDef>
{
	public readonly string sermonConfigId;

	public readonly int parishionersCount;

	public readonly List<ParishionerData> parishionerDatas = new List<ParishionerData>();

	public readonly bool success;

	public readonly int churchQuality;

	public readonly int graveyardQuality;

	public readonly int successRewardCemeteryMoney;

	public readonly int successRewardChurchMoney;

	public readonly int successRewardSmileMoney;

	public readonly int baseMoneyReward;

	public readonly float successRewardCemeteryFaith;

	public readonly float successRewardChurchFaith;

	public readonly float successRewardSmileFaith;

	public readonly float baseFaithReward;

	public int Money
	{
		get
		{
			int num = parishionersCount * baseMoneyReward;
			if (success)
			{
				num += parishionersCount * successRewardSmileMoney;
				num += graveyardQuality * successRewardCemeteryMoney;
				num += churchQuality * successRewardChurchMoney;
			}
			return num;
		}
	}

	public int FaithOnlyParishioners => (int)Math.Round((float)parishionersCount * baseFaithReward, MidpointRounding.AwayFromZero);

	public int FaithOnlyBonus
	{
		get
		{
			float num = 0f;
			if (success)
			{
				num += (float)parishionersCount * successRewardSmileFaith;
				num += (float)graveyardQuality * successRewardCemeteryFaith;
				num += (float)churchQuality * successRewardChurchFaith;
			}
			return (int)Math.Round(num, MidpointRounding.AwayFromZero);
		}
	}

	public int MoneyOnlyParishioners => parishionersCount * baseMoneyReward;

	public int MoneyOnlyBonus
	{
		get
		{
			int num = 0;
			if (success)
			{
				num += parishionersCount * successRewardSmileMoney;
				num += graveyardQuality * successRewardCemeteryMoney;
				num += churchQuality * successRewardChurchMoney;
			}
			return num;
		}
	}

	public SermonResultData(string sermonId, string sermonConfigId, int parishionersCount, bool success, int churchQuality, int graveyardQuality)
		: base(sermonId)
	{
		this.sermonConfigId = sermonConfigId;
		this.parishionersCount = parishionersCount;
		this.success = success;
		this.churchQuality = churchQuality;
		this.graveyardQuality = graveyardQuality;
		successRewardCemeteryMoney = base.Definition.successRewardCemeteryMoney.EvaluateInt();
		successRewardChurchMoney = base.Definition.successRewardChurchMoney.EvaluateInt();
		successRewardSmileMoney = base.Definition.successRewardSmileMoney.EvaluateInt();
		baseMoneyReward = base.Definition.baseMoneyReward.EvaluateInt();
		successRewardCemeteryFaith = base.Definition.successRewardCemeteryFaith.EvaluateFloat();
		successRewardChurchFaith = base.Definition.successRewardChurchFaith.EvaluateFloat();
		successRewardSmileFaith = base.Definition.successRewardSmileFaith.EvaluateFloat();
		baseFaithReward = base.Definition.baseFaithReward.EvaluateFloat();
		parishionerDatas.Clear();
		if (parishionersCount <= 0)
		{
			return;
		}
		int num = (int)Math.Round((float)parishionersCount * baseFaithReward, MidpointRounding.AwayFromZero);
		float num2 = 0f;
		if (success)
		{
			num2 += (float)parishionersCount * successRewardSmileFaith;
			num2 += (float)graveyardQuality * successRewardCemeteryFaith;
			num2 += (float)churchQuality * successRewardChurchFaith;
		}
		int num3 = (int)Math.Round(num2, MidpointRounding.AwayFromZero);
		int num4 = num + num3;
		int num5 = num4 / parishionersCount;
		int num6 = num4 % parishionersCount;
		for (int i = 0; i < parishionersCount; i++)
		{
			int num7 = num5;
			if (i < num6)
			{
				num7++;
			}
			parishionerDatas.Add(new ParishionerData(base.Definition, num7));
		}
	}

	public override string ToString()
	{
		return "SermonResultData sermonConfigId:[" + sermonConfigId + "]," + string.Format(" {0}:[{1}],", "parishionersCount", parishionersCount) + string.Format(" {0}:[{1}],", "success", success) + string.Format(" {0}:[{1}],", "churchQuality", churchQuality) + string.Format(" {0}:[{1}],", "graveyardQuality", graveyardQuality) + $" {Money}:[{Money}]," + string.Format(" {0}:[{1}],", "FaithOnlyParishioners", FaithOnlyParishioners) + string.Format(" {0}:[{1}],", "FaithOnlyBonus", FaithOnlyBonus) + string.Format(" {0}:[{1}],", "MoneyOnlyParishioners", MoneyOnlyParishioners) + string.Format(" {0}:[{1}],", "MoneyOnlyBonus", MoneyOnlyBonus);
	}
}
