using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ZombieWindowTabButton : MonoBehaviour
{
	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Canvas labelCanvas;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private GameObject actionIndicator;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Sprite activeBackSprite;

	[SerializeField]
	private Sprite inactiveBackSprite;

	[SerializeField]
	private Sprite inactiveBackSpriteSelected;

	[SerializeField]
	private TextStyle activeStyle;

	[SerializeField]
	private TextStyle inactiveStyle;

	public void Init(Action onPressAction)
	{
		button.onClick.AddListener(delegate
		{
			LazyAudio.PlayAndForget("tab_click");
			onPressAction?.Invoke();
		});
		button.onExit.RemoveAllListeners();
		button.onExit.AddListener(OnDeselect);
		button.onEnter.RemoveAllListeners();
		button.onEnter.AddListener(OnSelect);
	}

	public void UpdateActionIndicatorStatus(bool value)
	{
		actionIndicator.SetActive(value);
	}

	public void UpdateText(string text)
	{
		label.text = text;
	}

	public void UpdateState(bool isActive, Canvas windowCanvas)
	{
		if (isActive)
		{
			activeStyle.ApplyStyle(label);
			backgroundImage.sprite = activeBackSprite;
		}
		else
		{
			inactiveStyle.ApplyStyle(label);
			backgroundImage.sprite = inactiveBackSprite;
		}
		button.interactable = !isActive;
		canvas.sortingOrder = windowCanvas.sortingOrder + ((!isActive) ? 1 : 2);
		labelCanvas.sortingOrder = canvas.sortingOrder + 5;
	}

	private void OnDisable()
	{
		if (button.interactable)
		{
			OnDeselect();
		}
	}

	private void OnSelect()
	{
		backgroundImage.sprite = inactiveBackSpriteSelected;
		activeStyle.ApplyStyle(label);
	}

	private void OnDeselect()
	{
		backgroundImage.sprite = inactiveBackSprite;
		inactiveStyle.ApplyStyle(label);
	}
}
