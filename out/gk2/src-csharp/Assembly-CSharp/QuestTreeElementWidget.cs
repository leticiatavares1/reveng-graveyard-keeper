using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class QuestTreeElementWidget : LazyWidget<QuestTreeElementWidgetData>
{
	[SerializeField]
	private Image selection;

	[SerializeField]
	private Image glow;

	[SerializeField]
	private Image iconDefault;

	[SerializeField]
	private GameObject[] iconsCustom;

	[SerializeField]
	private GameObject unknownObj;

	[SerializeField]
	private GameObject visibleObj;

	[SerializeField]
	private GameObject revealedObj;

	[SerializeField]
	private Image revealedImage;

	[SerializeField]
	private Sprite[] phaseSprites = new Sprite[5];

	[SerializeField]
	private GameObject completedObj;

	[SerializeField]
	private GameObject questionMark;

	public LazyButton button;

	public QuestTreeElementWidgetData Data => data;

	public override void Init()
	{
		base.Init();
		OnDeselect();
	}

	public override void Redraw()
	{
		button.onExit.RemoveAllListeners();
		button.onExit.AddListener(OnDeselect);
		button.onEnter.RemoveAllListeners();
		button.onEnter.AddListener(OnSelect);
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(OnClicked);
		base.Redraw();
		button.interactable = false;
		unknownObj.SetActive(value: false);
		visibleObj.SetActive(value: false);
		revealedObj.SetActive(value: false);
		completedObj.SetActive(value: false);
		base.name = $"{data.questData.Definition.id}({data.displayViewStatus})";
		bool flag = false;
		GameObject[] array = iconsCustom;
		foreach (GameObject gameObject in array)
		{
			if (data.questData.Definition.iconId == gameObject.name)
			{
				flag = true;
				gameObject.gameObject.SetActive(value: true);
			}
			else
			{
				gameObject.gameObject.SetActive(value: false);
			}
		}
		if (flag)
		{
			iconDefault.gameObject.SetActive(value: false);
		}
		else
		{
			iconDefault.gameObject.SetActive(value: true);
			iconDefault.sprite = data.questData.Definition.Icon;
		}
		switch (data.displayViewStatus)
		{
		case QuestViewStatus.Unknown:
		{
			unknownObj.SetActive(value: true);
			array = iconsCustom;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
			}
			iconDefault.gameObject.SetActive(value: false);
			break;
		}
		case QuestViewStatus.Visible:
			visibleObj.SetActive(value: true);
			button.interactable = true;
			break;
		case QuestViewStatus.Revealed:
		{
			revealedObj.SetActive(value: true);
			button.interactable = true;
			int num = Math.Clamp(data.questData.Definition.phase - 1, 0, 4);
			revealedImage.sprite = phaseSprites[num];
			break;
		}
		case QuestViewStatus.Completed:
			button.interactable = true;
			completedObj.SetActive(value: true);
			break;
		}
		if (questionMark != null)
		{
			questionMark.SetActive(ShouldShowQuestionMark());
		}
	}

	private bool ShouldShowQuestionMark()
	{
		if (data.hideQuestionMark)
		{
			return false;
		}
		QuestViewStatus displayViewStatus = data.displayViewStatus;
		if (displayViewStatus != QuestViewStatus.Visible && displayViewStatus != QuestViewStatus.Revealed)
		{
			return false;
		}
		QuestDef definition = data.questData.Definition;
		if (string.IsNullOrEmpty(definition.wgoNpcId))
		{
			return false;
		}
		QuestFinishCheck finishCheck = definition.finishCheck;
		if (finishCheck != null)
		{
			return !string.IsNullOrEmpty(finishCheck.phrase);
		}
		return false;
	}

	private void OnClicked()
	{
		data.onQuestClicked?.Invoke(data);
		OnDeselect();
		Redraw();
	}

	private void OnDisable()
	{
		OnDeselect();
	}

	private void OnSelect()
	{
		selection.gameObject.SetActive(value: true);
		glow.gameObject.SetActive(value: true);
	}

	private void OnDeselect()
	{
		selection.gameObject.SetActive(value: false);
		glow.gameObject.SetActive(value: false);
	}

	protected override void TestDraw()
	{
	}
}
