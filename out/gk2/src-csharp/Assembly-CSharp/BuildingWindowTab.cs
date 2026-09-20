using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWindowTab : MonoBehaviour
{
	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private GameObject activeObject;

	[SerializeField]
	private GameObject inactiveObject;

	[SerializeField]
	private Image[] imageIcons;

	[SerializeField]
	private Image[] backgroundImages;

	[SerializeField]
	private Sprite activeBackSprite;

	[SerializeField]
	private Sprite inactiveBackSprite;

	[SerializeField]
	private Sprite inactiveBackSpriteSelected;

	private Action<BuildingWindowTab> onPressAction;

	private bool isActive;

	private string tabId;

	private bool isInitialized;

	public string TabId => tabId;

	public void Init(string tabId, Action<BuildingWindowTab> onPressAction)
	{
		this.tabId = tabId;
		this.onPressAction = onPressAction;
		if (!isInitialized)
		{
			isInitialized = true;
			button.onDown.AddListener(HandlePress);
			button.onExit.RemoveAllListeners();
			button.onExit.AddListener(OnDeselect);
			button.onEnter.RemoveAllListeners();
			button.onEnter.AddListener(OnSelect);
		}
	}

	public void UpdateState(bool isActive, Canvas parentCanvas)
	{
		this.isActive = isActive;
		Image[] array;
		if (isActive)
		{
			array = backgroundImages;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].sprite = activeBackSprite;
			}
		}
		else
		{
			array = backgroundImages;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].sprite = inactiveBackSprite;
			}
		}
		array = imageIcons;
		foreach (Image obj in array)
		{
			obj.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(isActive ? (tabId + "_active") : (tabId + "_inactive"));
			obj.SetNativeSize();
		}
		button.interactable = !isActive;
		UpdateSorting(parentCanvas);
	}

	public void UpdateSorting(Canvas parentCanvas)
	{
		canvas.sortingOrder = parentCanvas.sortingOrder + 5 + (isActive ? 1 : (-1));
	}

	private void HandlePress()
	{
		onPressAction?.Invoke(this);
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
		Image[] array = backgroundImages;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sprite = inactiveBackSpriteSelected;
		}
		array = imageIcons;
		foreach (Image obj in array)
		{
			obj.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(tabId + "_active");
			obj.SetNativeSize();
		}
	}

	private void OnDeselect()
	{
		Image[] array = backgroundImages;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sprite = inactiveBackSprite;
		}
		array = imageIcons;
		foreach (Image obj in array)
		{
			obj.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(tabId + "_inactive");
			obj.SetNativeSize();
		}
	}
}
