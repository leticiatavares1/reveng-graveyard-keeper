using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Can be used to set an internal variable, to later be retrieved with a 'Get Internal Var' node.")]
[ContextDefinedInputs(new Type[] { typeof(Wild) })]
[Name("Set Internal Var", 0)]
[Category("Variables/Internal")]
[ExposeAsDefinition]
public abstract class RelayValueInputBase : FlowNode
{
	public abstract Type relayType { get; }
}
