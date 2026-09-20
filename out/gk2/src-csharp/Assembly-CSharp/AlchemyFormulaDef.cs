using System;
using System.Collections.Generic;
using System.Text;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class AlchemyFormulaDef : BalanceBaseObject
{
	[AutoParse("runes_r")]
	public int runesRed;

	[AutoParse("runes_g")]
	public int runesGreen;

	[AutoParse("runes_b")]
	public int runesBlue;

	[AutoParse("tab")]
	public AlchemyFormulaTab tab;

	[AutoParse("hidden_at_start")]
	public bool hiddenAtStart;

	[AutoParse("crafts_in")]
	public List<string> craftsIn = new List<string>();

	[AutoParse("on_craft_end_expressions")]
	public List<LazyExpression> onCraftEndExpressions = new List<LazyExpression>();

	public ItemDef ItemDef => GameBalance.Me.GetData<ItemDef>(id);

	public Vector3Int GetRunesAsVector3Int()
	{
		return new Vector3Int(runesRed, runesGreen, runesBlue);
	}

	public string GetRunesAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (runesRed > 0)
		{
			stringBuilder.Append(string.Format("{0}{1}", "rune_r".FontIcon(), runesRed));
		}
		if (runesGreen > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(string.Format("{0}{1}", "rune_g".FontIcon(), runesGreen));
		}
		if (runesBlue > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(string.Format("{0}{1}", "rune_b".FontIcon(), runesBlue));
		}
		return stringBuilder.ToString();
	}
}
