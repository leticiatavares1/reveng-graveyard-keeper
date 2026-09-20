namespace NGTools;

public interface IFieldModifier : IValueGetter
{
	string Name { get; }

	bool IsPublic { get; }

	void SetValue(object instance, object value);

	object GetValue(object instance);
}
