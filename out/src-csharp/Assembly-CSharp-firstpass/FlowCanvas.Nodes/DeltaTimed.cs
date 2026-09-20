using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Mutliply input value by Time.deltaTime and optional multiplier")]
[Category("Time")]
[Name("Per Second (Float)", 0)]
public class DeltaTimed : PureFunctionNode<float, float, float>
{
	public override float Invoke(float value, float multiplier = 1f)
	{
		return value * multiplier * Time.deltaTime;
	}
}
