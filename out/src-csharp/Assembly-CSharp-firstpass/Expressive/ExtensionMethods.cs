using System;
using System.Collections.Generic;
using System.Globalization;

namespace Expressive;

internal static class ExtensionMethods
{
	internal static bool IsArithmeticOperator(this string source)
	{
		if (!string.Equals(source, "+", StringComparison.Ordinal) && !string.Equals(source, "-", StringComparison.Ordinal) && !string.Equals(source, "−", StringComparison.Ordinal) && !string.Equals(source, "/", StringComparison.Ordinal) && !string.Equals(source, "÷", StringComparison.Ordinal) && !string.Equals(source, "*", StringComparison.Ordinal) && !string.Equals(source, "×", StringComparison.Ordinal) && !string.Equals(source, "+", StringComparison.Ordinal) && !string.Equals(source, "+", StringComparison.Ordinal))
		{
			return string.Equals(source, "+", StringComparison.Ordinal);
		}
		return true;
	}

	internal static bool IsNumeric(this string source)
	{
		double result;
		return double.TryParse(source, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
	}

	internal static T PeekOrDefault<T>(this Queue<T> queue)
	{
		if (queue.Count <= 0)
		{
			return default(T);
		}
		return queue.Peek();
	}

	internal static string SubstringUpTo(this string source, int startIndex, char character)
	{
		if (startIndex != 0)
		{
			string text = source.Substring(startIndex);
			return text.Substring(0, text.IndexOf(character) + 1);
		}
		return source.Substring(startIndex, source.IndexOf(character) + 1);
	}
}
