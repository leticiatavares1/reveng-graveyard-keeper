using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Expressive.Exceptions;
using Expressive.Expressions;
using Expressive.Functions;
using Expressive.Functions.Conversion;
using Expressive.Functions.Date;
using Expressive.Functions.Logical;
using Expressive.Functions.Mathematical;
using Expressive.Functions.Relational;
using Expressive.Functions.Statistical;
using Expressive.Functions.String;
using Expressive.Operators;
using Expressive.Operators.Additive;
using Expressive.Operators.Bitwise;
using Expressive.Operators.Conditional;
using Expressive.Operators.Grouping;
using Expressive.Operators.Logical;
using Expressive.Operators.Multiplicative;
using Expressive.Operators.Relational;

namespace Expressive;

public class Context
{
	internal const char DateSeparator = '#';

	internal const char ParameterSeparator = ',';

	private readonly IDictionary<string, Func<IExpression[], IDictionary<string, object>, object>> registeredFunctions;

	private readonly IDictionary<string, IOperator> registeredOperators;

	internal ExpressiveOptions Options { get; }

	internal CultureInfo CurrentCulture { get; }

	internal CultureInfo DecimalCurrentCulture { get; }

	internal char DecimalSeparator { get; }

	internal IEnumerable<string> FunctionNames => registeredFunctions.Keys.OrderByDescending((string k) => k.Length);

	internal IEnumerable<string> OperatorNames => registeredOperators.Keys.OrderByDescending((string k) => k.Length);

	private bool IsCaseInsensitiveEqualityEnabled
	{
		get
		{
			if (!Options.HasFlag(ExpressiveOptions.IgnoreCase))
			{
				return Options.HasFlag(ExpressiveOptions.IgnoreCaseForEquality);
			}
			return true;
		}
	}

	internal StringComparison EqualityStringComparison
	{
		get
		{
			if (!IsCaseInsensitiveEqualityEnabled)
			{
				return StringComparison.Ordinal;
			}
			return StringComparison.OrdinalIgnoreCase;
		}
	}

	internal bool IsCaseInsensitiveParsingEnabled
	{
		get
		{
			if (!Options.HasFlag(ExpressiveOptions.IgnoreCase))
			{
				return Options.HasFlag(ExpressiveOptions.IgnoreCaseForParsing);
			}
			return true;
		}
	}

	internal IEqualityComparer<string> ParsingStringComparer
	{
		get
		{
			if (!IsCaseInsensitiveParsingEnabled)
			{
				return EqualityComparer<string>.Default;
			}
			return StringComparer.OrdinalIgnoreCase;
		}
	}

	internal StringComparison ParsingStringComparison
	{
		get
		{
			if (!IsCaseInsensitiveParsingEnabled)
			{
				return StringComparison.Ordinal;
			}
			return StringComparison.OrdinalIgnoreCase;
		}
	}

	public Context(ExpressiveOptions options)
		: this(options, CultureInfo.CurrentCulture, CultureInfo.InvariantCulture)
	{
	}

