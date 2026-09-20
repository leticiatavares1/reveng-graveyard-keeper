using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UITabButton : MonoBehaviour
{
	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Image buttonImage;

	[SerializeField]
	private Sprite unselectedSprite;

	[SerializeField]
	private Sprite selectedSprite;

	private Action<string> onButtonClicked;

	private bool isSelected;

	private string id;

	private void Awake()
	{
		button.onClick.AddListener(OnTabClicked);
	}

	public void Init(Action<string> onButtonClicked)
	{
		this.onButtonClicked = onButtonClicked;
	}

	private void OnTabClicked()
	{
		if (!isSelected)
		{
			SetSelected(isSelected: true);
			onButtonClicked?.Invoke(id);
		}
	}

	public void SetSelected(bool isSelected)
	{
		this.isSelected = isSelected;
		buttonImage.sprite = (isSelected ? selectedSprite : unselectedSprite);
		canvas.overrideSorting = isSelected;
	}
}
