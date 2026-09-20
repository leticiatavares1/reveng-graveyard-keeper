using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class QuestTreeTabButton : MonoBehaviour
{
	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private GameObject activeGameObject;

	[SerializeField]
	private GameObject inactiveGameObject;

	[SerializeField]
	private Image iconActive;

	[SerializeField]
	private Image iconInactive;

	[SerializeField]
	private LocalizedLabel localizedLabel;

	[SerializeField]
	private Color activeColor;

	[SerializeField]
	private Color inactiveColor;

	[SerializeField]
	private Color inactiveColorSelected;

	public string Tab { get; private set; }

	public void Init(string tab, Sprite sprite, string langToken, Action<QuestTreeTabButton> onPressAction)
	{
		OnDeselect();
		button.onExit.RemoveAllListeners();
		button.onExit.AddListener(OnDeselect);
		button.onEnter.RemoveAllListeners();
		button.onEnter.AddListener(OnSelect);
		Tab = tab;
		localizedLabel.langToken = langToken;
		localizedLabel.Localize();
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(delegate
		{
			onPressAction?.Invoke(this);
		});
		button.onClick.AddListener(OnDeselect);
		iconActive.sprite = sprite;
		iconInactive.sprite = sprite;
	}

	public void SetState(bool active)
	{
		iconActive.BlueColorReplace(activeColor);
		activeGameObject.SetActive(active);
		inactiveGameObject.SetActive(!active);
		button.interactable = !active;
	}

	private void OnDisable()
	{
		OnDeselect();
	}

	private void OnSelect()
	{
		iconInactive.BlueColorReplace(inactiveColorSelected);
	}

	private void OnDeselect()
	{
		iconInactive.BlueColorReplace(inactiveColor);
	}
}
