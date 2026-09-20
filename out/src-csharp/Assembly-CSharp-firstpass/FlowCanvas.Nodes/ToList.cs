using System;
using System.Collections.Generic;
using LinqTools;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Utilities/Converters")]
[Obsolete]
public class ToList<T> : PureFunctionNode<List<T>, IList<T>>
{
	public override List<T> Invoke(IList<T> list)
	{
		return list.ToList();
	}
}