	public Context(ExpressiveOptions options, CultureInfo mainCurrentCulture, CultureInfo decimalCurrentCulture)
	{
		Options = options;
		CurrentCulture = mainCurrentCulture ?? throw new ArgumentNullException("mainCurrentCulture");
		DecimalCurrentCulture = decimalCurrentCulture ?? throw new ArgumentNullException("decimalCurrentCulture");
		DecimalSeparator = Convert.ToChar(DecimalCurrentCulture.NumberFormat.NumberDecimalSeparator, DecimalCurrentCulture);
		registeredFunctions = new Dictionary<string, Func<IExpression[], IDictionary<string, object>, object>>(ParsingStringComparer);
		registeredOperators = new Dictionary<string, IOperator>(ParsingStringComparer);
		RegisterOperator(new PlusOperator());
		RegisterOperator(new SubtractOperator());
		RegisterOperator(new BitwiseAndOperator());
		RegisterOperator(new BitwiseOrOperator());
		RegisterOperator(new BitwiseExclusiveOrOperator());
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
		RegisterFunction(new DateFunction());
		RegisterFunction(new DecimalFunction());
		RegisterFunction(new DoubleFunction());
		RegisterFunction(new IntegerFunction());
		RegisterFunction(new LongFunction());
		RegisterFunction(new StringFunction());
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
		RegisterFunction(new PowFunction());
		RegisterFunction(new RandomFunction());
		RegisterFunction(new RoundFunction());
		RegisterFunction(new SignFunction());
		RegisterFunction(new SinFunction());
		RegisterFunction(new SqrtFunction());
		RegisterFunction(new SumFunction());
		RegisterFunction(new TanFunction());
		RegisterFunction(new TruncateFunction());
		RegisterFunction(new EFunction());
		RegisterFunction(new PIFunction());
		RegisterFunction(new IfFunction());
		RegisterFunction(new InFunction());
		RegisterFunction(new MaxFunction());
		RegisterFunction(new MinFunction());
		RegisterFunction(new AverageFunction());
		RegisterFunction(new MeanFunction());
		RegisterFunction(new MedianFunction());
		RegisterFunction(new ModeFunction());
		RegisterFunction(new ContainsFunction());
		RegisterFunction(new EndsWithFunction());
		RegisterFunction(new LengthFunction());
		RegisterFunction(new PadLeftFunction());
		RegisterFunction(new PadRightFunction());
		RegisterFunction(new RegexFunction());
		RegisterFunction(new StartsWithFunction());
		RegisterFunction(new SubstringFunction());
		RegisterFunction(new ConcatFunction());
		RegisterFunction(new IndexOfFunction());
	}

	public Context(Context context)
	{
		registeredFunctions = new Dictionary<string, Func<IExpression[], IDictionary<string, object>, object>>(context.registeredFunctions);
		registeredOperators = new Dictionary<string, IOperator>(context.registeredOperators);
		Options = context.Options;
		CurrentCulture = context.CurrentCulture;
		DecimalCurrentCulture = context.DecimalCurrentCulture;
		DecimalSeparator = context.DecimalSeparator;
	}

	public void RegisterFunction(string functionName, Func<IExpression[], IDictionary<string, object>, object> function, bool force = false)
	{
		CheckForExistingFunctionName(functionName, force);
		registeredFunctions[functionName] = function;
	}

	public void RegisterFunction(IFunction function, bool force = false)
	{
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		RegisterFunction(function.Name, delegate(IExpression[] p, IDictionary<string, object> a)
		{
			function.Variables = a;
			return function.Evaluate(p, this);
		}, force);
	}

	public void RegisterOperator(IOperator op, bool force = false)
	{
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		foreach (string tag in op.Tags)
		{
			if (!force && registeredOperators.ContainsKey(tag))
			{
				throw new OperatorNameAlreadyRegisteredException(tag);
			}
			registeredOperators[tag] = op;
		}
	}

	public void UnregisterFunction(string functionName)
	{
		registeredFunctions.ContainsKey(functionName);
		registeredFunctions.Remove(functionName);
	}

	public void UnregisterOperator(string tag)
	{
		registeredOperators.ContainsKey(tag);
		registeredOperators.Remove(tag);
	}

	public virtual bool TryGetFunction(string functionName, out Func<IExpression[], IDictionary<string, object>, object> value)
	{
		return registeredFunctions.TryGetValue(functionName, out value);
	}

	internal bool TryGetOperator(string operatorName, out IOperator value)
	{
		return registeredOperators.TryGetValue(operatorName, out value);
	}

	private void CheckForExistingFunctionName(string functionName, bool force)
	{
		if (!force && registeredFunctions.ContainsKey(functionName))
		{
			throw new FunctionNameAlreadyRegisteredException(functionName);
		}
	}
}
