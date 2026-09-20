using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UIItemAddNotification : UIBaseNotification
{
	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private TextMeshProUGUI nameLabel;

	[SerializeField]
	private TextMeshProUGUI countLabel;

	public int DisplayingCount { get; set; }

	public string ItemId { get; set; }

	public override void Draw()
	{
		ItemDef data = GameBalance.Me.GetData<ItemDef>(ItemId);
		if (data != null)
		{
			nameLabel.text = data.GetHeader();
		}
		else
		{
			nameLabel.text = LLBase.L(ItemId);
		}
		itemCell.Draw(new Item(ItemId, DisplayingCount));
		UpdateCountLabel();
	}

	public void AddCountToItem(int value)
	{
		DisplayingCount += value;
		base.CurrentTime = 0f;
		UpdateCountLabel();
	}

	private void UpdateCountLabel()
	{
		countLabel.gameObject.SetActive(DisplayingCount > 1);
		countLabel.text = "+" + DisplayingCount;
	}

	public override void ReleaseToPool()
	{
		LazyPooler.ReleaseObject(this);
	}
}
