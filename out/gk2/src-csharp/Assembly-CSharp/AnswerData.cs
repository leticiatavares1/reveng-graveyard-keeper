using System;

[Serializable]
public class AnswerData
{
	public SmartRes lockRes;

	public SmartRes costRes;

	public SmartRes rewardRes;

	public SmartRes fakeRewardRes;

	public string dayNumber = string.Empty;

	public string order;

	public bool customHideCondition;

	public bool notAvailable;

	public void AddLockRes(SmartRes res)
	{
		AddRes(ref lockRes, res);
	}

	public void AddCostRes(SmartRes res)
	{
		AddRes(ref costRes, res);
	}

	public void AddRewardRes(SmartRes res)
	{
		AddRes(ref rewardRes, res);
	}

	public void AddDay(string dayNumber)
	{
		this.dayNumber = dayNumber;
	}

	public void AddOrder(string order)
	{
		this.order = order;
	}

	private void AddRes(ref SmartRes data, SmartRes res)
	{
		if (data == null)
		{
			data = res;
			return;
		}
		data.items.AddRange(res.items);
		data.gameRes.Add(res.gameRes);
	}
}
