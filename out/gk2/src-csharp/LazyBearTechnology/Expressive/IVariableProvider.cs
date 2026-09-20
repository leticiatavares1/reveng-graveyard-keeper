namespace Expressive;

public interface IVariableProvider
{
	bool TryGetValue(string variableName, out object value);
}
