using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("÷", 0)]
[Category("Logic Operators/Vector3")]
public class Vector3Divide : PureFunctionNode<Vector3, Vector3, float>
{
	public override Vector3 Invoke(Vector3 a, float b)
	{
		return a / b;
	}
}
