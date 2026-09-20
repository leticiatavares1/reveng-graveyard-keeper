using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BearLogicExpression
{
	[Serializable]
	private class BearLogicExpressionPart
	{
		public enum Target
		{
			None,
			Object,
			Subject
		}

		public string expr = "";

		public Target target;
	}

	[SerializeField]
	private List<BearLogicExpressionPart> _s = new List<BearLogicExpressionPart>();

	public bool Evaluate(GameRes globals, GameRes obj, GameRes subj)
	{
		bool flag = true;
		string text = "";
		string text2 = "";
		foreach (BearLogicExpressionPart item in _s)
		{
			if (string.IsNullOrEmpty(item.expr))
			{
				continue;
			}
			text2 += item.expr;
			if (item.expr.Contains("&") || item.expr.Contains("|"))
			{
				if (text != "")
				{
					Debug.LogError("Error evaluating logical expression: " + text2);
					return false;
				}
				text = item.expr;
				continue;
			}
			bool flag2 = (item.target switch
			{
				BearLogicExpressionPart.Target.Object => obj, 
				BearLogicExpressionPart.Target.Subject => subj, 
				_ => globals, 
			}).Get(item.expr) > 0f;
			if (text != null)
			{
				if (text == null || text.Length != 0)
				{
					switch (text)
					{
					case " ":
						break;
					case "&":
					case "&&":
						flag = flag && flag2;
						goto IL_013f;
					case "|":
					case "||":
						flag = flag || flag2;
						goto IL_013f;
					default:
						goto IL_013f;
					}
				}
				flag = flag2;
			}
			goto IL_013f;
			IL_013f:
			text = "";
		}
		Debug.Log("Eval logic expr : " + text2 + " = " + flag);
		return flag;
	}
}
