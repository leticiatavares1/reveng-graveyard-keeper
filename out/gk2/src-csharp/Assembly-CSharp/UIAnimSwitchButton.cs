using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimSwitchButton : MonoBehaviour
{
	[SerializeField]
	private Button prevButton;

	[SerializeField]
	private Button nextButton;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private Image labelBg;

	private List<string> idList = new List<string>();

	private int currentIdIndex;

	private Action<string, bool> clickCallback;

	private void Start()
	{
		if (toggle != null)
		{
			toggle.isOn = false;
			prevButton.interactable = false;
			nextButton.interactable = false;
			labelBg.color = prevButton.colors.disabledColor;
		}
	}

	private void UpdateBtnSelection(bool isNext)
	{
		if (isNext)
		{
			currentIdIndex++;
			if (currentIdIndex >= idList.Count)
			{
				currentIdIndex = 0;
			}
		}
		else
		{
			currentIdIndex--;
			if (currentIdIndex < 0)
			{
				currentIdIndex = idList.Count - 1;
			}
		}
		label.text = idList[currentIdIndex];
		FireCallback();
	}

	private void InitButtons()
	{
		prevButton.onClick.AddListener(delegate
		{
			UpdateBtnSelection(isNext: false);
		});
		nextButton.onClick.AddListener(delegate
		{
			UpdateBtnSelection(isNext: true);
		});
	}

	private void FireCallback()
	{
		bool arg = toggle == null || toggle.isOn;
		currentIdIndex = Mathf.Clamp(currentIdIndex, 0, idList.Count - 1);
		clickCallback?.Invoke(idList[currentIdIndex], arg);
	}

	public void Init(List<string> idList, Action<string, bool> clickCallback)
	{
		this.idList = idList;
		this.clickCallback = clickCallback;
		label.text = idList[0];
		InitButtons();
		FireCallback();
		if (toggle != null)
		{
			toggle.onValueChanged.AddListener(delegate(bool toggle)
			{
				FireCallback();
				labelBg.color = (toggle ? Color.white : nextButton.colors.disabledColor);
				nextButton.interactable = toggle;
				prevButton.interactable = toggle;
			});
		}
	}

	public void OverrideValue(string id)
	{
		currentIdIndex = idList.IndexOf(id);
		if (currentIdIndex == -1)
		{
			currentIdIndex = 0;
		}
		label.text = idList[currentIdIndex];
		FireCallback();
	}

	public void Clear()
	{
		prevButton.onClick.RemoveAllListeners();
		nextButton.onClick.RemoveAllListeners();
	}
}
