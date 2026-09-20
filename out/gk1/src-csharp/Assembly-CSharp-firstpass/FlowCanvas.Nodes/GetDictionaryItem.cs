using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[ExposeAsDefinition]
[Category("Collections/Dictionaries")]
public class GetDictionaryItem<T> : CallableFunctionNode<T, IDictionary<string, T>, string>
{
	public override T Invoke(IDictionary<string, T> dict, string key)
	{
		return dict[key];
	}
}
