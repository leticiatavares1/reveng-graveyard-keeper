using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UIMoneyChangedNotification : UIBaseNotification
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private TextStyle plusStyle;

	[SerializeField]
	private TextStyle minusStyle;

	public int DisplayingCount { get; set; }

	public override void Draw()
	{
		label.text = Trading.FormatMoney(DisplayingCount, printZero: false, " ", GameResIconType.MoneyBig);
		if (DisplayingCount >= 0)
		{
			plusStyle.ApplyStyle(label);
		}
		else
		{
			minusStyle.ApplyStyle(label);
		}
	}

	public void AddCount(int value)
	{
		DisplayingCount += value;
		base.CurrentTime = 0f;
		Draw();
	}

	public override void ReleaseToPool()
	{
		LazyPooler.ReleaseObject(this);
	}
}
