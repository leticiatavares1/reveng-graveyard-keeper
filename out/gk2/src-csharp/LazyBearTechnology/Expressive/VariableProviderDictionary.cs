using System;
using System.Collections;
using System.Collections.Generic;

namespace Expressive;

internal class VariableProviderDictionary : IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
	private readonly IVariableProvider variableProvider;

	public int Count => ThrowNotSupported<int>();

	public bool IsReadOnly => ThrowNotSupported<bool>();

	public object this[string key]
	{
		get
		{
			return ThrowNotSupported<object>();
		}
		set
		{
			ThrowNotSupported<object>();
		}
	}

	public ICollection<string> Keys => ThrowNotSupported<ICollection<string>>();

	public ICollection<object> Values => ThrowNotSupported<ICollection<object>>();

	public VariableProviderDictionary(IVariableProvider variableProvider)
	{
		this.variableProvider = variableProvider;
	}

	public bool TryGetValue(string key, out object value)
	{
		return variableProvider.TryGetValue(key, out value);
	}

	public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
	{
		return ThrowNotSupported<IEnumerator<KeyValuePair<string, object>>>();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(KeyValuePair<string, object> item)
	{
		ThrowNotSupported<bool>();
	}

	public void Clear()
	{
		ThrowNotSupported<bool>();
	}

	public bool Contains(KeyValuePair<string, object> item)
	{
		return ThrowNotSupported<bool>();
	}

	public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
	{
		ThrowNotSupported<bool>();
	}

	public bool Remove(KeyValuePair<string, object> item)
	{
		return ThrowNotSupported<bool>();
	}

	public void Add(string key, object value)
	{
		ThrowNotSupported<bool>();
	}

	public bool ContainsKey(string key)
	{
		return ThrowNotSupported<bool>();
	}

	public bool Remove(string key)
	{
		return ThrowNotSupported<bool>();
	}

	private static TReturn ThrowNotSupported<TReturn>()
	{
		throw new NotSupportedException();
	}
}
