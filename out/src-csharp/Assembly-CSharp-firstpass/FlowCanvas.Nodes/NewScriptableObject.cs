using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Creates a new ScriptableObject instance")]
[Category("Unity")]
public class NewScriptableObject<T> : CallableFunctionNode<T> where T : ScriptableObject
{
	public override T Invoke()
	{
		return ScriptableObject.CreateInstance<T>();
	}
}
