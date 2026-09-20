using System;
using System.Text;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public static class Param
{
	public static string FormIconFromPlayerRes(string param)
	{
		return param.FontIcon() + MainGame.PlayerData.GetRes(param);
	}

	public static string ToFormattedString(this GameRes gameRes, bool showOnlyType = false, Func<string, string, string> overrodePattern = null, bool ignoreZeroValues = false, bool appendSpace = true, GameResIconType iconType = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (GameResAtom item in gameRes.List)
		{
			stringBuilder.Append(item.ToFormattedString(showOnlyType, overrodePattern, ignoreZeroValues, appendSpace, iconType));
		}
		return stringBuilder.ToString();
	}

	public static string ToFormattedString(this GameResAtom atom, bool showOnlyType = false, Func<string, string, string> overrodePattern = null, bool ignoreZeroValues = false, bool appendSpace = true, GameResIconType iconType = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if ((object)iconType == null)
		{
			iconType = GameResIconType.Common;
		}
		if (atom.type == "money")
		{
			stringBuilder.Append(Trading.FormatMoney(Mathf.CeilToInt(atom.value), printZero: false, " ", iconType));
			return stringBuilder.ToString();
		}
		int num = (int)atom.value;
		if (num == 0 && !showOnlyType && !ignoreZeroValues)
		{
			return string.Empty;
		}
		if (stringBuilder.Length > 0 && appendSpace)
		{
			stringBuilder.Append(" ");
		}
		GameResIconConfig configForRes = GameResDisplayConfig.GetConfigForRes(atom.type, iconType);
		string text;
		if (configForRes != null)
		{
			text = configForRes.iconName.FontIcon();
		}
		else if (!atom.type.StartsWith("wz_"))
		{
			text = ((!atom.type.EndsWith("_REP")) ? atom.type.FontIcon() : "icon_smile02".FontIcon());
		}
		else
		{
			string id = atom.type.Replace("wz_", "");
			text = MainGame.Instance.GameSave.worldData.GetWorldZoneDataById(id)?.Definition.qualityIcon.FontIcon();
		}
		if (overrodePattern == null)
		{
			stringBuilder.Append(text);
			if (!showOnlyType)
			{
				stringBuilder.Append((num >= 0) ? "+" : "-");
				stringBuilder.Append(Mathf.Abs(num));
			}
		}
		else
		{
			string value = overrodePattern(text, num.ToString());
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	public static Item ItemFromAtom(this GameResAtom atom)
	{
		return new Item("game_res_" + atom.type, (int)atom.value);
	}
}
