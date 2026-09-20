using System;
using System.Globalization;
using Expressive;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public abstract class LazyExpressionBase
{
	[SerializeField]
	protected string expressionString = string.Empty;

	[SerializeField]
	protected string expressionStringUnparsed = string.Empty;

	[SerializeField]
	protected PureValueType pureValueType;

	[SerializeField]
	protected float pureValueFloat;

	[SerializeField]
	protected bool pureValueBool;

	[NonSerialized]
	protected Expression expression;

	public bool HasExpression => !string.IsNullOrEmpty(expressionString);

	public bool HasPureValue => pureValueType != PureValueType.None;

	public void FromString(string str)
	{
		FromString(str, PureValueType.None);
	}

	public void FromString(string str, PureValueType expectedPureType)
	{
		str = str.Replace("&quot;", "\"");
		expressionString = str;
		expression = null;
		pureValueType = PureValueType.None;
		bool flag = float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out pureValueFloat);
		if (expectedPureType == PureValueType.None && flag)
		{
			pureValueType = PureValueType.Float;
		}
		else
		{
			if (expectedPureType == PureValueType.None || HasExpressionSymptom(str))
			{
				return;
			}
			pureValueType = expectedPureType;
			switch (expectedPureType)
			{
			case PureValueType.Bool:
				if (!bool.TryParse(str, out pureValueBool))
				{
					pureValueBool = str == "1" || (flag && pureValueFloat != 0f);
				}
				break;
			case PureValueType.Float:
			case PureValueType.String:
				break;
			}
		}
	}

	protected virtual bool HasExpressionSymptom(string strToCheck)
	{
		if (string.IsNullOrEmpty(strToCheck))
		{
			return false;
		}
		if (!strToCheck.Contains('(') && !strToCheck.Contains('"') && !strToCheck.Contains('+') && !strToCheck.Contains('-') && !strToCheck.Contains('/'))
		{
			return strToCheck.Contains('*');
		}
		return true;
	}

	public string GetRawExpressionString()
	{
		return expressionString;
	}

	public void ValidateExpression(string errorMessagePrefix)
	{
		CheckExpressionInit();
		try
		{
			if (expression != null)
			{
				_ = expression.ReferencedVariables;
			}
		}
		catch (Exception e)
		{
			HandleEvaluateError(e, errorMessagePrefix);
		}
	}

	public override string ToString()
	{
		return expressionString.ToString();
	}

	public string ToUnparsedString()
	{
		return expressionStringUnparsed;
	}

	protected virtual void CheckExpressionInit()
	{
		throw new NotImplementedException();
	}

	protected virtual void HandleEvaluateError(Exception e, string errorMessagePrefix = null)
	{
		string text = $"Error in expression {expression} ({expressionString}): {e}";
		if (!string.IsNullOrEmpty(errorMessagePrefix))
		{
			text = text.Insert(0, errorMessagePrefix + "\n");
		}
		Debug.LogError(text);
	}

	protected virtual string ParseRegex(string input)
	{
		return input;
	}

	public static T ParseExpression<T>(string expressionString) where T : LazyExpressionBase, new()
	{
		return ParseExpression<T>(expressionString, PureValueType.None);
	}

	public static T ParseExpression<T>(string expressionString, PureValueType pureValueType) where T : LazyExpressionBase, new()
	{
		expressionString = expressionString.Trim().Replace("&#xd", "").Replace("&#xD", "");
		if (string.IsNullOrEmpty(expressionString))
		{
			return null;
		}
		string cyrillicValidationError = GetCyrillicValidationError(expressionString);
		if (!string.IsNullOrEmpty(cyrillicValidationError))
		{
			Debug.LogError(cyrillicValidationError);
		}
		T obj = new T
		{
			expressionStringUnparsed = expressionString
		};
		expressionString = obj.ParseRegex(expressionString);
		obj.FromString(expressionString, pureValueType);
		return obj;
	}

	public static string GetCyrillicValidationError(string value)
	{
		if (string.IsNullOrEmpty(value) || !TryGetCyrillicChar(value, out var cyrillicChar, out var index))
		{
			return string.Empty;
		}
		object[] obj = new object[4] { value, cyrillicChar, null, null };
		int num = cyrillicChar;
		obj[2] = num.ToString("X4");
		obj[3] = index;
		return string.Format("Cyrillic characters are not allowed in LazyExpression [{0}]: '{1}' (U+{2}) at index {3}", obj);
	}

	private static bool TryGetCyrillicChar(string value, out char cyrillicChar, out int index)
	{
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (IsCyrillic(c))
			{
				cyrillicChar = c;
				index = i;
				return true;
			}
		}
		cyrillicChar = '\0';
		index = -1;
		return false;
	}

	private static bool IsCyrillic(char c)
	{
		if ((c < 'Ѐ' || c > 'ӿ') && (c < 'Ԁ' || c > 'ԯ') && (c < '\u2de0' || c > '\u2dff') && (c < 'Ꙁ' || c > '\ua69f'))
		{
			if (c >= 'ᲀ')
			{
				return c <= '\u1c8f';
			}
			return false;
		}
		return true;
	}
}
