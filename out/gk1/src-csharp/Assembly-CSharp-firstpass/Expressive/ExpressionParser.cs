using System;
using System.Collections.Generic;
using System.Globalization;
using Expressive.Exceptions;
using Expressive.Expressions;
using Expressive.Functions;
using Expressive.Functions.Date;
using Expressive.Functions.Logical;
using Expressive.Functions.Mathematical;
using Expressive.Functions.Statistical;
using Expressive.Functions.String;
using Expressive.Operators;
using Expressive.Operators.Additive;
using Expressive.Operators.Bitwise;
using Expressive.Operators.Conditional;
using Expressive.Operators.Grouping;
using Expressive.Operators.Logic;
using Expressive.Operators.Multiplicative;
using Expressive.Operators.Relational;
using LinqTools;

namespace Expressive;

internal sealed class ExpressionParser
{
	private const char DateSeparator = '#';

	private const char ParameterSeparator = ',';

	private readonly char _decimalSeparator;

	private readonly ExpressiveOptions _options;

	private readonly IDictionary<string, Func<IExpression[], IDictionary<string, object>, object>> _registeredFunctions;

	private readonly IDictionary<string, IOperator> _registeredOperators;

	private readonly StringComparer _stringComparer;

	internal ExpressionParser(ExpressiveOptions options)
	{
		_options = options;
		_stringComparer = (_options.HasFlag(ExpressiveOptions.IgnoreCase) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
		_decimalSeparator = '.';
		_registeredFunctions = new Dictionary<string, Func<IExpression[], IDictionary<string, object>, object>>(GetDictionaryComparer(options));
		_registeredOperators = new Dictionary<string, IOperator>(GetDictionaryComparer(options));
		RegisterOperator(new PlusOperator());
		RegisterOperator(new SubtractOperator());
		RegisterOperator(new BitwiseAndOperator());
		RegisterOperator(new BitwiseOrOperator());
		RegisterOperator(new BitwiseXOrOperator());
		RegisterOperator(new LeftShiftOperator());
		RegisterOperator(new RightShiftOperator());
		RegisterOperator(new NullCoalescingOperator());
		RegisterOperator(new ParenthesisCloseOperator());
		RegisterOperator(new ParenthesisOpenOperator());
		RegisterOperator(new AndOperator());
		RegisterOperator(new NotOperator());
		RegisterOperator(new OrOperator());
		RegisterOperator(new DivideOperator());
		RegisterOperator(new ModulusOperator());
		RegisterOperator(new MultiplyOperator());
		RegisterOperator(new EqualOperator());
		RegisterOperator(new GreaterThanOperator());
		RegisterOperator(new GreaterThanOrEqualOperator());
		RegisterOperator(new LessThanOperator());
		RegisterOperator(new LessThanOrEqualOperator());
		RegisterOperator(new NotEqualOperator());
		RegisterFunction(new AddDaysFunction());
		RegisterFunction(new AddHoursFunction());
		RegisterFunction(new AddMillisecondsFunction());
		RegisterFunction(new AddMinutesFunction());
		RegisterFunction(new AddMonthsFunction());
		RegisterFunction(new AddSecondsFunction());
		RegisterFunction(new AddYearsFunction());
		RegisterFunction(new DayOfFunction());
		RegisterFunction(new DaysBetweenFunction());
		RegisterFunction(new HourOfFunction());
		RegisterFunction(new HoursBetweenFunction());
		RegisterFunction(new MillisecondOfFunction());
		RegisterFunction(new MillisecondsBetweenFunction());
		RegisterFunction(new MinuteOfFunction());
		RegisterFunction(new MinutesBetweenFunction());
		RegisterFunction(new MonthOfFunction());
		RegisterFunction(new SecondOfFunction());
		RegisterFunction(new SecondsBetweenFunction());
		RegisterFunction(new YearOfFunction());
		RegisterFunction(new AbsFunction());
		RegisterFunction(new AcosFunction());
		RegisterFunction(new AsinFunction());
		RegisterFunction(new AtanFunction());
		RegisterFunction(new CeilingFunction());
		RegisterFunction(new CosFunction());
		RegisterFunction(new CountFunction());
		RegisterFunction(new ExpFunction());
		RegisterFunction(new FloorFunction());
		RegisterFunction(new IEEERemainderFunction());
		RegisterFunction(new Log10Function());
		RegisterFunction(new LogFunction());
		RegisterFunction(new MaxFunction());
		RegisterFunction(new MinFunction());
		RegisterFunction(new PowFunction());
		RegisterFunction(new RoundFunction());
		RegisterFunction(new SignFunction());
		RegisterFunction(new SinFunction());
		RegisterFunction(new SqrtFunction());
		RegisterFunction(new SumFunction());
		RegisterFunction(new TanFunction());
		RegisterFunction(new TruncateFunction());
		RegisterFunction(new IfFunction());
		RegisterFunction(new InFunction());
		RegisterFunction(new AverageFunction());
		RegisterFunction(new MeanFunction());
		RegisterFunction(new MedianFunction());
		RegisterFunction(new ModeFunction());
		RegisterFunction(new LengthFunction());
		RegisterFunction(new PadLeftFunction());
		RegisterFunction(new PadRightFunction());
		RegisterFunction(new RegexFunction());
		RegisterFunction(new SubstringFunction());
	}

	internal IExpression CompileExpression(string expression, IList<string> variables)
	{
		if (expression.IsNullOrWhiteSpace())
		{
			throw new ExpressiveException("An Expression cannot be empty.");
		}
		IList<Token> list = Tokenise(expression);
		int num = list.Select((Token t) => t.CurrentToken).Count((string t) => string.Equals(t, "(", StringComparison.Ordinal));
		int num2 = list.Select((Token t) => t.CurrentToken).Count((string t) => string.Equals(t, ")", StringComparison.Ordinal));
		if (num > num2)
		{
			throw new ArgumentException("There aren't enough ')' symbols. Expected " + num + " but there is only " + num2);
		}
		if (num < num2)
		{
			throw new ArgumentException("There are too many ')' symbols. Expected " + num + " but there is " + num2);
		}
		return CompileExpression(new Queue<Token>(list), OperatorPrecedence.Minimum, variables, isWithinFunction: false);
	}

	internal void RegisterFunction(string functionName, Func<IExpression[], IDictionary<string, object>, object> function)
	{
		CheckForExistingFunctionName(functionName);
		_registeredFunctions.Add(functionName, function);
	}

	internal void RegisterFunction(IFunction function)
	{
		CheckForExistingFunctionName(function.Name);
		_registeredFunctions.Add(function.Name, delegate(IExpression[] p, IDictionary<string, object> a)
		{
			function.Variables = a;
			return function.Evaluate(p);
		});
	}

	internal void RegisterOperator(IOperator op)
	{
		string[] tags = op.Tags;
		foreach (string key in tags)
		{
			_registeredOperators.Add(key, op);
		}
	}

	internal void UnregisterFunction(string name)
	{
		if (_registeredFunctions.ContainsKey(name))
		{
			_registeredFunctions.Remove(name);
		}
	}

	private IExpression CompileExpression(Queue<Token> tokens, OperatorPrecedence minimumPrecedence, IList<string> variables, bool isWithinFunction)
	{
		if (tokens == null)
		{
			throw new ArgumentNullException("tokens", "You must call Tokenise before compiling");
		}
		IExpression expression = null;
		Token token = tokens.PeekOrDefault();
		Token previousToken = null;
		while (token != null)
		{
			Func<IExpression[], IDictionary<string, object>, object> value = null;
			IOperator value2 = null;
			if (_registeredOperators.TryGetValue(token.CurrentToken, out value2))
			{
				OperatorPrecedence precedence = value2.GetPrecedence(previousToken);
				if (precedence <= minimumPrecedence)
				{
					break;
				}
				tokens.Dequeue();
				if (!value2.CanGetCaptiveTokens(previousToken, token, tokens))
				{
					value2.GetCaptiveTokens(previousToken, token, tokens);
					break;
				}
				IExpression expression2 = null;
				Token[] captiveTokens = value2.GetCaptiveTokens(previousToken, token, tokens);
				if (captiveTokens.Length > 1)
				{
					Token[] innerCaptiveTokens = value2.GetInnerCaptiveTokens(captiveTokens);
					expression2 = CompileExpression(new Queue<Token>(innerCaptiveTokens), OperatorPrecedence.Minimum, variables, isWithinFunction);
					token = captiveTokens[captiveTokens.Length - 1];
				}
				else
				{
					expression2 = CompileExpression(tokens, precedence, variables, isWithinFunction);
					token = new Token(")", -1);
				}
				expression = value2.BuildExpression(previousToken, new IExpression[2] { expression, expression2 });
			}
			else if (_registeredFunctions.TryGetValue(token.CurrentToken, out value))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				List<IExpression> list = new List<IExpression>();
				Queue<Token> queue = new Queue<Token>();
				int num = 0;
				tokens.Dequeue();
				while (tokens.Count > 0)
				{
					Token token2 = tokens.Dequeue();
					if (string.Equals(token2.CurrentToken, "(", StringComparison.Ordinal))
					{
						num++;
					}
					else if (string.Equals(token2.CurrentToken, ")", StringComparison.Ordinal))
					{
						num--;
					}
					if ((num != 1 || !(token2.CurrentToken == "(")) && (num != 0 || !(token2.CurrentToken == ")")))
					{
						queue.Enqueue(token2);
					}
					if (num == 0 && queue.Any())
					{
						list.Add(CompileExpression(queue, OperatorPrecedence.Minimum, variables, isWithinFunction: true));
						queue.Clear();
					}
					else if (string.Equals(token2.CurrentToken, ','.ToString(), StringComparison.Ordinal) && num == 1)
					{
						list.Add(CompileExpression(queue, OperatorPrecedence.Minimum, variables, isWithinFunction: true));
						queue.Clear();
					}
					if (num <= 0)
					{
						break;
					}
				}
				expression = new FunctionExpression(token.CurrentToken, value, list.ToArray());
			}
			else if (token.CurrentToken.IsNumeric())
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				int result = 0;
				decimal result2 = 0.0m;
				double result3 = 0.0;
				float result4 = 0f;
				long result5 = 0L;
				if (int.TryParse(token.CurrentToken, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
				{
					expression = new ConstantValueExpression(ConstantValueExpressionType.Integer, result);
				}
				else if (decimal.TryParse(token.CurrentToken, NumberStyles.Any, CultureInfo.InvariantCulture, out result2))
				{
					expression = new ConstantValueExpression(ConstantValueExpressionType.Decimal, result2);
				}
				else if (double.TryParse(token.CurrentToken, NumberStyles.Any, CultureInfo.InvariantCulture, out result3))
				{
					expression = new ConstantValueExpression(ConstantValueExpressionType.Double, result3);
				}
				else if (float.TryParse(token.CurrentToken, NumberStyles.Any, CultureInfo.InvariantCulture, out result4))
				{
					expression = new ConstantValueExpression(ConstantValueExpressionType.Float, result4);
				}
				else if (long.TryParse(token.CurrentToken, NumberStyles.Any, CultureInfo.InvariantCulture, out result5))
				{
					expression = new ConstantValueExpression(ConstantValueExpressionType.Long, result5);
				}
			}
			else if (token.CurrentToken.StartsWith("[") && token.CurrentToken.EndsWith("]"))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				string text = token.CurrentToken.Replace("[", "").Replace("]", "");
				expression = new VariableExpression(text);
				if (!variables.Contains(text, _stringComparer))
				{
					variables.Add(text);
				}
			}
			else if (string.Equals(token.CurrentToken, "true", StringComparison.OrdinalIgnoreCase))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				expression = new ConstantValueExpression(ConstantValueExpressionType.Boolean, true);
			}
			else if (string.Equals(token.CurrentToken, "false", StringComparison.OrdinalIgnoreCase))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				expression = new ConstantValueExpression(ConstantValueExpressionType.Boolean, false);
			}
			else if (string.Equals(token.CurrentToken, "null", StringComparison.OrdinalIgnoreCase))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				expression = new ConstantValueExpression(ConstantValueExpressionType.Null, null);
			}
			else if (token.CurrentToken.StartsWith('#'.ToString()) && token.CurrentToken.EndsWith('#'.ToString()))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				string text2 = token.CurrentToken.Replace('#'.ToString(), "");
				DateTime result6 = DateTime.MinValue;
				if (!DateTime.TryParse(text2, out result6))
				{
					if (string.Equals("TODAY", text2, StringComparison.OrdinalIgnoreCase))
					{
						result6 = DateTime.Today;
					}
					else
					{
						if (!string.Equals("NOW", text2, StringComparison.OrdinalIgnoreCase))
						{
							throw new UnrecognisedTokenException(text2);
						}
						result6 = DateTime.Now;
					}
				}
				expression = new ConstantValueExpression(ConstantValueExpressionType.DateTime, result6);
			}
			else if ((token.CurrentToken.StartsWith("'") && token.CurrentToken.EndsWith("'")) || (token.CurrentToken.StartsWith("\"") && token.CurrentToken.EndsWith("\"")))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				expression = new ConstantValueExpression(ConstantValueExpressionType.String, CleanString(token.CurrentToken.Substring(1, token.Length - 2)));
			}
			else
			{
				if (!string.Equals(token.CurrentToken, ','.ToString(), StringComparison.Ordinal))
				{
					tokens.Dequeue();
					throw new UnrecognisedTokenException(token.CurrentToken);
				}
				if (!isWithinFunction)
				{
					throw new ExpressiveException($"Unexpected token '{token}'");
				}
				tokens.Dequeue();
			}
			previousToken = token;
			token = tokens.PeekOrDefault();
		}
		return expression;
	}

	private static string CleanString(string input)
	{
		if (input.Length <= 1)
		{
			return input;
		}
		char[] array = new char[input.Length];
		int length = 0;
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (c == '\\' && i < input.Length - 1)
			{
				switch (input[i + 1])
				{
				case 'n':
					array[length++] = '\n';
					i++;
					continue;
				case 'r':
					array[length++] = '\r';
					i++;
					continue;
				case 't':
					array[length++] = '\t';
					i++;
					continue;
				case '\'':
					array[length++] = '\'';
					i++;
					continue;
				}
			}
			array[length++] = c;
		}
		return new string(array, 0, length);
	}

	private static bool CanExtractValue(string expression, int expressionLength, int index, string value)
	{
		return string.Equals(value, ExtractValue(expression, expressionLength, index, value), StringComparison.OrdinalIgnoreCase);
	}

	private static bool CanGetString(string expression, int startIndex, char quoteCharacter)
	{
		return !GetString(expression, startIndex, quoteCharacter).IsNullOrWhiteSpace();
	}

	private void CheckForExistingFunctionName(string functionName)
	{
		if (_registeredFunctions.ContainsKey(functionName))
		{
			throw new FunctionNameAlreadyRegisteredException(functionName);
		}
	}

	private void CheckForExistingParticipant(IExpression participant, Token token, bool isWithinFunction)
	{
		if (participant != null)
		{
			if (isWithinFunction)
			{
				throw new MissingTokenException("Missing token, expecting ','.", ',');
			}
			throw new ExpressiveException($"Unexpected token '{token.CurrentToken}' at index {token.StartIndex}");
		}
	}

	private static bool CheckForTag(string tag, string lookAhead, ExpressiveOptions options)
	{
		if (!options.HasFlag(ExpressiveOptions.IgnoreCase) || !string.Equals(lookAhead, tag, StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(lookAhead, tag, StringComparison.Ordinal);
		}
		return true;
	}

	private static string ExtractValue(string expression, int expressionLength, int index, string expectedValue)
	{
		string result = null;
		int length = expectedValue.Length;
		if (expressionLength >= index + length)
		{
			string text = expression.Substring(index, length);
			bool flag = true;
			if (expressionLength > index + length)
			{
				flag = !char.IsLetterOrDigit(expression[index + length]);
			}
			if (string.Equals(text, expectedValue, StringComparison.OrdinalIgnoreCase) && flag)
			{
				result = text;
			}
		}
		return result;
	}

	private StringComparer GetDictionaryComparer(ExpressiveOptions options)
	{
		if (!options.HasFlag(ExpressiveOptions.IgnoreCase))
		{
			return StringComparer.Ordinal;
		}
		return StringComparer.OrdinalIgnoreCase;
	}

	private string GetNumber(string expression, int startIndex)
	{
		bool flag = false;
		int num = startIndex;
		char c = expression[num];
		while (num < expression.Length && (char.IsDigit(c) || (!flag && c == _decimalSeparator)))
		{
			if (!flag && c == _decimalSeparator)
			{
				flag = true;
			}
			num++;
			if (num == expression.Length)
			{
				break;
			}
			c = expression[num];
		}
		return expression.Substring(startIndex, num - startIndex);
	}

	private static string GetString(string expression, int startIndex, char quoteCharacter)
	{
		int num = startIndex;
		bool flag = false;
		char c = expression[num];
		char c2 = '\0';
		while (num < expression.Length && !flag)
		{
			if (num != startIndex && c == quoteCharacter && c2 != '\\')
			{
				flag = true;
			}
			c2 = c;
			num++;
			if (num == expression.Length)
			{
				break;
			}
			c = expression[num];
		}
		if (flag)
		{
			return expression.Substring(startIndex, num - startIndex);
		}
		return null;
	}

	private static bool IsQuote(char character)
	{
		if (character != '\'')
		{
			return character == '"';
		}
		return true;
	}

	private IList<Token> Tokenise(string expression)
	{
		if (expression.IsNullOrWhiteSpace())
		{
			return null;
		}
		int length = expression.Length;
		IOrderedEnumerable<KeyValuePair<string, IOperator>, int> orderedEnumerable = _registeredOperators.OrderByDescending((KeyValuePair<string, IOperator> op) => op.Key.Length);
		List<Token> list = new List<Token>();
		IList<char> list2 = null;
		int i;
		int num;
		for (i = 0; i < length; i += ((num == 0) ? 1 : num))
		{
			num = 0;
			bool flag = false;
			foreach (KeyValuePair<string, Func<IExpression[], IDictionary<string, object>, object>> registeredFunction in _registeredFunctions)
			{
				string text = expression.Substring(i, Math.Min(registeredFunction.Key.Length, length - i));
				if (CheckForTag(registeredFunction.Key, text, _options))
				{
					CheckForUnrecognised(list2, list, i);
					num = registeredFunction.Key.Length;
					list.Add(new Token(text, i));
					break;
				}
			}
			if (num == 0)
			{
				foreach (KeyValuePair<string, IOperator> item in orderedEnumerable)
				{
					string text2 = expression.Substring(i, Math.Min(item.Key.Length, length - i));
					if (CheckForTag(item.Key, text2, _options))
					{
						CheckForUnrecognised(list2, list, i);
						num = item.Key.Length;
						list.Add(new Token(text2, i));
						break;
					}
				}
			}
			if (num == 0)
			{
				char c = expression[i];
				if (c == '[')
				{
					char c2 = ']';
					if (!CanGetString(expression, i, c2))
					{
						throw new MissingTokenException($"Missing closing token '{c2}'", c2);
					}
					string text3 = expression.SubstringUpTo(i, c2);
					CheckForUnrecognised(list2, list, i);
					list.Add(new Token(text3, i));
					num = text3.Length;
				}
				else if (char.IsDigit(c))
				{
					string number = GetNumber(expression, i);
					CheckForUnrecognised(list2, list, i);
					list.Add(new Token(number, i));
					num = number.Length;
				}
				else if (IsQuote(c))
				{
					if (!CanGetString(expression, i, c))
					{
						throw new MissingTokenException($"Missing closing token '{c}'", c);
					}
					string @string = GetString(expression, i, c);
					CheckForUnrecognised(list2, list, i);
					list.Add(new Token(@string, i));
					num = @string.Length;
				}
				else if (c == '#')
				{
					if (!CanGetString(expression, i, c))
					{
						throw new MissingTokenException($"Missing closing token '{c}'", c);
					}
					string text4 = "#" + expression.SubstringUpTo(i + 1, '#');
					CheckForUnrecognised(list2, list, i);
					list.Add(new Token(text4, i));
					num = text4.Length;
				}
				else if (c == ',')
				{
					CheckForUnrecognised(list2, list, i);
					list.Add(new Token(c.ToString(), i));
					num = 1;
				}
				else if ((c == 't' || c == 'T') && CanExtractValue(expression, length, i, "true"))
				{
					CheckForUnrecognised(list2, list, i);
					string text5 = ExtractValue(expression, length, i, "true");
					if (!text5.IsNullOrWhiteSpace())
					{
						list.Add(new Token(text5, i));
						num = 4;
					}
				}
				else if ((c == 'f' || c == 'F') && CanExtractValue(expression, length, i, "false"))
				{
					CheckForUnrecognised(list2, list, i);
					string text6 = ExtractValue(expression, length, i, "false");
					if (!text6.IsNullOrWhiteSpace())
					{
						list.Add(new Token(text6, i));
						num = 5;
					}
				}
				else if (c == 'n' && CanExtractValue(expression, length, i, "null"))
				{
					CheckForUnrecognised(list2, list, i);
					string text7 = ExtractValue(expression, length, i, "null");
					if (!text7.IsNullOrWhiteSpace())
					{
						list.Add(new Token(text7, i));
						num = 4;
					}
				}
				else if (!char.IsWhiteSpace(c))
				{
					if (list2 == null)
					{
						list2 = new List<char>();
					}
					flag = true;
					list2.Add(c);
				}
			}
			if (!flag)
			{
				CheckForUnrecognised(list2, list, i);
				list2 = null;
			}
		}
		CheckForUnrecognised(list2, list, i);
		return list;
	}

	private static void CheckForUnrecognised(IList<char> unrecognised, IList<Token> tokens, int index)
	{
		if (unrecognised != null)
		{
			string text = new string(unrecognised.ToArray());
			tokens.Add(new Token(text, index - text.Length));
		}
	}
}
