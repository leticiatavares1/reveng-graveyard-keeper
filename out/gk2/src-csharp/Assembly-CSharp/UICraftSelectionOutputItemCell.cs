using TMPro;
using UnityEngine;

public class UICraftSelectionOutputItemCell : MonoBehaviour
{
	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private TextMeshProUGUI queueCount;

	public UIItemCell UIItemCell => itemCell;

	public void Draw(OutputPreview outputPreview, int queueCount = 0)
	{
		itemCell.DrawCraftOutput(outputPreview);
		itemCell.ShowMouseSelectionFrame = false;
		UpdateQueueCount(queueCount);
	}

	public void UpdateQueueCount(int count)
	{
		queueCount.text = count.ToString();
		queueCount.gameObject.SetActive(count > 0);
	}
}
