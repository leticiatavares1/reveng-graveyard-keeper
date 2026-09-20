using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestInfoElement : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Vector2 sizeDeltaForLabelIfAnyLinked;

	[SerializeField]
	private Vector2 sizeDeltaForLabelIfNoLinked;

	[SerializeField]
	private RectTransform linkedParent;

	[HideInInspector]
	public bool anyLinked;

	private QuestData questData;

	public QuestData QuestData => questData;

	public void Draw(QuestData questData, List<LinkedEntityWidget> linkedEntityWidgets)
	{
		label.text = questData.Description;
		for (int i = 0; i < linkedEntityWidgets.Count; i++)
		{
			linkedEntityWidgets[i].transform.SetParent(linkedParent);
		}
		anyLinked = linkedEntityWidgets.Count > 0;
		if (anyLinked)
		{
			label.rectTransform.sizeDelta = sizeDeltaForLabelIfAnyLinked;
			linkedParent.parent.gameObject.SetActive(value: true);
		}
		else
		{
			label.rectTransform.sizeDelta = sizeDeltaForLabelIfNoLinked;
			linkedParent.parent.gameObject.SetActive(value: false);
		}
		base.gameObject.SetActive(value: true);
	}
}
