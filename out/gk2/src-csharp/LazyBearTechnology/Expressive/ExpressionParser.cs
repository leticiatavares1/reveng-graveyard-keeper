using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Expressive.Exceptions;
using Expressive.Expressions;
using Expressive.Operators;
using Expressive.Tokenisation;

namespace Expressive;

internal sealed class ExpressionParser
{
	private readonly Context context;

	private readonly Tokeniser tokeniser;

	internal ExpressionParser(Context context)
	{
		this.context = context;
		tokeniser = new Tokeniser(this.context, new List<ITokenExtractor>
		{
			new KeywordTokenExtractor(this.context.FunctionNames),
			new KeywordTokenExtractor(this.context.OperatorNames),
			new ParenthesisedTokenExtractor('[', ']'),
			new NumericTokenExtractor(),
			new ParenthesisedTokenExtractor('#'),
			new ValueTokenExtractor(","),
			new ParenthesisedTokenExtractor('"'),
			new ParenthesisedTokenExtractor('\''),
			new ValueTokenExtractor("true"),
			new ValueTokenExtractor("TRUE"),
			new ValueTokenExtractor("false"),
			new ValueTokenExtractor("FALSE"),
			new ValueTokenExtractor("null"),
			new ValueTokenExtractor("NULL")
		});
	}

	internal IExpression CompileExpression(string expression, IList<string> variables)
	{
		if (string.IsNullOrWhiteSpace(expression))
		{
			throw new ExpressiveException("An Expression cannot be empty.");
		}
		IList<Token> list = tokeniser.Tokenise(expression);
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
			Func<IExpression[], IDictionary<string, object>, object> value2;
			if (context.TryGetOperator(token.CurrentToken, out var value))
			{
				OperatorPrecedence precedence = value.GetPrecedence(previousToken);
				if (precedence <= minimumPrecedence)
				{
					break;
				}
				tokens.Dequeue();
				if (!value.CanGetCaptiveTokens(previousToken, token, tokens))
				{
					value.GetCaptiveTokens(previousToken, token, tokens);
					break;
				}
				IExpression expression2 = null;
				Token[] captiveTokens = value.GetCaptiveTokens(previousToken, token, tokens);
				if (captiveTokens.Length > 1)
				{
					Token[] innerCaptiveTokens = value.GetInnerCaptiveTokens(captiveTokens);
					expression2 = CompileExpression(new Queue<Token>(innerCaptiveTokens), OperatorPrecedence.Minimum, variables, isWithinFunction);
					token = captiveTokens[^1];
				}
				else
				{
					expression2 = CompileExpression(tokens, precedence, variables, isWithinFunction);
					token = new Token(")", -1);
				}
				expression = value.BuildExpression(previousToken, new IExpression[2] { expression, expression2 }, context);
			}
			else if (context.TryGetFunction(token.CurrentToken, out value2))
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
				expression = new FunctionExpression(token.CurrentToken, value2, list.ToArray());
			}
			else if (token.CurrentToken.IsNumeric(context.DecimalCurrentCulture))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				decimal result2;
				double result3;
				float result4;
				long result5;
				if (int.TryParse(token.CurrentToken, NumberStyles.Any, context.DecimalCurrentCulture, out var result))
				{
					expression = new ConstantValueExpression(result);
				}
				else if (decimal.TryParse(token.CurrentToken, NumberStyles.Any, context.DecimalCurrentCulture, out result2))
				{
					expression = new ConstantValueExpression(result2);
				}
				else if (double.TryParse(token.CurrentToken, NumberStyles.Any, context.DecimalCurrentCulture, out result3))
				{
					expression = new ConstantValueExpression(result3);
				}
				else if (float.TryParse(token.CurrentToken, NumberStyles.Any, context.DecimalCurrentCulture, out result4))
				{
					expression = new ConstantValueExpression(result4);
				}
				else if (long.TryParse(token.CurrentToken, NumberStyles.Any, context.DecimalCurrentCulture, out result5))
				{
					expression = new ConstantValueExpression(result5);
				}
			}
			else if (token.CurrentToken.StartsWith("[") && token.CurrentToken.EndsWith("]"))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				string text = token.CurrentToken.Replace("[", "").Replace("]", "");
				expression = new VariableExpression(text);
				if (!variables.Contains(text, context.ParsingStringComparer))
				{
					variables.Add(text);
				}
			}
			else if (string.Equals(token.CurrentToken, "true", StringComparison.OrdinalIgnoreCase))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				expression = new ConstantValueExpression(true);
			}
			else if (string.Equals(token.CurrentToken, "false", StringComparison.OrdinalIgnoreCase))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				expression = new ConstantValueExpression(false);
			}
			else if (string.Equals(token.CurrentToken, "null", StringComparison.OrdinalIgnoreCase))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				expression = new ConstantValueExpression(null);
			}
			else if (token.CurrentToken.StartsWith('#'.ToString()) && token.CurrentToken.EndsWith('#'.ToString()))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				string text2 = token.CurrentToken.Replace('#'.ToString(), "");
				if (!DateTime.TryParse(text2, out var result6))
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
				expression = new ConstantValueExpression(result6);
			}
			else if ((token.CurrentToken.StartsWith("'") && token.CurrentToken.EndsWith("'")) || (token.CurrentToken.StartsWith("\"") && token.CurrentToken.EndsWith("\"")))
			{
				CheckForExistingParticipant(expression, token, isWithinFunction);
				tokens.Dequeue();
				expression = new ConstantValueExpression(CleanString(token.CurrentToken.Substring(1, token.Length - 2)));
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
				case '"':
					array[length++] = '"';
					i++;
					continue;
				case '\\':
					array[length++] = '\\';
					i++;
					continue;
				}
			}
			array[length++] = c;
		}
		return new string(array, 0, length);
	}

	private static void CheckForExistingParticipant(IExpression participant, Token token, bool isWithinFunction)
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
}
