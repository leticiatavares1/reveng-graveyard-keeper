using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomFontSettings : MonoBehaviour
{
	[Serializable]
	public enum CustomParameter
	{
		Color,
		EffectColor,
		TopAnchor,
		BottomAnchor,
		SimpleUITableOffset
	}

	[Serializable]
	public class CustomParameterSetting
	{
		public CustomParameter param;

		public Color color;

		public float v;
	}

	public List<CustomParameterSetting> pars = new List<CustomParameterSetting>();

	private List<CustomParameterSetting> _original_pars;

	private void CheckCache()
	{
		if (_original_pars != null)
		{
			return;
		}
		_original_pars = new List<CustomParameterSetting>();
		foreach (CustomParameterSetting par in pars)
		{
			_original_pars.Add(ReadCustomParam(par.param));
		}
	}

	public void Apply()
	{
		CheckCache();
		foreach (CustomParameterSetting par in pars)
		{
			SetCustomParam(par.param, par);
		}
	}

	public void Restore()
	{
		CheckCache();
		foreach (CustomParameterSetting original_par in _original_pars)
		{
			SetCustomParam(original_par.param, original_par);
		}
	}

	private void SetCustomParam(CustomParameter par, CustomParameterSetting data)
	{
		switch (par)
		{
		case CustomParameter.Color:
			GetComponent<UILabel>().color = data.color;
			break;
		case CustomParameter.EffectColor:
			GetComponent<UILabel>().effectColor = data.color;
			break;
		case CustomParameter.TopAnchor:
			GetComponent<UIWidget>().topAnchor.absolute = (int)data.v;
			break;
		case CustomParameter.BottomAnchor:
			GetComponent<UIWidget>().bottomAnchor.absolute = (int)data.v;
			break;
		case CustomParameter.SimpleUITableOffset:
			GetComponent<SimpleUITable>().offset = (int)data.v;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private CustomParameterSetting ReadCustomParam(CustomParameter par)
	{
		CustomParameterSetting customParameterSetting = new CustomParameterSetting
		{
			param = par
		};
		switch (par)
		{
		case CustomParameter.Color:
			customParameterSetting.color = GetComponent<UILabel>().color;
			break;
		case CustomParameter.EffectColor:
			customParameterSetting.color = GetComponent<UILabel>().effectColor;
			break;
		case CustomParameter.TopAnchor:
			customParameterSetting.v = GetComponent<UIWidget>().topAnchor.absolute;
			break;
		case CustomParameter.BottomAnchor:
			customParameterSetting.v = GetComponent<UIWidget>().bottomAnchor.absolute;
			break;
		case CustomParameter.SimpleUITableOffset:
			customParameterSetting.v = GetComponent<SimpleUITable>().offset;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return customParameterSetting;
	}
}
