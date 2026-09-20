using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Log text in the console")]
[Category("Utility")]
public class LogText : CallableActionNode<string>
{
	public override void Invoke(string text)
	{
		Debug.Log(text);
	}
}
