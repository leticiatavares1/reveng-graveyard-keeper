using System;
using LazyBearTechnology;
using UnityEngine;

public class UIMapMilestone : MonoBehaviour
{
	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private GameObject activated;

	[SerializeField]
	private GameObject notActivated;

	[SerializeField]
	private GameObject selection;

	[SerializeField]
	private RectTransform navigationRect;

	private Action<UIMapMilestone> onClick;

	public RectTransform RectTransform => rectTransform;

	public RectTransform NavigationRect => navigationRect;

	public UIMapMilestoneData UIMapMilestoneData { get; private set; }

	public WgoData WgoData { get; private set; }

	public bool IsInteractable { get; private set; }

	public void DrawAsActivated(UIMapMilestoneData milestoneData, WgoData wgoData, bool isInteractable, Action<UIMapMilestone> onClick)
	{
		activated.SetActive(value: true);
		notActivated.SetActive(value: false);
		UIMapMilestoneData = milestoneData;
		WgoData = wgoData;
		IsInteractable = isInteractable;
		this.onClick = onClick;
	}

	public void DrawAsNotActivated(UIMapMilestoneData milestoneData, WgoData wgoData)
	{
		activated.SetActive(value: false);
		notActivated.SetActive(value: true);
		UIMapMilestoneData = milestoneData;
		WgoData = wgoData;
		IsInteractable = false;
		onClick = null;
	}

	public void OnClick()
	{
		if (IsInteractable)
		{
			onClick?.Invoke(this);
			LazyAudio.PlayAndForget("gui_click");
		}
	}

	public void OnEnter()
	{
		if (IsInteractable)
		{
			selection.SetActive(value: true);
			LazyAudio.PlayAndForget("gui_hover_light");
		}
	}

	public void OnExit()
	{
		selection.SetActive(value: false);
	}

	private void OnDisable()
	{
		OnExit();
	}
}
