using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Get Internal Var", 0)]
[Description("Returns the selected and previously set Internal Variable's input value.")]
[ExposeAsDefinition]
[ContextDefinedOutputs(new Type[] { typeof(Wild) })]
[Category("Variables/Internal")]
[DoNotList]
public abstract class RelayValueOutputBase : FlowNode
{
	public abstract void SetSource(RelayValueInputBase source);
}
