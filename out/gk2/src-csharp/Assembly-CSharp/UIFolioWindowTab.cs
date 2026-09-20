using System;
using LazyBearTechnology;
using UnityEngine;

public class UIFolioWindowTab : MonoBehaviour
{
	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private GameObject activeObj;

	[SerializeField]
	private GameObject inactiveObj;

	[SerializeField]
	private AlchemyFormulaTab tab;

	public AlchemyFormulaTab Tab => tab;

	public void Init(Action<UIFolioWindowTab> onPressAction)
	{
		button.onExit.RemoveAllListeners();
		button.onEnter.RemoveAllListeners();
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(delegate
		{
			onPressAction?.Invoke(this);
		});
	}

	public void SetState(bool active)
	{
		activeObj.SetActive(active);
		inactiveObj.SetActive(!active);
		button.interactable = !active;
	}
}
