using System;
using System.Collections.Generic;
using System.Threading;
using Expressive.Exceptions;
using Expressive.Expressions;
using Expressive.Functions;
using Expressive.Operators;

namespace Expressive;

public sealed class Expression : IExpression
{
	private IExpression compiledExpression;

	private readonly Context context;

	private readonly string originalExpression;

	private readonly ExpressionParser parser;

	private string[] referencedVariables;

	public IReadOnlyCollection<string> ReferencedVariables
	{
		get
		{
			CompileExpression();
			return (IReadOnlyCollection<string>)(object)referencedVariables;
		}
	}

	public Expression(string expression, ExpressiveOptions options = ExpressiveOptions.None)
		: this(expression, new Context(options))
	{
	}

	public Expression(string expression, Context context)
	{
		originalExpression = expression;
		this.context = context ?? throw new ArgumentNullException("context");
		parser = new ExpressionParser(this.context);
	}

	public object Evaluate(IDictionary<string, object> variables = null)
	{
		try
		{
			CompileExpression();
			return compiledExpression?.Evaluate(ApplyStringComparerSettings(variables, context.ParsingStringComparer));
		}
		catch (Exception innerException)
		{
			throw new ExpressiveException(innerException);
		}
	}

	public T Evaluate<T>(IDictionary<string, object> variables = null)
	{
		try
		{
			return (T)Evaluate(variables);
		}
		catch (ExpressiveException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new ExpressiveException(innerException);
		}
	}

	public object Evaluate(IVariableProvider variableProvider)
	{
		if (variableProvider == null)
		{
			throw new ArgumentNullException("variableProvider");
		}
		return Evaluate(new VariableProviderDictionary(variableProvider));
	}

	public T Evaluate<T>(IVariableProvider variableProvider)
	{
		if (variableProvider == null)
		{
			throw new ArgumentNullException("variableProvider");
		}
		try
		{
			return (T)Evaluate(variableProvider);
		}
		catch (ExpressiveException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new ExpressiveException(innerException);
		}
	}

	public void EvaluateAsync(Action<string, object> callback, IDictionary<string, object> variables = null)
	{
		this.EvaluateAsync<object>(callback, variables);
	}

	public void EvaluateAsync<T>(Action<string, T> callback, IDictionary<string, object> variables = null)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			T arg = default(T);
			string arg2 = null;
			try
			{
				arg = Evaluate<T>(variables);
			}
			catch (ExpressiveException ex)
			{
				arg2 = ex.Message;
			}
			callback(arg2, arg);
		});
	}

	public void RegisterFunction(string functionName, Func<IExpression[], IDictionary<string, object>, object> function)
	{
		context.RegisterFunction(functionName, function);
	}

	public void RegisterFunction(IFunction function)
	{
		context.RegisterFunction(function);
	}

	public void RegisterOperator(IOperator op, bool force = false)
	{
		context.RegisterOperator(op, force);
	}

	public void UnregisterFunction(string functionName)
	{
		context.UnregisterFunction(functionName);
	}

	public void UnregisterOperator(string tag)
	{
		context.UnregisterOperator(tag);
	}

	private void CompileExpression()
	{
		if (compiledExpression == null || context.Options.HasFlag(ExpressiveOptions.NoCache))
		{
			List<string> list = new List<string>();
			compiledExpression = parser.CompileExpression(originalExpression, list);
			referencedVariables = list.ToArray();
		}
	}

	private static IDictionary<string, object> ApplyStringComparerSettings(IDictionary<string, object> variables, IEqualityComparer<string> desiredStringComparer)
	{
		if (variables != null)
		{
			if (!(variables is Dictionary<string, object> dictionary))
			{
				if (variables is VariableProviderDictionary)
				{
					return variables;
				}
			}
			else if (dictionary.Comparer.Equals(desiredStringComparer))
			{
				return dictionary;
			}
			return new Dictionary<string, object>(variables, desiredStringComparer);
		}
		return null;
	}
}
