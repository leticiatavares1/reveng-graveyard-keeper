using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectSwitcher : MonoBehaviour
{
	[Serializable]
	public class CustomObject
	{
		public GameObject go;

		public LightEnvironmentPreset lightPreset;
	}

	[SerializeField]
	private List<CustomObject> controlObjects = new List<CustomObject>();

	private CustomObject currentSelectedObj;

	[SerializeField]
	private int currentItemIdx = -1;

	public void EnableObject(CustomObject customObject)
	{
		if (customObject == null)
		{
			return;
		}
		foreach (CustomObject controlObject in controlObjects)
		{
			if (controlObject.go != null)
			{
				controlObject.go.SetActive(value: false);
			}
		}
		currentSelectedObj = customObject;
		if (currentSelectedObj.go != null)
		{
			currentSelectedObj.go.SetActive(value: true);
		}
		if (currentSelectedObj.lightPreset != null)
		{
			EnvironmentEngine.Instance.ApplyOverridePreset(currentSelectedObj.lightPreset);
		}
	}

	public void DestroyAllExceptCurrent()
	{
	}

	private void SwitchToPreviousObject()
	{
		if (--currentItemIdx < 0)
		{
			currentItemIdx = controlObjects.Count - 1;
		}
		EnableCurrentObject();
	}

	private void SwitchToNextObject()
	{
		if (++currentItemIdx > controlObjects.Count - 1)
		{
			currentItemIdx = 0;
		}
		EnableCurrentObject();
	}

	private void FinalizeSwitcher()
	{
	}

	private void EnableCurrentObject()
	{
		if (controlObjects.Count != 0)
		{
			currentItemIdx = Mathf.Clamp(currentItemIdx, 0, controlObjects.Count - 1);
			currentSelectedObj = controlObjects[currentItemIdx];
			EnableObject(currentSelectedObj);
		}
	}
}
