using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class CharPageTabButton : MonoBehaviour
{
	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private TextStyle normalLabelStyle;

	[SerializeField]
	private TextStyle selectedLabelStyle;

	[SerializeField]
	private GameObject actionIndicator;

	[SerializeField]
	private GameObject activeGameObject;

	[SerializeField]
	private GameObject rightSeparator;

	[SerializeField]
	private GameObject selection;

	private string tabName;

	public void Init(string name, Action onPressAction, bool isLastTab)
	{
		tabName = name;
		button.onClick.AddListener(delegate
		{
			onPressAction?.Invoke();
		});
		SetLabel();
		rightSeparator.SetActive(!isLastTab);
		button.onEnter.AddListener(OnEnter);
		button.onExit.AddListener(OnExit);
	}

	public void UpdateActionIndicatorStatus(bool value)
	{
		actionIndicator.SetActive(value);
	}

	public void UpdateState(bool isActive)
	{
		activeGameObject.SetActive(isActive);
		if (isActive)
		{
			selection.gameObject.SetActive(value: false);
			selectedLabelStyle.ApplyStyle(label);
		}
		else
		{
			normalLabelStyle.ApplyStyle(label);
		}
		button.interactable = !isActive;
	}

	private void OnEnable()
	{
		SetLabel();
	}

	private void OnDisable()
	{
		if (button.interactable)
		{
			OnExit();
		}
	}

	private void OnEnter()
	{
		selection.gameObject.SetActive(value: true);
		selectedLabelStyle.ApplyStyle(label);
	}

	private void OnExit()
	{
		selection.gameObject.SetActive(value: false);
		normalLabelStyle.ApplyStyle(label);
	}

	public void SetLabel()
	{
		if (!string.IsNullOrEmpty(tabName))
		{
			label.text = LLBase.L(tabName);
		}
	}
}
