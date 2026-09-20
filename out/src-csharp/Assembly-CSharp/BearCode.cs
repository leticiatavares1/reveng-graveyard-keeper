using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class BearCode
{
	[Serializable]
	private class BearCodeItem
	{
		public enum EquationType
		{
			Equal,
			Plus,
			Minus,
			Multiply,
			Divide
		}

		public enum CodeType
		{
			Equation,
			FunctionCall,
			Expression
		}

		public string var_name = "";

		public string value = "";

		public EquationType eq_type;

		public CodeType code_type;

		public string[] pars = new string[0];

		public new string ToString()
		{
			return var_name + " " + eq_type.ToString() + " " + value;
		}
	}

	public delegate void CallDelegate(string obj, string method_name, string[] pars);

	private List<BearCodeItem> _equations = new List<BearCodeItem>();

	private static readonly Regex _regex_eq = new Regex("^([^ =\\+\\*\\-\\/]+) *([\\+\\*\\-\\/]?=) *(.+)$");

	private static readonly Regex _regex_method = new Regex("^([^\\.]*?)\\.?([^\\.\\(\\)]+)\\((.*)\\)$");

	public BearCode()
	{
	}

	public BearCode(string code)
	{
		FromString(code);
	}

	private void FromString(string code)
	{
		string[] array = code.Split('\n');
		foreach (string text in array)
		{
			string text2 = text.Trim('\r', ' ', '\t').Replace("\r", " ").Replace("\t", " ")
				.Replace("  ", " ");
			if (string.IsNullOrEmpty(text2))
			{
				continue;
			}
			Match match = _regex_eq.Match(text2);
			if (match.Success)
			{
				string text3 = match.Groups[2].Captures[0].ToString();
				BearCodeItem.EquationType eq_type;
				switch (text3)
				{
				case "=":
					eq_type = BearCodeItem.EquationType.Equal;
					break;
				case "*=":
					eq_type = BearCodeItem.EquationType.Multiply;
					break;
				case "+=":
					eq_type = BearCodeItem.EquationType.Plus;
					break;
				case "-=":
					eq_type = BearCodeItem.EquationType.Minus;
					break;
				case "/=":
					eq_type = BearCodeItem.EquationType.Divide;
					break;
				default:
					Debug.LogError("Unknown sign '" + text3 + "' in: " + text);
					continue;
				}
				BearCodeItem bearCodeItem = new BearCodeItem
				{
					var_name = match.Groups[1].Captures[0].ToString(),
					eq_type = eq_type,
					value = match.Groups[3].Captures[0].ToString(),
					code_type = BearCodeItem.CodeType.Equation
				};
				if (string.IsNullOrEmpty(bearCodeItem.var_name))
				{
					Debug.LogError("Missing variable name: " + text);
				}
				else if (string.IsNullOrEmpty(bearCodeItem.value))
				{
					Debug.LogError("Missing value: " + text);
				}
				else
				{
					_equations.Add(bearCodeItem);
				}
				continue;
			}
			match = _regex_method.Match(text2);
			if (match.Success)
			{
				BearCodeItem bearCodeItem2 = new BearCodeItem();
				bearCodeItem2.var_name = match.Groups[1].Captures[0].ToString();
				bearCodeItem2.value = match.Groups[2].Captures[0].ToString();
				bearCodeItem2.code_type = BearCodeItem.CodeType.FunctionCall;
				bearCodeItem2.pars = match.Groups[3].Captures[0].ToString().Split(',');
				BearCodeItem bearCodeItem3 = bearCodeItem2;
				for (int j = 0; j < bearCodeItem3.pars.Length; j++)
				{
					bearCodeItem3.pars[j] = bearCodeItem3.pars[j].Trim(' ');
				}
				_equations.Add(bearCodeItem3);
			}
			else
			{
				BearCodeItem item = new BearCodeItem
				{
					value = text2,
					code_type = BearCodeItem.CodeType.Expression
				};
				_equations.Add(item);
			}
		}
	}

	public void Run(GameRes res, CallDelegate dlg, Dictionary<string, GameRes> objects = null)
	{
		foreach (BearCodeItem equation in _equations)
		{
			switch (equation.code_type)
			{
			case BearCodeItem.CodeType.Equation:
			{
				float num = res.Get(equation.var_name);
				if (float.TryParse(equation.value, out var result))
				{
					float num2 = result;
					switch (equation.eq_type)
					{
					case BearCodeItem.EquationType.Equal:
						num = num2;
						break;
					case BearCodeItem.EquationType.Minus:
						num -= num2;
						break;
					case BearCodeItem.EquationType.Plus:
						num += num2;
						break;
					case BearCodeItem.EquationType.Divide:
						num /= num2;
						break;
					case BearCodeItem.EquationType.Multiply:
						num *= num2;
						break;
					default:
						Debug.LogError("Not implemented: " + equation.eq_type);
						break;
					}
					if (equation.var_name.Contains("."))
					{
						string[] array = equation.var_name.Split('.');
						if (array.Length != 2)
						{
							Debug.LogError("Syntax error at variable name: " + equation.var_name);
						}
						else if (objects == null)
						{
							Debug.LogError("Objects is null");
						}
						else if (!objects.ContainsKey(array[0]))
						{
							Debug.LogError("Object \"" + array[0] + "\" not found!");
						}
						else
						{
							objects[array[0]].Set(array[1], num);
						}
					}
					else
					{
						res.Set(equation.var_name, num);
					}
				}
				else
				{
					Debug.LogError("Syntax error in: " + (object)equation);
				}
				break;
			}
			case BearCodeItem.CodeType.FunctionCall:
				if (dlg == null)
				{
					Debug.LogError("Delegate is null");
				}
				else
				{
					dlg(equation.var_name, equation.value, equation.pars);
				}
				break;
			}
		}
	}
}
