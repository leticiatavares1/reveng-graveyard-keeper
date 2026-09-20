using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("-", 0)]
[Category("Logic Operators/Vector3")]
public class Vector3Subtract : PureFunctionNode<Vector3, Vector3, Vector3>
{
	public override Vector3 Invoke(Vector3 a, Vector3 b)
	{
		return a - b;
	}
}
