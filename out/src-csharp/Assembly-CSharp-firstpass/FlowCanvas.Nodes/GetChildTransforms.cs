using LinqTools;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Unity")]
[Description("Get all child transforms of specified parent")]
public class GetChildTransforms : PureFunctionNode<Transform[], Transform>
{
	public override Transform[] Invoke(Transform parent)
	{
		return parent.Cast<Transform>().ToArray();
	}
}
