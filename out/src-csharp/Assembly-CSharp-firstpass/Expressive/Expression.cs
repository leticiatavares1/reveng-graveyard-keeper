using System;
using System.Collections.Generic;
using System.Threading;
using Expressive.Exceptions;
using Expressive.Expressions;
using Expressive.Functions;

namespace Expressive;

public sealed class Expression
{
	private IExpression _compiledExpression;

	private readonly ExpressiveOptions _options;

	private readonly string _originalExpression;

	private readonly ExpressionParser _parser;

	private string[] _variables;

	public string[] ReferencedVariables
	{
		get
		{
			CompileExpression();
			return _variables;
		}
	}

	public Expression(string expression)
		: this(expression, ExpressiveOptions.None)
	{
	}

	public Expression(string expression, ExpressiveOptions options)
	{
		_originalExpression = expression;
		_options = options;
		_parser = new ExpressionParser(_options);
	}

	public object Evaluate()
	{
		return Evaluate(null);
	}

	public object Evaluate(IDictionary<string, object> variables)
	{
		try
		{
			CompileExpression();
			if (variables != null && _options.HasFlag(ExpressiveOptions.IgnoreCase))
			{
				variables = new Dictionary<string, object>(variables, StringComparer.OrdinalIgnoreCase);
			}
			return (_compiledExpression == null) ? null : _compiledExpression.Evaluate(variables);
		}
		catch (Exception innerException)
		{
			throw new ExpressiveException(innerException);
		}
	}

	public void EvaluateAsync(Action<string, object> callback)
	{
		EvaluateAsync(callback, null);
	}

	public void EvaluateAsync(Action<string, object> callback, IDictionary<string, object> variables)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			object arg = null;
			string arg2 = null;
			try
			{
				arg = Evaluate(variables);
			}
			catch (Exception ex)
			{
				arg2 = ex.Message;
			}
			if (callback != null)
			{
				callback(arg2, arg);
			}
		});
	}

	public void RegisterFunction(string functionName, Func<IExpression[], IDictionary<string, object>, object> function)
	{
		_parser.RegisterFunction(functionName, function);
	}

	public void RegisterFunction(IFunction function)
	{
		_parser.RegisterFunction(function);
	}

	private void CompileExpression()
	{
		if (_compiledExpression == null || _options.HasFlag(ExpressiveOptions.NoCache))
		{
			List<string> list = new List<string>();
			_compiledExpression = _parser.CompileExpression(_originalExpression, list);
			_variables = list.ToArray();
		}
	}
}
