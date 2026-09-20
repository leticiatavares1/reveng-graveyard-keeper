using System.Collections;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[ExposeAsDefinition]
[Category("Collections/Lists")]
public class GetListItemIndex : PureFunctionNode<int, IList, object>
{
	public override int Invoke(IList list, object item)
	{
		return list.IndexOf(item);
	}
}
