using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestStartNotification : UIBaseNotification
{
	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private Image portrait;

	[SerializeField]
	private Color toReplace = new Color(1f, 1f, 1f, 0f);

	public QuestData QuestData { get; set; }

	private void Awake()
	{
		button.onClick.AddListener(OpenQuestTree);
	}

	public override void Draw()
	{
		portrait.sprite = QuestData.Definition.Portrait;
		portrait.BlueColorReplace(toReplace);
	}

	private void OpenQuestTree()
	{
		QuestTreePageWidget.OpenWindowFromScratchOnSelectedQuest(QuestData);
	}

	public override void ReleaseToPool()
	{
		LazyPooler.ReleaseObject(this);
	}
}
