using TMPro;
using UnityEngine;

public class CinematicsCommonObject : MonoBehaviour
{
	public TextMeshPro label;

	public TextMeshPro labelOverImage;

	public Transform center;

	public Transform centerForFullScreen;

	public bool UseOverImageText { get; private set; }

	public Transform GetCenter(bool isSwitch1)
	{
		if (!isSwitch1)
		{
			return center;
		}
		return centerForFullScreen;
	}

	public bool IsTargetLabel(TextMeshPro bound)
	{
		if (bound == null)
		{
			return false;
		}
		if (bound == labelOverImage)
		{
			return UseOverImageText;
		}
		if (bound == label)
		{
			return !UseOverImageText;
		}
		return true;
	}

	public void SetSwitchVariant(bool isSwitch1)
	{
		UseOverImageText = isSwitch1;
		HideWorldLabel(label);
		HideWorldLabel(labelOverImage);
		TextMeshPro textMeshPro = (isSwitch1 ? labelOverImage : label);
		if (textMeshPro != null)
		{
			textMeshPro.gameObject.SetActive(value: true);
		}
		if (center != null)
		{
			center.gameObject.SetActive(!isSwitch1);
		}
		if (centerForFullScreen != null)
		{
			centerForFullScreen.gameObject.SetActive(isSwitch1);
		}
	}

	public static void HideWorldLabel(TextMeshPro tmp)
	{
		if (!(tmp == null))
		{
			tmp.enabled = false;
			Renderer[] componentsInChildren = tmp.GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
	}
}
