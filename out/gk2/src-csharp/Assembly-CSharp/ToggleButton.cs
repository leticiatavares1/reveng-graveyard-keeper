using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
	[SerializeField]
	private LazyButton lazyButton;

	[SerializeField]
	private Toggle toggle;

	private Func<bool> getter;

	public LazyButton LazyButton => lazyButton;

	public void Init(Func<bool> getter, Action onClick)
	{
		this.getter = getter;
		lazyButton.onClick.RemoveAllListeners();
		lazyButton.onClick.AddListener(delegate
		{
			onClick?.Invoke();
			Refresh();
		});
		Refresh();
		SetInteractable(isInteractable: true);
	}

	public void SetInteractable(bool isInteractable)
	{
		lazyButton.interactable = isInteractable;
	}

	private void Update()
	{
		Refresh();
	}

	private void Refresh()
	{
		if (getter != null)
		{
			toggle.isOn = getter();
		}
	}
}
