using System;

namespace NGTools;

public interface IValueGetter
{
	Type Type { get; }

	T GetValue<T>(object instance);

	bool IsDefined(Type type, bool inherit);

	object[] GetCustomAttributes(Type type, bool inherit);
}
