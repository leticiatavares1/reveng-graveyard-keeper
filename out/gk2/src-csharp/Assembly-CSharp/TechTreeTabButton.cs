using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class TechTreeTabButton : MonoBehaviour
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
	private Image frameImageInactive;

	[SerializeField]
	private Sprite inactiveFrame;

	[SerializeField]
	private Sprite inactiveFrameSelected;

	[SerializeField]
	private GameObject inactiveOver;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Material inactiveIconMaterial;

	public TechTreeTab TechTreeTab { get; private set; }

	public void Init(TechTreeTab techTreeTab, Sprite sprite, string langToken, Action<TechTreeTabButton> onPressAction)
	{
		OnDeselect();
		button.onExit.RemoveAllListeners();
		button.onExit.AddListener(OnDeselect);
		button.onEnter.RemoveAllListeners();
		button.onEnter.AddListener(OnSelect);
		TechTreeTab = techTreeTab;
		if (localizedLabel != null)
		{
			localizedLabel.langToken = langToken;
			localizedLabel.Localize();
		}
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
		Canvas canvas = LazyUI.GetWindow<CharacterWindow>().Canvas;
		frameImageInactive.sprite = inactiveFrame;
		activeGameObject.SetActive(active);
		inactiveGameObject.SetActive(!active);
		button.interactable = !active;
		this.canvas.sortingOrder = (active ? (canvas.sortingOrder + 3) : (canvas.sortingOrder + 1));
		inactiveOver.SetActive(value: false);
		iconInactive.material = (active ? null : inactiveIconMaterial);
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
		iconInactive.material = null;
		frameImageInactive.sprite = inactiveFrameSelected;
		inactiveOver.SetActive(value: true);
	}

	private void OnDeselect()
	{
		iconInactive.material = inactiveIconMaterial;
		frameImageInactive.sprite = inactiveFrame;
		inactiveOver.SetActive(value: false);
	}
}
