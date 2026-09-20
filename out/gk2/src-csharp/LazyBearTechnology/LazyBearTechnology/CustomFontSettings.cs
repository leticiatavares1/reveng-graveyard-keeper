using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LazyBearTechnology;

public class CustomFontSettings : MonoBehaviour
{
	[Serializable]
	public enum CustomParamType
	{
		Color
	}

	[Serializable]
	public class CustomParameterSetting
	{
		public CustomParamType pType;

		public Color color;

		public float value;
	}

	public List<CustomParameterSetting> customParams = new List<CustomParameterSetting>();

	private List<CustomParameterSetting> originalParams;

	private void CheckOriginalParamsCached()
	{
		if (originalParams != null)
		{
			return;
		}
		originalParams = new List<CustomParameterSetting>();
		foreach (CustomParameterSetting customParam in customParams)
		{
			originalParams.Add(ReadCustomParam(customParam.pType));
		}
	}

	public void Apply()
	{
		CheckOriginalParamsCached();
		foreach (CustomParameterSetting customParam in customParams)
		{
			SetCustomParam(customParam.pType, customParam);
		}
	}

	public void Restore()
	{
		CheckOriginalParamsCached();
		foreach (CustomParameterSetting originalParam in originalParams)
		{
			SetCustomParam(originalParam.pType, originalParam);
		}
	}

	private void SetCustomParam(CustomParamType par, CustomParameterSetting data)
	{
		if (par == CustomParamType.Color)
		{
			GetComponent<TextMeshProUGUI>().color = data.color;
			return;
		}
		throw new ArgumentOutOfRangeException();
	}

	private CustomParameterSetting ReadCustomParam(CustomParamType param)
	{
		CustomParameterSetting customParameterSetting = new CustomParameterSetting
		{
			pType = param
		};
		if (param == CustomParamType.Color)
		{
			customParameterSetting.color = GetComponent<TextMeshProUGUI>().color;
			return customParameterSetting;
		}
		throw new ArgumentOutOfRangeException();
	}
}
