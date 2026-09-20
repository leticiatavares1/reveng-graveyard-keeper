using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterOptionSwitcher : MonoBehaviour
{
	private static readonly GameKey[] decreaseKeys = new GameKey[1] { GameKey.PrevSubTab };

	private static readonly GameKey[] increaseKeys = new GameKey[1] { GameKey.NextSubTab };

	[SerializeField]
	private TextStyle switchLabelStyle;

	[SerializeField]
	private TextStyle switchLabelStyleSelected;

	[SerializeField]
	private TextStyleComponent switchLabelTextStyleComponent;

	[SerializeField]
	private TextMeshProUGUI currentOptionLabel;

	[SerializeField]
	private Image pickedColorImage;

	[SerializeField]
	private List<Sprite> spriteFields;

	[SerializeField]
	private GameObject notInteractableObj;

	[SerializeField]
	[Space]
	private UISwitchButton uiSwitchButton;

	public bool IsInteractable
	{
		get
		{
			return uiSwitchButton.IsInteractable;
		}
		set
		{
			uiSwitchButton.IsInteractable = value;
			currentOptionLabel.gameObject.SetActive(value);
			notInteractableObj.SetActive(!value);
			if (!value)
			{
				pickedColorImage.gameObject.SetActive(value: false);
			}
		}
	}

	public void Initialize(Action<int> onChangedCallback, string[] fields, int currentFieldIndex = 0, bool loopNavigation = true)
	{
		uiSwitchButton.Initialize(onChangedCallback, fields, currentFieldIndex, "", decreaseKeys, increaseKeys, loopNavigation);
	}

	public void OnGamepadFocus()
	{
		switchLabelTextStyleComponent.SetTextStyle(switchLabelStyleSelected);
	}

	public void OnGamepadUnFocus()
	{
		switchLabelTextStyleComponent.SetTextStyle(switchLabelStyle);
	}

	public void UpdateField(int index)
	{
		uiSwitchButton.UpdateField(index);
	}

	public int GetCurrentIndex()
	{
		return uiSwitchButton.CurrentFieldIndex;
	}

	public void SetSprite(int index)
	{
		pickedColorImage.gameObject.SetActive(IsInteractable);
		pickedColorImage.color = Color.white;
		pickedColorImage.sprite = spriteFields[index];
	}

	public void SetSprite(Sprite sprite)
	{
		pickedColorImage.sprite = sprite;
		pickedColorImage.gameObject.SetActive(IsInteractable);
		pickedColorImage.color = Color.white;
	}

	public void SetColorImage(Texture2D texture2D)
	{
		pickedColorImage.sprite = null;
		bool flag = texture2D != null;
		if (flag)
		{
			Color32[] pixels = texture2D.GetPixels32();
			int num = pixels.Length;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			for (int i = 0; i < num; i++)
			{
				num2 += (float)(int)pixels[i].r;
				num3 += (float)(int)pixels[i].g;
				num4 += (float)(int)pixels[i].b;
			}
			pickedColorImage.color = new Color32((byte)(num2 / (float)num), (byte)(num3 / (float)num), (byte)(num4 / (float)num), byte.MaxValue);
		}
		pickedColorImage.gameObject.SetActive(flag && IsInteractable);
	}
}
