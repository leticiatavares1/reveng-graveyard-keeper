using System.Collections.Generic;
using LinqTools;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Collections/Lists")]
[ExposeAsDefinition]
public class GetLastListItem<T> : PureFunctionNode<T, IList<T>>
{
	public override T Invoke(IList<T> list)
	{
		return list.LastOrDefault();
	}
}
